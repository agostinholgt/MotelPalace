using System.Text.RegularExpressions;

namespace MotelPalace.Utils;

public static class Validacoes
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static bool EmailValido(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    public static string SomenteDigitos(string? valor) =>
        new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Telefone brasileiro com DDD: 10 ou 11 dígitos.</summary>
    public static bool TelefoneValido(string? telefone)
    {
        var n = SomenteDigitos(telefone).Length;
        return n is 10 or 11;
    }
}
