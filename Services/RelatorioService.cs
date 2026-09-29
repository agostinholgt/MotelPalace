using System.Text;
using MotelPalace.Enums;
using MotelPalace.Models;
using MotelPalace.Utils;

namespace MotelPalace.Services;

/// <summary>Relatórios gerenciais do Motel Palace (LINQ sobre reservas, pagamentos e estoque).</summary>
public class RelatorioService
{
    private readonly ReservaService _reservas;
    private readonly PagamentoService _pagamentos;
    private readonly SuiteService _suites;
    private readonly ConsumoItemService _itens;
    private readonly IRelogio _relogio;

    public RelatorioService(ReservaService reservas, PagamentoService pagamentos,
                            SuiteService suites, ConsumoItemService itens, IRelogio relogio)
    {
        _reservas = reservas;
        _pagamentos = pagamentos;
        _suites = suites;
        _itens = itens;
        _relogio = relogio;
    }

    public decimal FaturamentoRecebido() =>
        _pagamentos.Listar().Where(p => p.Status == StatusPagamento.Confirmado).Sum(p => p.Valor);

    public IReadOnlyDictionary<FormaPagamento, decimal> FaturamentoPorForma() =>
        _pagamentos.Listar()
            .Where(p => p.Status == StatusPagamento.Confirmado)
            .GroupBy(p => p.Forma)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Valor));

    public double TaxaOcupacao()
    {
        var todas = _suites.Listar();
        if (todas.Count == 0) return 0;
        return (double)todas.Count(s => s.Status == StatusSuite.Ocupada) / todas.Count * 100.0;
    }

    public IEnumerable<(string Item, int Quantidade, decimal Valor)> ItensMaisConsumidos(int top = 5) =>
        _reservas.Listar()
            .Where(r => r.Status != StatusReserva.Cancelada)
            .SelectMany(r => r.Consumos)
            .GroupBy(c => c.NomeItem)
            .Select(g => (Item: g.Key, Quantidade: g.Sum(c => c.Quantidade), Valor: g.Sum(c => c.Subtotal)))
            .OrderByDescending(x => x.Quantidade)
            .ThenBy(x => x.Item)
            .Take(top)
            .ToList();

    public IEnumerable<(TipoSuite Tipo, int Hospedagens, decimal Receita)> ReceitaPorTipoDeSuite() =>
        _reservas.Listar()
            .Where(r => r.Status == StatusReserva.Finalizada)
            .GroupBy(r => _suites.ObterPorId(r.SuiteId).Tipo)
            .Select(g => (Tipo: g.Key, Hospedagens: g.Count(), Receita: g.Sum(r => r.ValorTotal)))
            .OrderByDescending(x => x.Receita)
            .ToList();

    public string GerarResumoGerencial()
    {
        var sb = new StringBuilder();
        var finalizadas = _reservas.Listar().Where(r => r.Status == StatusReserva.Finalizada).ToList();

        sb.AppendLine("=========== MOTEL PALACE — RESUMO GERENCIAL ===========");
        sb.AppendLine($"Emitido em: {Formatar.DataHora(_relogio.Agora)}");
        sb.AppendLine();

        sb.AppendLine("• Suítes por status:");
        foreach (var grupo in _suites.Listar().GroupBy(s => s.Status).OrderBy(g => g.Key))
            sb.AppendLine($"    {grupo.Key,-12} {grupo.Count()}");
        sb.AppendLine($"    Taxa de ocupação: {TaxaOcupacao():F1}%");
        sb.AppendLine();

        sb.AppendLine("• Reservas por status:");
        foreach (var grupo in _reservas.Listar().GroupBy(r => r.Status).OrderBy(g => g.Key))
            sb.AppendLine($"    {grupo.Key,-12} {grupo.Count()}");
        sb.AppendLine();

        sb.AppendLine("• Composição da receita (hospedagens finalizadas):");
        sb.AppendLine($"    Hospedagem : {Formatar.Moeda(finalizadas.Sum(r => r.ValorHospedagem))}");
        sb.AppendLine($"    Hora extra : {Formatar.Moeda(finalizadas.Sum(r => r.ValorHoraExtra))}");
        sb.AppendLine($"    Consumo    : {Formatar.Moeda(finalizadas.Sum(r => r.ValorConsumo))}");
        sb.AppendLine($"    TOTAL      : {Formatar.Moeda(finalizadas.Sum(r => r.ValorTotal))}");
        sb.AppendLine();

        sb.AppendLine($"• Total recebido: {Formatar.Moeda(FaturamentoRecebido())}");
        foreach (var (forma, valor) in FaturamentoPorForma().OrderByDescending(x => x.Value))
            sb.AppendLine($"    {forma,-14} {Formatar.Moeda(valor)}");
        sb.AppendLine();

        sb.AppendLine("• Receita por tipo de suíte:");
        foreach (var (tipo, qtd, receita) in ReceitaPorTipoDeSuite())
            sb.AppendLine($"    {tipo,-13} {qtd} hosp. | {Formatar.Moeda(receita)}");
        sb.AppendLine();

        sb.AppendLine("• Itens mais consumidos:");
        foreach (var (item, qtd, valor) in ItensMaisConsumidos())
            sb.AppendLine($"    {item,-28} {qtd,3} un. | {Formatar.Moeda(valor)}");
        sb.AppendLine();

        sb.AppendLine("• Estoque baixo (≤ 5 un.):");
        var baixos = _itens.ListarEstoqueBaixo().ToList();
        if (baixos.Count == 0) sb.AppendLine("    Nenhum item com estoque baixo.");
        foreach (var item in baixos)
            sb.AppendLine($"    {item.Nome,-28} {item.Estoque,3} un.");

        sb.AppendLine("=======================================================");
        return sb.ToString();
    }
}
