using System.Globalization;

namespace MotelPalace.Utils;

public static class Formatar
{
    private static readonly CultureInfo PtBr = CriarCultura();

    private static CultureInfo CriarCultura()
    {
        try { return new CultureInfo("pt-BR"); }
        catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
    }

    public static string Moeda(decimal valor) => "R$ " + valor.ToString("N2", PtBr);

    public static string DataHora(DateTime data) =>
        data.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    public static string Cpf(string cpf)
    {
        var n = Validacoes.SomenteDigitos(cpf);
        return n.Length == 11
            ? $"{n[..3]}.{n[3..6]}.{n[6..9]}-{n[9..]}"
            : cpf;
    }

    public static string Telefone(string telefone)
    {
        var n = Validacoes.SomenteDigitos(telefone);
        return n.Length switch
        {
            11 => $"({n[..2]}) {n[2..7]}-{n[7..]}",
            10 => $"({n[..2]}) {n[2..6]}-{n[6..]}",
            _ => telefone
        };
    }
}
