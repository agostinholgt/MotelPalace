using MotelPalace.Enums;

namespace MotelPalace.Models;

/// <summary>Classe 3 — Reserva/Hospedagem. Classe central: liga Cliente e Suíte.</summary>
public class Reserva : Entidade
{
    public int ClienteId { get; set; }
    public int SuiteId { get; set; }

    public DateTime DataEntrada { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public DateTime? DataSaidaReal { get; set; }

    public PeriodoHospedagem Periodo { get; set; }
    /// <summary>Tipo de dia usado no cálculo da tarifa (fixado na entrada).</summary>
    public TipoDia TipoDiaAplicado { get; set; }

    public decimal ValorHospedagem { get; set; }
    public int HorasExtras { get; set; }
    public decimal ValorHoraExtra { get; set; }

    public StatusReserva Status { get; set; } = StatusReserva.Ativa;

    /// <summary>Itens consumidos (lado N:N via ReservaConsumo).</summary>
    public List<ReservaConsumo> Consumos { get; } = new();

    public decimal ValorConsumo => Consumos.Sum(c => c.Subtotal);
    public decimal ValorTotal => ValorHospedagem + ValorHoraExtra + ValorConsumo;
}
