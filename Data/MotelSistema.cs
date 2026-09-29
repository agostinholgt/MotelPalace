using MotelPalace.Models;
using MotelPalace.Repositories;
using MotelPalace.Services;
using MotelPalace.Utils;

namespace MotelPalace.Data;

/// <summary>
/// Raiz de composição (composition root): cria repositórios e injeta as dependências nos serviços.
/// </summary>
public class MotelSistema
{
    public IRelogio Relogio { get; }
    public CalendarioService Calendario { get; }
    public ClienteService Clientes { get; }
    public SuiteService Suites { get; }
    public TarifaService Tarifas { get; }
    public ConsumoItemService Itens { get; }
    public ReservaService Reservas { get; }
    public PagamentoService Pagamentos { get; }
    public RelatorioService Relatorios { get; }

    public MotelSistema(IRelogio relogio)
    {
        Relogio = relogio;

        var repoClientes = new RepositorioMemoria<Cliente>();
        var repoSuites = new RepositorioMemoria<Suite>();
        var repoTarifas = new RepositorioMemoria<Tarifa>();
        var repoItens = new RepositorioMemoria<ConsumoItem>();
        var repoReservas = new RepositorioMemoria<Reserva>();
        var repoPagamentos = new RepositorioMemoria<Pagamento>();

        Calendario = new CalendarioService();
        Clientes = new ClienteService(repoClientes, relogio);
        Suites = new SuiteService(repoSuites);
        Tarifas = new TarifaService(repoTarifas);
        Itens = new ConsumoItemService(repoItens);
        Reservas = new ReservaService(repoReservas, repoPagamentos, Clientes, Suites, Itens,
                                      Tarifas, Calendario, relogio);
        Pagamentos = new PagamentoService(repoPagamentos, Reservas, relogio);
        Relatorios = new RelatorioService(Reservas, Pagamentos, Suites, Itens, relogio);
    }
}
