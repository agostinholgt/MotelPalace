using MotelPalace.Enums;

namespace MotelPalace.Models;

/// <summary>Classe 4 — Tarifa: preço por tipo de suíte, período e tipo de dia.</summary>
public class Tarifa : Entidade
{
    public TipoSuite TipoSuite { get; set; }
    public PeriodoHospedagem Periodo { get; set; }
    public TipoDia TipoDia { get; set; }
    public decimal Valor { get; set; }
    /// <summary>Valor cobrado por cada hora extra iniciada após a saída prevista.</summary>
    public decimal ValorHoraExtra { get; set; }
}
