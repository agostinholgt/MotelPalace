using MotelPalace.Dtos;
using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;
using MotelPalace.Utils;

namespace MotelPalace.Services;

/// <summary>
/// Coração do sistema. Concentra as regras de negócio:
/// 1) bloqueia reserva em suíte ocupada/manutenção/limpeza;
/// 2) calcula valor pela tarifa (tipo de suíte + período + tipo de dia) e soma o consumo;
/// 3) cobra hora extra após a saída prevista (com tolerância);
/// 4) baixa o estoque ao lançar consumo;
/// 5) ao finalizar, a suíte vai automaticamente para "Limpeza".
/// </summary>
public class ReservaService
{
    /// <summary>Tolerância antes de começar a cobrar hora extra.</summary>
    public static readonly TimeSpan ToleranciaHoraExtra = TimeSpan.FromMinutes(10);

    private readonly IRepositorio<Reserva> _repo;
    private readonly IRepositorio<Pagamento> _pagamentos;
    private readonly ClienteService _clientes;
    private readonly SuiteService _suites;
    private readonly ConsumoItemService _itens;
    private readonly TarifaService _tarifas;
    private readonly CalendarioService _calendario;
    private readonly IRelogio _relogio;

    public ReservaService(
        IRepositorio<Reserva> repo,
        IRepositorio<Pagamento> pagamentos,
        ClienteService clientes,
        SuiteService suites,
        ConsumoItemService itens,
        TarifaService tarifas,
        CalendarioService calendario,
        IRelogio relogio)
    {
        _repo = repo;
        _pagamentos = pagamentos;
        _clientes = clientes;
        _suites = suites;
        _itens = itens;
        _tarifas = tarifas;
        _calendario = calendario;
        _relogio = relogio;
    }

    // ------------------------------------------------------------------
    // Criação
    // ------------------------------------------------------------------

    public Reserva Criar(int clienteId, int suiteId, PeriodoHospedagem periodo)
    {
        var cliente = _clientes.ObterPorId(clienteId);
        var suite = _suites.ObterPorId(suiteId);

        GarantirSuiteReservavel(suite);

        if (_repo.Buscar(r => r.ClienteId == cliente.Id && r.Status == StatusReserva.Ativa).Any())
            throw new RegraDeNegocioException($"O cliente {cliente.Nome} já possui uma hospedagem ativa.");

        var entrada = _relogio.Agora;
        var tipoDia = _calendario.ObterTipoDia(entrada);
        var tarifa = _tarifas.Obter(suite.Tipo, periodo, tipoDia);

        var reserva = new Reserva
        {
            ClienteId = cliente.Id,
            SuiteId = suite.Id,
            DataEntrada = entrada,
            DataSaidaPrevista = entrada + periodo.Duracao(),
            Periodo = periodo,
            TipoDiaAplicado = tipoDia,
            ValorHospedagem = tarifa.Valor,
            Status = StatusReserva.Ativa
        };

        _repo.Adicionar(reserva);
        _suites.MarcarOcupada(suite.Id);
        return reserva;
    }

    private void GarantirSuiteReservavel(Suite suite)
    {
        switch (suite.Status)
        {
            case StatusSuite.Ocupada:
                throw new RegraDeNegocioException($"A suíte {suite.Numero} está ocupada.");
            case StatusSuite.Manutencao:
                throw new RegraDeNegocioException($"A suíte {suite.Numero} está em manutenção.");
            case StatusSuite.Limpeza:
                throw new RegraDeNegocioException($"A suíte {suite.Numero} está em limpeza.");
        }

        // Defesa extra: nunca permitir duas reservas ativas na mesma suíte.
        if (_repo.Buscar(r => r.SuiteId == suite.Id && r.Status == StatusReserva.Ativa).Any())
            throw new RegraDeNegocioException($"A suíte {suite.Numero} já possui uma reserva ativa.");
    }

    // ------------------------------------------------------------------
    // Consumo (frigobar / cardápio)
    // ------------------------------------------------------------------

    public ReservaConsumo LancarConsumo(int reservaId, int itemId, int quantidade)
    {
        var reserva = ObterPorId(reservaId);
        if (reserva.Status != StatusReserva.Ativa)
            throw new RegraDeNegocioException("Só é possível lançar consumo em reservas ativas.");

        var item = _itens.ObterPorId(itemId);

        // Baixa o estoque (lança exceção se insuficiente, antes de alterar a reserva).
        item.BaixarEstoque(quantidade);

        var existente = reserva.Consumos
            .FirstOrDefault(c => c.ItemId == item.Id && c.PrecoUnitario == item.Preco);

        if (existente is not null)
        {
            existente.Quantidade += quantidade;
            return existente;
        }

        var lancamento = new ReservaConsumo
        {
            ReservaId = reserva.Id,
            ItemId = item.Id,
            NomeItem = item.Nome,
            PrecoUnitario = item.Preco,
            Quantidade = quantidade,
            DataLancamento = _relogio.Agora
        };
        reserva.Consumos.Add(lancamento);
        return lancamento;
    }

    // ------------------------------------------------------------------
    // Conta / hora extra
    // ------------------------------------------------------------------

    /// <summary>Calcula horas extras e valor devido no instante informado.</summary>
    public (int Horas, decimal Valor) CalcularHoraExtra(Reserva reserva, DateTime momento)
    {
        if (momento <= reserva.DataSaidaPrevista) return (0, 0m);

        var excesso = momento - reserva.DataSaidaPrevista;
        if (excesso <= ToleranciaHoraExtra) return (0, 0m);

        // Cada hora iniciada é cobrada por inteiro.
        int horas = (int)Math.Ceiling(excesso.TotalHours);

        var suite = _suites.ObterPorId(reserva.SuiteId);
        var tarifa = _tarifas.Obter(suite.Tipo, reserva.Periodo, reserva.TipoDiaAplicado);
        return (horas, horas * tarifa.ValorHoraExtra);
    }

    public ContaReserva ObterConta(int reservaId, DateTime? momento = null)
    {
        var r = ObterPorId(reservaId);

        int horasExtras;
        decimal valorExtra;
        DateTime instante;

        if (r.Status == StatusReserva.Ativa)
        {
            instante = momento ?? _relogio.Agora;
            (horasExtras, valorExtra) = CalcularHoraExtra(r, instante);
        }
        else
        {
            instante = r.DataSaidaReal ?? momento ?? _relogio.Agora;
            horasExtras = r.HorasExtras;
            valorExtra = r.ValorHoraExtra;
        }

        var consumo = r.ValorConsumo;
        var total = r.ValorHospedagem + valorExtra + consumo;
        var pago = TotalPago(r.Id);

        return new ContaReserva(r.Id, instante, r.ValorHospedagem, horasExtras, valorExtra,
                                consumo, total, pago, total - pago);
    }

    public decimal TotalPago(int reservaId) =>
        _pagamentos
            .Buscar(p => p.ReservaId == reservaId && p.Status == StatusPagamento.Confirmado)
            .Sum(p => p.Valor);

    // ------------------------------------------------------------------
    // Encerramento
    // ------------------------------------------------------------------

    public Reserva Finalizar(int reservaId)
    {
        var reserva = ObterPorId(reservaId);
        if (reserva.Status != StatusReserva.Ativa)
            throw new RegraDeNegocioException($"A reserva #{reserva.Id} não está ativa (status: {reserva.Status}).");

        var agora = _relogio.Agora;
        var conta = ObterConta(reservaId, agora);

        if (conta.Saldo > 0)
            throw new RegraDeNegocioException(
                $"Conta não quitada: saldo pendente de {Formatar.Moeda(conta.Saldo)}. Registre o pagamento antes de finalizar.");

        reserva.HorasExtras = conta.HorasExtras;
        reserva.ValorHoraExtra = conta.HoraExtra;
        reserva.DataSaidaReal = agora;
        reserva.Status = StatusReserva.Finalizada;

        // Regra: ao finalizar, a suíte é liberada automaticamente para limpeza.
        _suites.MarcarParaLimpeza(reserva.SuiteId);
        return reserva;
    }

    public Reserva Cancelar(int reservaId)
    {
        var reserva = ObterPorId(reservaId);
        if (reserva.Status != StatusReserva.Ativa)
            throw new RegraDeNegocioException($"Só é possível cancelar reservas ativas (status atual: {reserva.Status}).");

        if (reserva.Consumos.Count > 0)
            throw new RegraDeNegocioException(
                "Não é possível cancelar: já existe consumo lançado. Finalize a hospedagem.");

        // Estorna pagamentos já recebidos.
        foreach (var pagamento in _pagamentos.Buscar(p => p.ReservaId == reservaId && p.Status == StatusPagamento.Confirmado))
            pagamento.Status = StatusPagamento.Estornado;

        reserva.Status = StatusReserva.Cancelada;
        _suites.LiberarSemUso(reserva.SuiteId);
        return reserva;
    }

    // ------------------------------------------------------------------
    // Consultas
    // ------------------------------------------------------------------

    public Reserva ObterPorId(int id) =>
        _repo.ObterPorId(id) ?? throw new EntidadeNaoEncontradaException(nameof(Reserva), id);

    public IReadOnlyList<Reserva> Listar() => _repo.ListarTodos();

    public IEnumerable<Reserva> ListarAtivas() => _repo.Buscar(r => r.Status == StatusReserva.Ativa);

    public IEnumerable<Reserva> HistoricoDoCliente(int clienteId) =>
        _repo.Buscar(r => r.ClienteId == clienteId);

    public IEnumerable<Reserva> ListarComSaidaAtrasada() =>
        _repo.Buscar(r => r.Status == StatusReserva.Ativa
                          && _relogio.Agora - r.DataSaidaPrevista > ToleranciaHoraExtra);
}
