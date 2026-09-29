using MotelPalace.Data;
using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Utils;

namespace MotelPalace.UI;

/// <summary>
/// Roteiro automático (dotnet run -- --demo) que exercita TODAS as regras de negócio,
/// incluindo os casos em que o sistema precisa bloquear a operação.
/// </summary>
public class Demonstracao
{
    private readonly MotelSistema _s;
    private readonly RelogioSimulado _relogio;

    public Demonstracao(MotelSistema sistema, RelogioSimulado relogio)
    {
        _s = sistema;
        _relogio = relogio;
    }

    public void Executar()
    {
        // Sábado, 10/10/2026, 21h.
        _relogio.Definir(new DateTime(2026, 10, 10, 21, 0, 0));

        var joao = _s.Clientes.ObterPorCpf("52998224725");
        var maria = _s.Clientes.ObterPorCpf("11144477735");

        // ---------------------------------------------------------------
        Saida.Titulo("1) CHECK-IN EM UM SÁBADO À NOITE");
        var agora = _relogio.Agora;
        Saida.Info($"Agora: {Formatar.DataHora(agora)} — tipo de dia: {_s.Calendario.ObterTipoDia(agora)}");

        var suite201 = _s.Suites.ObterPorNumero(201);
        var reserva = _s.Reservas.Criar(joao.Id, suite201.Id, PeriodoHospedagem.SeisHoras);
        Saida.Ok($"João entrou na suíte 201 (Luxo, 6h). Tarifa aplicada: {Formatar.Moeda(reserva.ValorHospedagem)}. " +
                 $"Saída prevista: {Formatar.DataHora(reserva.DataSaidaPrevista)}");
        Saida.Info($"Status da suíte 201: {_s.Suites.ObterPorId(suite201.Id).Status}");

        // ---------------------------------------------------------------
        Saida.Titulo("2) REGRAS DE BLOQUEIO DE RESERVA");
        Tentar("Maria tenta reservar a suíte 201 (ocupada)",
            () => _s.Reservas.Criar(maria.Id, suite201.Id, PeriodoHospedagem.TresHoras));

        Tentar("Maria tenta reservar a suíte 104 (manutenção)",
            () => _s.Reservas.Criar(maria.Id, _s.Suites.ObterPorNumero(104).Id, PeriodoHospedagem.TresHoras));

        Tentar("João tenta abrir uma segunda hospedagem enquanto a primeira está ativa",
            () => _s.Reservas.Criar(joao.Id, _s.Suites.ObterPorNumero(202).Id, PeriodoHospedagem.TresHoras));

        // ---------------------------------------------------------------
        Saida.Titulo("3) CONSUMO COM BAIXA AUTOMÁTICA DE ESTOQUE");
        var cerveja = _s.Itens.Listar().First(i => i.Nome.StartsWith("Cerveja"));
        var batata = _s.Itens.Listar().First(i => i.Nome.StartsWith("Porção"));
        var espumante = _s.Itens.Listar().First(i => i.Nome == "Espumante");

        Saida.Info($"Estoque antes  → {cerveja.Nome}: {cerveja.Estoque} | {batata.Nome}: {batata.Estoque}");
        _s.Reservas.LancarConsumo(reserva.Id, cerveja.Id, 4);
        _s.Reservas.LancarConsumo(reserva.Id, batata.Id, 1);
        Saida.Ok("Lançado: 4x cerveja e 1x porção de batata.");
        Saida.Info($"Estoque depois → {cerveja.Nome}: {cerveja.Estoque} | {batata.Nome}: {batata.Estoque}");

        Tentar($"Pedido de 10 espumantes (estoque: {espumante.Estoque})",
            () => _s.Reservas.LancarConsumo(reserva.Id, espumante.Id, 10));

        // ---------------------------------------------------------------
        Saida.Titulo("4) HORA EXTRA");
        _relogio.Avancar(TimeSpan.FromHours(6) + TimeSpan.FromMinutes(5));
        Saida.Info($"Agora: {Formatar.DataHora(_relogio.Agora)} — 5 min após a saída prevista (dentro da tolerância de 10 min).");
        Saida.Conta(_s.Reservas.ObterConta(reserva.Id));

        _relogio.Avancar(TimeSpan.FromMinutes(40));
        Saida.Info($"Agora: {Formatar.DataHora(_relogio.Agora)} — 45 min de atraso → 1 hora extra iniciada.");
        Saida.Conta(_s.Reservas.ObterConta(reserva.Id));

        // ---------------------------------------------------------------
        Saida.Titulo("5) PAGAMENTO, CHECK-OUT E LIMPEZA AUTOMÁTICA");
        var p1 = _s.Pagamentos.Registrar(reserva.Id, 100m, FormaPagamento.Pix);
        Saida.Ok($"Pagamento parcial de {Formatar.Moeda(p1.Valor)} via {p1.Forma}.");

        Tentar("Tentativa de check-out com saldo pendente", () => _s.Reservas.Finalizar(reserva.Id));
        Tentar("Tentativa de pagar mais que o saldo devedor",
            () => _s.Pagamentos.Registrar(reserva.Id, 9999m, FormaPagamento.Dinheiro));

        var p2 = _s.Pagamentos.QuitarSaldo(reserva.Id, FormaPagamento.CartaoCredito);
        Saida.Ok($"Saldo restante quitado: {Formatar.Moeda(p2.Valor)} via {p2.Forma}.");

        var finalizada = _s.Reservas.Finalizar(reserva.Id);
        var suiteAposCheckout = _s.Suites.ObterPorId(suite201.Id);
        Saida.Ok($"Hospedagem #{finalizada.Id} finalizada. Total: {Formatar.Moeda(finalizada.ValorTotal)} " +
                 $"(hospedagem {Formatar.Moeda(finalizada.ValorHospedagem)} + hora extra {Formatar.Moeda(finalizada.ValorHoraExtra)} " +
                 $"+ consumo {Formatar.Moeda(finalizada.ValorConsumo)}).");
        Saida.Info($"Status da suíte 201 após o check-out: {suiteAposCheckout.Status}");

        Tentar("Maria tenta reservar a suíte 201 enquanto está em limpeza",
            () => _s.Reservas.Criar(maria.Id, suite201.Id, PeriodoHospedagem.TresHoras));

        _s.Suites.ConcluirLimpeza(suite201.Id);
        Saida.Ok($"Camareira concluiu a limpeza. Suíte 201: {_s.Suites.ObterPorId(suite201.Id).Status}");

        // ---------------------------------------------------------------
        Saida.Titulo("6) FERIADO: TARIFA ESPECIAL E CANCELAMENTO");
        _relogio.Definir(new DateTime(2026, 10, 12, 14, 0, 0));
        Saida.Info($"Agora: {Formatar.DataHora(_relogio.Agora)} — {_s.Calendario.NomeDoFeriado(_relogio.Agora)} " +
                   $"(tipo de dia: {_s.Calendario.ObterTipoDia(_relogio.Agora)})");

        var suite301 = _s.Suites.ObterPorNumero(301);
        var reservaMaria = _s.Reservas.Criar(maria.Id, suite301.Id, PeriodoHospedagem.Pernoite);
        Saida.Ok($"Maria reservou a suíte temática 301 (pernoite): {Formatar.Moeda(reservaMaria.ValorHospedagem)}");

        _s.Pagamentos.Registrar(reservaMaria.Id, 200m, FormaPagamento.Pix);
        Saida.Info("Maria pagou R$ 200,00 de sinal via Pix.");

        _s.Reservas.Cancelar(reservaMaria.Id);
        Saida.Ok($"Reserva cancelada. Sinal estornado. Suíte 301: {_s.Suites.ObterPorId(suite301.Id).Status}");

        // ---------------------------------------------------------------
        Saida.Titulo("7) TABELA DE TARIFAS — SUÍTE LUXO, 6 HORAS");
        foreach (var dia in Enum.GetValues<TipoDia>())
        {
            var t = _s.Tarifas.Obter(TipoSuite.Luxo, PeriodoHospedagem.SeisHoras, dia);
            Console.WriteLine($"  {dia,-12} {Formatar.Moeda(t.Valor),10}   hora extra: {Formatar.Moeda(t.ValorHoraExtra)}");
        }

        // ---------------------------------------------------------------
        Saida.Titulo("8) RELATÓRIO GERENCIAL");
        Console.WriteLine(_s.Relatorios.GerarResumoGerencial());
    }

    private static void Tentar(string descricao, Action acao)
    {
        Saida.Info(descricao);
        try
        {
            acao();
            Saida.Ok("Executado.");
        }
        catch (RegraDeNegocioException ex)
        {
            Saida.Erro("Bloqueado pela regra de negócio → " + ex.Message);
        }
    }
}
