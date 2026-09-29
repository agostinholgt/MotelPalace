namespace MotelPalace.Enums;

/// <summary>Períodos de permanência oferecidos pelo Motel Palace.</summary>
public enum PeriodoHospedagem
{
    TresHoras,
    SeisHoras,
    DozeHoras,
    Pernoite
}

public static class PeriodoHospedagemExtensions
{
    /// <summary>Duração contratada do período.</summary>
    public static TimeSpan Duracao(this PeriodoHospedagem periodo) => periodo switch
    {
        PeriodoHospedagem.TresHoras => TimeSpan.FromHours(3),
        PeriodoHospedagem.SeisHoras => TimeSpan.FromHours(6),
        PeriodoHospedagem.DozeHoras => TimeSpan.FromHours(12),
        PeriodoHospedagem.Pernoite => TimeSpan.FromHours(14),
        _ => throw new ArgumentOutOfRangeException(nameof(periodo), periodo, "Período desconhecido.")
    };

    public static string Descricao(this PeriodoHospedagem periodo) => periodo switch
    {
        PeriodoHospedagem.TresHoras => "3 horas",
        PeriodoHospedagem.SeisHoras => "6 horas",
        PeriodoHospedagem.DozeHoras => "12 horas",
        PeriodoHospedagem.Pernoite => "Pernoite (14h)",
        _ => periodo.ToString()
    };
}
