using MotelPalace.Enums;

namespace MotelPalace.Models;

/// <summary>Classe 6 — Pagamento. Uma reserva pode ter vários (pagamento dividido).</summary>
public class Pagamento : Entidade
{
    public int ReservaId { get; set; }
    public decimal Valor { get; set; }
    public FormaPagamento Forma { get; set; }
    public DateTime DataPagamento { get; set; }
    public StatusPagamento Status { get; set; } = StatusPagamento.Confirmado;
}
