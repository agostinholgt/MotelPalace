using MotelPalace.Enums;

namespace MotelPalace.Services;

/// <summary>
/// Classifica datas em dia útil, fim de semana ou feriado (nacionais fixos e móveis).
/// Regra do Palace: a partir das 18h de sexta-feira já vale a tarifa de fim de semana.
/// </summary>
public class CalendarioService
{
    private readonly Dictionary<int, Dictionary<DateTime, string>> _cache = new();

    public TipoDia ObterTipoDia(DateTime momento)
    {
        if (EhFeriado(momento)) return TipoDia.Feriado;

        return momento.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => TipoDia.FimDeSemana,
            DayOfWeek.Friday when momento.Hour >= 18 => TipoDia.FimDeSemana,
            _ => TipoDia.DiaUtil
        };
    }

    public bool EhFeriado(DateTime data) => ObterFeriados(data.Year).ContainsKey(data.Date);

    public string? NomeDoFeriado(DateTime data) =>
        ObterFeriados(data.Year).TryGetValue(data.Date, out var nome) ? nome : null;

    public IReadOnlyDictionary<DateTime, string> ObterFeriados(int ano)
    {
        if (_cache.TryGetValue(ano, out var existente)) return existente;

        var pascoa = CalcularPascoa(ano);
        var feriados = new Dictionary<DateTime, string>
        {
            [new DateTime(ano, 1, 1)] = "Confraternização Universal",
            [new DateTime(ano, 4, 21)] = "Tiradentes",
            [new DateTime(ano, 5, 1)] = "Dia do Trabalho",
            [new DateTime(ano, 9, 7)] = "Independência do Brasil",
            [new DateTime(ano, 10, 12)] = "Nossa Senhora Aparecida",
            [new DateTime(ano, 11, 2)] = "Finados",
            [new DateTime(ano, 11, 15)] = "Proclamação da República",
            [new DateTime(ano, 11, 20)] = "Consciência Negra",
            [new DateTime(ano, 12, 25)] = "Natal",
            [pascoa.AddDays(-48)] = "Segunda de Carnaval",
            [pascoa.AddDays(-47)] = "Terça de Carnaval",
            [pascoa.AddDays(-2)] = "Sexta-feira Santa",
            [pascoa.AddDays(60)] = "Corpus Christi"
        };

        _cache[ano] = feriados;
        return feriados;
    }

    /// <summary>Algoritmo de Meeus/Jones/Butcher para o domingo de Páscoa.</summary>
    public static DateTime CalcularPascoa(int ano)
    {
        int a = ano % 19;
        int b = ano / 100;
        int c = ano % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int mes = (h + l - 7 * m + 114) / 31;
        int dia = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateTime(ano, mes, dia);
    }
}
