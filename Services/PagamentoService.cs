using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;
using MotelPalace.Utils;

namespace MotelPalace.Services;

public class PagamentoService
{
    private readonly IRepositorio<Pagamento> _repo;
    private readonly ReservaService _reservas;
    private readonly IRelogio _relogio;

    public PagamentoService(IRepositorio<Pagamento> repo, ReservaService reservas, IRelogio relogio)
    {
        _repo = repo;
        _reservas = reservas;
        _relogio = relogio;
    }

    /// <summary>Registra um pagamento (parcial ou total). Não aceita valor acima do saldo.</summary>
    public Pagamento Registrar(int reservaId, decimal valor, FormaPagamento forma)
    {
        var reserva = _reservas.ObterPorId(reservaId);
        if (reserva.Status == StatusReserva.Cancelada)
            throw new RegraDeNegocioException("Não é possível receber pagamento de uma reserva cancelada.");
        if (valor <= 0)
            throw new RegraDeNegocioException("O valor do pagamento deve ser maior que zero.");

        var conta = _reservas.ObterConta(reservaId);
        if (conta.Saldo <= 0)
            throw new RegraDeNegocioException("Esta reserva já está quitada.");
        if (valor > conta.Saldo)
            throw new RegraDeNegocioException(
                $"O valor {Formatar.Moeda(valor)} excede o saldo devedor de {Formatar.Moeda(conta.Saldo)}.");

        return _repo.Adicionar(new Pagamento
        {
            ReservaId = reservaId,
            Valor = valor,
            Forma = forma,
            DataPagamento = _relogio.Agora,
            Status = StatusPagamento.Confirmado
        });
    }

    /// <summary>Quita todo o saldo restante de uma vez, em uma única forma de pagamento.</summary>
    public Pagamento QuitarSaldo(int reservaId, FormaPagamento forma)
    {
        var conta = _reservas.ObterConta(reservaId);
        if (conta.Saldo <= 0)
            throw new RegraDeNegocioException("Esta reserva já está quitada.");
        return Registrar(reservaId, conta.Saldo, forma);
    }

    public Pagamento Estornar(int pagamentoId)
    {
        var pagamento = _repo.ObterPorId(pagamentoId)
                        ?? throw new EntidadeNaoEncontradaException(nameof(Pagamento), pagamentoId);
        if (pagamento.Status != StatusPagamento.Confirmado)
            throw new RegraDeNegocioException("Somente pagamentos confirmados podem ser estornados.");

        pagamento.Status = StatusPagamento.Estornado;
        return pagamento;
    }

    public IReadOnlyList<Pagamento> Listar() => _repo.ListarTodos();

    public IEnumerable<Pagamento> ListarPorReserva(int reservaId) =>
        _repo.Buscar(p => p.ReservaId == reservaId);
}
