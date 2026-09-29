using System.Text;
using MotelPalace.Data;
using MotelPalace.UI;
using MotelPalace.Utils;

Console.OutputEncoding = Encoding.UTF8;

// Relógio simulado: permite "avançar o tempo" para testar hora extra, feriados etc.
var relogio = new RelogioSimulado(DateTime.Now);
var sistema = new MotelSistema(relogio);
DadosIniciais.Popular(sistema);

if (args.Contains("--demo"))
    new Demonstracao(sistema, relogio).Executar();
else
    new MenuPrincipal(sistema, relogio).Executar();
