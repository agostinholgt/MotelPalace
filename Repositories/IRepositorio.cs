using MotelPalace.Models;

namespace MotelPalace.Repositories;

public interface IRepositorio<T> where T : Entidade
{
    T Adicionar(T entidade);
    T? ObterPorId(int id);
    IReadOnlyList<T> ListarTodos();
    IEnumerable<T> Buscar(Func<T, bool> predicado);
    bool Remover(int id);
}
