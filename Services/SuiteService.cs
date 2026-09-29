using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;

namespace MotelPalace.Services;

public class SuiteService
{
    // Máquina de estados: quais mudanças de status são permitidas.
    private static readonly Dictionary<StatusSuite, StatusSuite[]> Transicoes = new()
    {
        [StatusSuite.Disponivel] = new[] { StatusSuite.Ocupada, StatusSuite.Manutencao },
        [StatusSuite.Ocupada] = new[] { StatusSuite.Limpeza, StatusSuite.Disponivel },
        [StatusSuite.Limpeza] = new[] { StatusSuite.Disponivel, StatusSuite.Manutencao },
        [StatusSuite.Manutencao] = new[] { StatusSuite.Disponivel }
    };

    private readonly IRepositorio<Suite> _repo;

    public SuiteService(IRepositorio<Suite> repo) => _repo = repo;

    public Suite Cadastrar(int numero, TipoSuite tipo, string descricao, int capacidade)
    {
        if (numero <= 0)
            throw new RegraDeNegocioException("O número da suíte deve ser maior que zero.");
        if (_repo.Buscar(s => s.Numero == numero).Any())
            throw new RegraDeNegocioException($"Já existe a suíte de número {numero}.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraDeNegocioException("Informe uma descrição para a suíte.");
        if (capacidade is < 1 or > 6)
            throw new RegraDeNegocioException("A capacidade deve estar entre 1 e 6 pessoas.");

        return _repo.Adicionar(new Suite
        {
            Numero = numero,
            Tipo = tipo,
            Descricao = descricao.Trim(),
            Capacidade = capacidade,
            Status = StatusSuite.Disponivel
        });
    }

    public Suite ObterPorId(int id) =>
        _repo.ObterPorId(id) ?? throw new EntidadeNaoEncontradaException(nameof(Suite), id);

    public Suite ObterPorNumero(int numero) =>
        _repo.Buscar(s => s.Numero == numero).FirstOrDefault()
        ?? throw new EntidadeNaoEncontradaException(nameof(Suite), $"nº {numero}");

    public IReadOnlyList<Suite> Listar() => _repo.ListarTodos();

    public IEnumerable<Suite> ListarPorStatus(StatusSuite status) =>
        _repo.Buscar(s => s.Status == status);

    public IEnumerable<Suite> ListarDisponiveis(TipoSuite? tipo = null) =>
        _repo.Buscar(s => s.Status == StatusSuite.Disponivel && (tipo is null || s.Tipo == tipo));

    // ---- Operações de status (usadas pela recepção e pela ReservaService) ----

    public Suite MarcarOcupada(int id) => Alterar(id, StatusSuite.Ocupada);

    public Suite MarcarParaLimpeza(int id) => Alterar(id, StatusSuite.Limpeza);

    public Suite ConcluirLimpeza(int id)
    {
        var suite = ObterPorId(id);
        if (suite.Status != StatusSuite.Limpeza)
            throw new RegraDeNegocioException($"A suíte {suite.Numero} não está em limpeza (status: {suite.Status}).");
        return Alterar(id, StatusSuite.Disponivel);
    }

    public Suite EnviarParaManutencao(int id)
    {
        var suite = ObterPorId(id);
        if (suite.Status == StatusSuite.Ocupada)
            throw new RegraDeNegocioException(
                $"A suíte {suite.Numero} está ocupada e não pode ir para manutenção agora.");
        return Alterar(id, StatusSuite.Manutencao);
    }

    public Suite ConcluirManutencao(int id)
    {
        var suite = ObterPorId(id);
        if (suite.Status != StatusSuite.Manutencao)
            throw new RegraDeNegocioException($"A suíte {suite.Numero} não está em manutenção (status: {suite.Status}).");
        return Alterar(id, StatusSuite.Disponivel);
    }

    /// <summary>Cancelamento de reserva: a suíte volta a ficar disponível (sem uso, não precisa de limpeza).</summary>
    public Suite LiberarSemUso(int id) => Alterar(id, StatusSuite.Disponivel);

    private Suite Alterar(int id, StatusSuite novoStatus)
    {
        var suite = ObterPorId(id);
        if (!Transicoes[suite.Status].Contains(novoStatus))
            throw new RegraDeNegocioException(
                $"Transição inválida na suíte {suite.Numero}: {suite.Status} → {novoStatus}.");
        suite.Status = novoStatus;
        return suite;
    }
}
