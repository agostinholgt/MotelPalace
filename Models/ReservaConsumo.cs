namespace MotelPalace.Models;

/// <summary>
/// Tabela de ligação N:N entre Reserva e ConsumoItem.
/// Guarda o preço unitário no momento do lançamento (histórico não muda se o preço mudar).
/// </summary>
public class ReservaConsumo
{
    public int ReservaId { get; set; }
    public int ItemId { get; set; }
    public string NomeItem { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public DateTime DataLancamento { get; set; }

    public decimal Subtotal => PrecoUnitario * Quantidade;
}
