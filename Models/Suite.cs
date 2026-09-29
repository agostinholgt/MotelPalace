using MotelPalace.Enums;

namespace MotelPalace.Models;

/// <summary>Classe 2 — Suíte. O tipo já vem como atributo (enum TipoSuite).</summary>
public class Suite : Entidade
{
    public int Numero { get; set; }
    public TipoSuite Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public StatusSuite Status { get; set; } = StatusSuite.Disponivel;

    public override string ToString() =>
        $"Suíte {Numero} [{Tipo}] cap. {Capacidade} - {Status} - {Descricao}";
}
