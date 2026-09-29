using MotelPalace.Data;
using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Utils;

namespace MotelPalace.UI;

/// <summary>Menu interativo de console para operar o Motel Palace.</summary>
public class MenuPrincipal
{
    private readonly MotelSistema _s;
    private readonly RelogioSimulado _relogio;

    public MenuPrincipal(MotelSistema sistema, RelogioSimulado relogio)
    {
        _s = sistema;
        _relogio = relogio;
    }

    public void Executar()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
  __  __  ___ _____ _____ _       ____   _    _      _    ____ _____
 |  \/  |/ _ \_   _| ____| |     |  _ \ / \  | |    / \  / ___| ____|
 | |\/| | | | || | |  _| | |     | |_) / _ \ | |   / _ \| |   |  _|
 | |  | | |_| || | | |___| |___  |  __/ ___ \| |__/ ___ \ |___| |___
 |_|  |_|\___/ |_| |_____|_____| |_| /_/   \_\_____/_/ \_\____|_____|
");
        Console.ResetColor();

        while (true)
        {
            Saida.Titulo($"MENU PRINCIPAL — {Formatar.DataHora(_relogio.Agora)}");
            Console.WriteLine("  1) Clientes");
            Console.WriteLine("  2) Suítes");
            Console.WriteLine("  3) Hospedagens (reservas)");
            Console.WriteLine("  4) Pagamentos");
            Console.WriteLine("  5) Cardápio / Frigobar");
            Console.WriteLine("  6) Tarifas");
            Console.WriteLine("  7) Relatório gerencial");
            Console.WriteLine("  8) Relógio simulado (avançar tempo)");
            Console.WriteLine("  0) Sair");

            switch (Entrada.Inteiro("Opção"))
            {
                case 1: MenuClientes(); break;
                case 2: MenuSuites(); break;
                case 3: MenuReservas(); break;
                case 4: MenuPagamentos(); break;
                case 5: MenuItens(); break;
                case 6: MenuTarifas(); break;
                case 7: Tentar(() => Console.WriteLine(_s.Relatorios.GerarResumoGerencial())); Entrada.Pausa(); break;
                case 8: Tentar(AvancarRelogio); break;
                case 0: return;
                default: Saida.Erro("Opção inválida."); break;
            }
        }
    }

    // ------------------------------------------------------------------
    // Infra do menu
    // ------------------------------------------------------------------

    private static void Tentar(Action acao)
    {
        try { acao(); }
        catch (RegraDeNegocioException ex) { Saida.Erro(ex.Message); }
    }

    private void Submenu(string titulo, params (string Texto, Action Acao)[] opcoes)
    {
        while (true)
        {
            Saida.Titulo(titulo);
            for (int i = 0; i < opcoes.Length; i++)
                Console.WriteLine($"  {i + 1}) {opcoes[i].Texto}");
            Console.WriteLine("  0) Voltar");

            var escolha = Entrada.Inteiro("Opção");
            if (escolha == 0) return;
            if (escolha < 1 || escolha > opcoes.Length)
            {
                Saida.Erro("Opção inválida.");
                continue;
            }
            Tentar(opcoes[escolha - 1].Acao);
        }
    }

    // ------------------------------------------------------------------
    // Clientes
    // ------------------------------------------------------------------

    private void MenuClientes() => Submenu("CLIENTES",
        ("Cadastrar cliente", CadastrarCliente),
        ("Listar clientes", () => _s.Clientes.Listar().ToList().ForEach(c => Console.WriteLine("  " + c))),
        ("Buscar por CPF", BuscarClientePorCpf),
        ("Atualizar contato", AtualizarContato),
        ("Histórico de hospedagens de um cliente", HistoricoCliente));

    private void CadastrarCliente()
    {
        var nome = Entrada.Texto("Nome");
        var cpf = Entrada.Texto("CPF");
        var tel = Entrada.Texto("Telefone (com DDD)");
        var email = Entrada.Texto("E-mail");
        var c = _s.Clientes.Cadastrar(nome, cpf, tel, email);
        Saida.Ok($"Cliente cadastrado: {c}");
    }

    private void BuscarClientePorCpf()
    {
        var c = _s.Clientes.ObterPorCpf(Entrada.Texto("CPF"));
        Console.WriteLine("  " + c);
    }

    private void AtualizarContato()
    {
        var id = Entrada.Inteiro("Id do cliente");
        var tel = Entrada.Texto("Novo telefone");
        var email = Entrada.Texto("Novo e-mail");
        Saida.Ok("Contato atualizado: " + _s.Clientes.AtualizarContato(id, tel, email));
    }

    private void HistoricoCliente()
    {
        var id = Entrada.Inteiro("Id do cliente");
        var cliente = _s.Clientes.ObterPorId(id);
        var historico = _s.Reservas.HistoricoDoCliente(id).ToList();
        Console.WriteLine($"  Histórico de {cliente.Nome}: {historico.Count} hospedagem(ns)");
        foreach (var r in historico) ImprimirReserva(r);
    }

    // ------------------------------------------------------------------
    // Suítes
    // ------------------------------------------------------------------

    private void MenuSuites() => Submenu("SUÍTES",
        ("Listar todas", () => _s.Suites.Listar().ToList().ForEach(x => Console.WriteLine("  " + x))),
        ("Listar disponíveis", () => _s.Suites.ListarDisponiveis().ToList().ForEach(x => Console.WriteLine("  " + x))),
        ("Cadastrar suíte", CadastrarSuite),
        ("Enviar para manutenção", () =>
            Saida.Ok("Ok: " + _s.Suites.EnviarParaManutencao(_s.Suites.ObterPorNumero(Entrada.Inteiro("Número da suíte")).Id))),
        ("Concluir manutenção", () =>
            Saida.Ok("Ok: " + _s.Suites.ConcluirManutencao(_s.Suites.ObterPorNumero(Entrada.Inteiro("Número da suíte")).Id))),
        ("Concluir limpeza (liberar suíte)", () =>
            Saida.Ok("Ok: " + _s.Suites.ConcluirLimpeza(_s.Suites.ObterPorNumero(Entrada.Inteiro("Número da suíte")).Id))));

    private void CadastrarSuite()
    {
        var numero = Entrada.Inteiro("Número");
        var tipo = Entrada.Opcao<TipoSuite>("Tipo da suíte");
        var desc = Entrada.Texto("Descrição");
        var cap = Entrada.Inteiro("Capacidade");
        Saida.Ok("Suíte cadastrada: " + _s.Suites.Cadastrar(numero, tipo, desc, cap));
    }

    // ------------------------------------------------------------------
    // Reservas
    // ------------------------------------------------------------------

    private void MenuReservas() => Submenu("HOSPEDAGENS",
        ("Nova hospedagem (check-in)", NovaReserva),
        ("Lançar consumo", LancarConsumo),
        ("Ver conta", () => Saida.Conta(_s.Reservas.ObterConta(Entrada.Inteiro("Id da reserva")))),
        ("Finalizar hospedagem (check-out)", FinalizarReserva),
        ("Cancelar hospedagem", () =>
        {
            var r = _s.Reservas.Cancelar(Entrada.Inteiro("Id da reserva"));
            Saida.Ok($"Reserva #{r.Id} cancelada.");
        }),
        ("Listar hospedagens ativas", () => _s.Reservas.ListarAtivas().ToList().ForEach(ImprimirReserva)),
        ("Listar TODAS as hospedagens", () => _s.Reservas.Listar().ToList().ForEach(ImprimirReserva)),
        ("Saídas atrasadas (hora extra em andamento)", () =>
        {
            var atrasadas = _s.Reservas.ListarComSaidaAtrasada().ToList();
            if (atrasadas.Count == 0) Saida.Info("Nenhuma saída atrasada.");
            atrasadas.ForEach(ImprimirReserva);
        }));

    private void NovaReserva()
    {
        var cpf = Entrada.Texto("CPF do cliente");
        var cliente = _s.Clientes.ObterPorCpf(cpf);

        Console.WriteLine("Suítes disponíveis:");
        var disponiveis = _s.Suites.ListarDisponiveis().ToList();
        if (disponiveis.Count == 0) throw new RegraDeNegocioException("Não há suítes disponíveis no momento.");
        disponiveis.ForEach(x => Console.WriteLine("  " + x));

        var suite = _s.Suites.ObterPorNumero(Entrada.Inteiro("Número da suíte"));
        var periodo = Entrada.Opcao<PeriodoHospedagem>("Período");

        var r = _s.Reservas.Criar(cliente.Id, suite.Id, periodo);
        Saida.Ok($"Check-in feito! Reserva #{r.Id} — {Formatar.Moeda(r.ValorHospedagem)} " +
                 $"({r.TipoDiaAplicado}). Saída prevista: {Formatar.DataHora(r.DataSaidaPrevista)}");
    }

    private void LancarConsumo()
    {
        var reservaId = Entrada.Inteiro("Id da reserva");
        _s.Itens.Listar().ToList().ForEach(i =>
            Console.WriteLine($"  #{i.Id} {i.Nome,-26} {Formatar.Moeda(i.Preco),10}  estoque: {i.Estoque}"));
        var itemId = Entrada.Inteiro("Id do item");
        var qtd = Entrada.Inteiro("Quantidade");

        var lanc = _s.Reservas.LancarConsumo(reservaId, itemId, qtd);
        Saida.Ok($"Lançado: {qtd}x {lanc.NomeItem} ({Formatar.Moeda(lanc.PrecoUnitario * qtd)}). Estoque atualizado.");
    }

    private void FinalizarReserva()
    {
        var id = Entrada.Inteiro("Id da reserva");
        Saida.Conta(_s.Reservas.ObterConta(id));
        var r = _s.Reservas.Finalizar(id);
        var suite = _s.Suites.ObterPorId(r.SuiteId);
        Saida.Ok($"Hospedagem #{r.Id} finalizada. Suíte {suite.Numero} agora está em: {suite.Status}.");
    }

    private void ImprimirReserva(Reserva r)
    {
        var cliente = _s.Clientes.ObterPorId(r.ClienteId);
        var suite = _s.Suites.ObterPorId(r.SuiteId);
        Console.WriteLine(
            $"  #{r.Id} | {cliente.Nome} | Suíte {suite.Numero} | {r.Periodo.Descricao()} | " +
            $"entrada {Formatar.DataHora(r.DataEntrada)} | previsto {Formatar.DataHora(r.DataSaidaPrevista)} | {r.Status}");
    }

    // ------------------------------------------------------------------
    // Pagamentos
    // ------------------------------------------------------------------

    private void MenuPagamentos() => Submenu("PAGAMENTOS",
        ("Registrar pagamento", () =>
        {
            var id = Entrada.Inteiro("Id da reserva");
            Saida.Conta(_s.Reservas.ObterConta(id));
            var valor = Entrada.Decimal("Valor a pagar");
            var forma = Entrada.Opcao<FormaPagamento>("Forma de pagamento");
            var p = _s.Pagamentos.Registrar(id, valor, forma);
            Saida.Ok($"Pagamento #{p.Id} de {Formatar.Moeda(p.Valor)} via {p.Forma} confirmado.");
        }),
        ("Quitar saldo total", () =>
        {
            var id = Entrada.Inteiro("Id da reserva");
            var forma = Entrada.Opcao<FormaPagamento>("Forma de pagamento");
            var p = _s.Pagamentos.QuitarSaldo(id, forma);
            Saida.Ok($"Saldo quitado: {Formatar.Moeda(p.Valor)} via {p.Forma}.");
        }),
        ("Estornar pagamento", () =>
        {
            var p = _s.Pagamentos.Estornar(Entrada.Inteiro("Id do pagamento"));
            Saida.Ok($"Pagamento #{p.Id} estornado.");
        }),
        ("Listar pagamentos de uma reserva", () =>
        {
            foreach (var p in _s.Pagamentos.ListarPorReserva(Entrada.Inteiro("Id da reserva")))
                Console.WriteLine($"  #{p.Id} {Formatar.DataHora(p.DataPagamento)} {p.Forma,-14} {Formatar.Moeda(p.Valor),12} {p.Status}");
        }));

    // ------------------------------------------------------------------
    // Itens
    // ------------------------------------------------------------------

    private void MenuItens() => Submenu("CARDÁPIO / FRIGOBAR",
        ("Listar itens", () => _s.Itens.Listar().ToList().ForEach(i =>
            Console.WriteLine($"  #{i.Id} {i.Nome,-26} {i.Categoria,-10} {Formatar.Moeda(i.Preco),10}  estoque: {i.Estoque}"))),
        ("Cadastrar item", () =>
        {
            var nome = Entrada.Texto("Nome");
            var cat = Entrada.Opcao<CategoriaConsumo>("Categoria");
            var preco = Entrada.Decimal("Preço");
            var estoque = Entrada.Inteiro("Estoque inicial");
            Saida.Ok("Item cadastrado: " + _s.Itens.Cadastrar(nome, cat, preco, estoque).Nome);
        }),
        ("Repor estoque", () =>
        {
            var item = _s.Itens.Repor(Entrada.Inteiro("Id do item"), Entrada.Inteiro("Quantidade a repor"));
            Saida.Ok($"Estoque de {item.Nome}: {item.Estoque} un.");
        }),
        ("Alterar preço", () =>
        {
            var item = _s.Itens.AlterarPreco(Entrada.Inteiro("Id do item"), Entrada.Decimal("Novo preço"));
            Saida.Ok($"{item.Nome} agora custa {Formatar.Moeda(item.Preco)}.");
        }),
        ("Itens com estoque baixo", () =>
        {
            foreach (var i in _s.Itens.ListarEstoqueBaixo())
                Console.WriteLine($"  #{i.Id} {i.Nome,-26} estoque: {i.Estoque}");
        }));

    // ------------------------------------------------------------------
    // Tarifas
    // ------------------------------------------------------------------

    private void MenuTarifas() => Submenu("TARIFAS",
        ("Listar todas", () =>
        {
            foreach (var t in _s.Tarifas.Listar().OrderBy(t => t.TipoSuite).ThenBy(t => t.Periodo).ThenBy(t => t.TipoDia))
                Console.WriteLine($"  {t.TipoSuite,-13} {t.Periodo.Descricao(),-15} {t.TipoDia,-12} {Formatar.Moeda(t.Valor),10}  (hora extra {Formatar.Moeda(t.ValorHoraExtra)})");
        }),
        ("Consultar tarifa", () =>
        {
            var tipo = Entrada.Opcao<TipoSuite>("Tipo de suíte");
            var periodo = Entrada.Opcao<PeriodoHospedagem>("Período");
            var dia = Entrada.Opcao<TipoDia>("Tipo de dia");
            var t = _s.Tarifas.Obter(tipo, periodo, dia);
            Saida.Info($"{Formatar.Moeda(t.Valor)} | hora extra {Formatar.Moeda(t.ValorHoraExtra)}");
        }),
        ("Cadastrar tarifa", () =>
        {
            var tipo = Entrada.Opcao<TipoSuite>("Tipo de suíte");
            var periodo = Entrada.Opcao<PeriodoHospedagem>("Período");
            var dia = Entrada.Opcao<TipoDia>("Tipo de dia");
            var valor = Entrada.Decimal("Valor");
            var extra = Entrada.Decimal("Valor da hora extra");
            _s.Tarifas.Cadastrar(tipo, periodo, dia, valor, extra);
            Saida.Ok("Tarifa cadastrada.");
        }),
        ("Ver feriados do ano", () =>
        {
            var ano = Entrada.Inteiro("Ano");
            foreach (var (data, nome) in _s.Calendario.ObterFeriados(ano).OrderBy(x => x.Key))
                Console.WriteLine($"  {data:dd/MM/yyyy} ({data:ddd}) — {nome}");
        }));

    // ------------------------------------------------------------------
    // Relógio
    // ------------------------------------------------------------------

    private void AvancarRelogio()
    {
        Console.WriteLine($"Agora: {Formatar.DataHora(_relogio.Agora)}");
        var horas = Entrada.Decimal("Quantas horas deseja avançar? (ex.: 6,5)");
        if (horas < 0) throw new RegraDeNegocioException("Não é possível voltar no tempo.");
        _relogio.Avancar(TimeSpan.FromHours((double)horas));
        Saida.Ok($"Novo horário: {Formatar.DataHora(_relogio.Agora)}");
    }
}
