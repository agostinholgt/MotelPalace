using System.Globalization;

namespace MotelPalace.UI;

/// <summary>Leitura segura de dados do console.</summary>
public static class Entrada
{
    public static string Texto(string rotulo)
    {
        Console.Write($"{rotulo}: ");
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    public static int Inteiro(string rotulo)
    {
        while (true)
        {
            var t = Texto(rotulo);
            if (int.TryParse(t, out var v)) return v;
            Console.WriteLine("Valor inválido: digite um número inteiro.");
        }
    }

    public static decimal Decimal(string rotulo)
    {
        while (true)
        {
            var t = Texto(rotulo).Replace(',', '.');
            if (decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var v)) return v;
            Console.WriteLine("Valor inválido: digite um número (ex.: 49,90).");
        }
    }

    public static T Opcao<T>(string rotulo) where T : struct, Enum
    {
        var valores = Enum.GetValues<T>();
        Console.WriteLine($"{rotulo}:");
        for (int i = 0; i < valores.Length; i++)
            Console.WriteLine($"  {i + 1}) {valores[i]}");

        while (true)
        {
            var n = Inteiro("Escolha");
            if (n >= 1 && n <= valores.Length) return valores[n - 1];
            Console.WriteLine("Opção inválida.");
        }
    }

    public static void Pausa()
    {
        Console.Write("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }
}
