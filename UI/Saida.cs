using MotelPalace.Dtos;
using MotelPalace.Utils;

namespace MotelPalace.UI;

/// <summary>Helpers de saída colorida no console.</summary>
public static class Saida
{
    public static void Titulo(string texto)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine($"══ {texto} " + new string('═', Math.Max(4, 60 - texto.Length)));
        Console.ResetColor();
    }

    public static void Ok(string texto) => Escrever(ConsoleColor.Green, "✔ " + texto);
    public static void Erro(string texto) => Escrever(ConsoleColor.Red, "✘ " + texto);
    public static void Info(string texto) => Escrever(ConsoleColor.Cyan, "• " + texto);

    private static void Escrever(ConsoleColor cor, string texto)
    {
        Console.ForegroundColor = cor;
        Console.WriteLine(texto);
        Console.ResetColor();
    }

    public static void Conta(ContaReserva c)
    {
        Console.WriteLine($"  Conta da reserva #{c.ReservaId} em {Formatar.DataHora(c.Momento)}");
        Console.WriteLine($"    Hospedagem : {Formatar.Moeda(c.Hospedagem)}");
        Console.WriteLine($"    Hora extra : {Formatar.Moeda(c.HoraExtra)} ({c.HorasExtras}h)");
        Console.WriteLine($"    Consumo    : {Formatar.Moeda(c.Consumo)}");
        Console.WriteLine($"    TOTAL      : {Formatar.Moeda(c.Total)}");
        Console.WriteLine($"    Pago       : {Formatar.Moeda(c.Pago)}");
        Console.WriteLine($"    SALDO      : {Formatar.Moeda(c.Saldo)}");
    }
}
