namespace MotelPalace.Utils;

public static class CpfValidador
{
    public static string Normalizar(string? valor) =>
        new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Valida os dígitos verificadores do CPF.</summary>
    public static bool Validar(string? cpf)
    {
        var numeros = Normalizar(cpf);
        if (numeros.Length != 11) return false;
        if (numeros.Distinct().Count() == 1) return false;

        var d = numeros.Select(c => c - '0').ToArray();

        int soma1 = 0;
        for (int i = 0; i < 9; i++) soma1 += d[i] * (10 - i);
        int resto1 = soma1 % 11;
        int dv1 = resto1 < 2 ? 0 : 11 - resto1;

        int soma2 = 0;
        for (int i = 0; i < 10; i++) soma2 += d[i] * (11 - i);
        int resto2 = soma2 % 11;
        int dv2 = resto2 < 2 ? 0 : 11 - resto2;

        return d[9] == dv1 && d[10] == dv2;
    }
}
