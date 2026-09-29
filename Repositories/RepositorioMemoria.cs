using MotelPalace.Models;

namespace MotelPalace.Repositories;

/// <summary>Repositório genérico em memória com Id auto-incrementado.</summary>
public class RepositorioMemoria<T> : IRepositorio<T> where T : Entidade
{
    private readonly Dictionary<int, T> _dados = new();
    private readonly object _trava = new();
    private int _proximoId = 1;

    public T Adicionar(T entidade)
    {
        lock (_trava)
        {
            entidade.Id = _proximoId++;
            _dados[entidade.Id] = entidade;
            return entidade;
        }
    }

    public T? ObterPorId(int id)
    {
        lock (_trava) return _dados.TryGetValue(id, out var e) ? e : null;
    }

    public IReadOnlyList<T> ListarTodos()
    {
        lock (_trava) return _dados.Values.OrderBy(x => x.Id).ToList();
    }

    public IEnumerable<T> Buscar(Func<T, bool> predicado)
    {
        lock (_trava) return _dados.Values.Where(predicado).OrderBy(x => x.Id).ToList();
    }

    public bool Remover(int id)
    {
        lock (_trava) return _dados.Remove(id);
    }
}
