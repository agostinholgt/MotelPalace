using MotelPalace.Enums;

namespace MotelPalace.Data;

/// <summary>Carga inicial: suítes, tarifas, cardápio/frigobar e alguns clientes.</summary>
public static class DadosIniciais
{
    public static void Popular(MotelSistema s)
    {
        PopularSuites(s);
        PopularTarifas(s);
        PopularItens(s);
        PopularClientes(s);
    }

    private static void PopularSuites(MotelSistema s)
    {
        s.Suites.Cadastrar(101, TipoSuite.Simples, "Cama queen, ar-condicionado e TV", 2);
        s.Suites.Cadastrar(102, TipoSuite.Simples, "Cama queen, ar-condicionado e TV", 2);
        s.Suites.Cadastrar(103, TipoSuite.Simples, "Cama queen e garagem privativa", 2);
        var manutencao = s.Suites.Cadastrar(104, TipoSuite.Simples, "Cama queen e garagem privativa", 2);
        s.Suites.EnviarParaManutencao(manutencao.Id); // suíte 104 começa em manutenção

        s.Suites.Cadastrar(201, TipoSuite.Luxo, "Banheira de hidromassagem e som ambiente", 2);
        s.Suites.Cadastrar(202, TipoSuite.Luxo, "Hidromassagem, sauna seca e frigobar", 2);
        s.Suites.Cadastrar(203, TipoSuite.Luxo, "Hidromassagem, sauna seca e frigobar", 3);

        s.Suites.Cadastrar(301, TipoSuite.Tematica, "Tema 'Noite em Paris' com iluminação cênica", 2);
        s.Suites.Cadastrar(302, TipoSuite.Tematica, "Tema 'Safari' com decoração exótica", 2);

        s.Suites.Cadastrar(401, TipoSuite.Presidencial, "Cobertura com piscina aquecida, sauna e varanda", 4);
    }

    private static void PopularTarifas(MotelSistema s)
    {
        // Base (dia útil) por período para a suíte Simples.
        var baseSimples = new Dictionary<PeriodoHospedagem, decimal>
        {
            [PeriodoHospedagem.TresHoras] = 60m,
            [PeriodoHospedagem.SeisHoras] = 90m,
            [PeriodoHospedagem.DozeHoras] = 140m,
            [PeriodoHospedagem.Pernoite] = 170m
        };

        var multiplicadorTipo = new Dictionary<TipoSuite, decimal>
        {
            [TipoSuite.Simples] = 1.0m,
            [TipoSuite.Luxo] = 1.6m,
            [TipoSuite.Tematica] = 2.0m,
            [TipoSuite.Presidencial] = 3.0m
        };

        var multiplicadorDia = new Dictionary<TipoDia, decimal>
        {
            [TipoDia.DiaUtil] = 1.0m,
            [TipoDia.FimDeSemana] = 1.25m,
            [TipoDia.Feriado] = 1.5m
        };

        foreach (var (tipo, mTipo) in multiplicadorTipo)
        foreach (var (periodo, valorBase) in baseSimples)
        foreach (var (dia, mDia) in multiplicadorDia)
        {
            var valor = ArredondarPara5(valorBase * mTipo * mDia);
            // Hora extra: metade do valor da tarifa de 3 horas do mesmo tipo/dia.
            var horaExtra = ArredondarPara5(baseSimples[PeriodoHospedagem.TresHoras] * mTipo * mDia * 0.5m);
            s.Tarifas.Cadastrar(tipo, periodo, dia, valor, horaExtra);
        }
    }

    private static decimal ArredondarPara5(decimal valor) => Math.Round(valor / 5m, 0) * 5m;

    private static void PopularItens(MotelSistema s)
    {
        s.Itens.Cadastrar("Água mineral 500ml", CategoriaConsumo.Bebida, 6m, 60);
        s.Itens.Cadastrar("Refrigerante lata", CategoriaConsumo.Bebida, 8m, 50);
        s.Itens.Cadastrar("Cerveja long neck", CategoriaConsumo.Bebida, 14m, 48);
        s.Itens.Cadastrar("Energético", CategoriaConsumo.Bebida, 15m, 24);
        s.Itens.Cadastrar("Espumante", CategoriaConsumo.Bebida, 120m, 4); // estoque baixo de propósito
        s.Itens.Cadastrar("Porção de batata frita", CategoriaConsumo.Comida, 38m, 20);
        s.Itens.Cadastrar("Pizza brotinho", CategoriaConsumo.Comida, 55m, 15);
        s.Itens.Cadastrar("Barra de chocolate", CategoriaConsumo.Comida, 12m, 40);
        s.Itens.Cadastrar("Kit sobremesa", CategoriaConsumo.Comida, 45m, 10);
        s.Itens.Cadastrar("Pétalas de rosa", CategoriaConsumo.Acessorio, 25m, 12);
        s.Itens.Cadastrar("Óleo para massagem", CategoriaConsumo.Acessorio, 40m, 8);
        s.Itens.Cadastrar("Kit conveniência", CategoriaConsumo.Acessorio, 10m, 100);
    }

    private static void PopularClientes(MotelSistema s)
    {
        s.Clientes.Cadastrar("João Pereira", "529.982.247-25", "(11) 98888-1111", "joao@email.com");
        s.Clientes.Cadastrar("Maria Souza", "111.444.777-35", "(11) 97777-2222", "maria@email.com");
        s.Clientes.Cadastrar("Carlos Almeida", "390.533.447-05", "(21) 96666-3333", "carlos@email.com");
        s.Clientes.Cadastrar("Ana Ribeiro", "168.995.350-09", "(31) 95555-4444", "ana@email.com");
    }
}
