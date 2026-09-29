using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;

namespace MotelPalace.Services;

public class ConsumoItemService
{
    private readonly IRepositorio<ConsumoItem> _repo;

    public ConsumoItemService(IRepositorio<ConsumoItem> repo) => _repo = repo;

    public ConsumoItem Cadastrar(string nome, CategoriaConsumo categoria, decimal preco, int estoqueInicial)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioException("Informe o nome do item.");
        if (preco <= 0)
            throw new RegraDeNegocioException("O preço deve ser maior que zero.");
        if (estoqueInicial < 0)
            throw new RegraDeNegocioException("O estoque inicial não pode ser negativo.");
        if (_repo.Buscar(i => string.Equals(i.Nome, nome.Trim(), StringComparison.OrdinalIgnoreCase)).Any())
            throw new RegraDeNegocioException($"Já existe um item chamado '{nome.Trim()}'.");

        return _repo.Adicionar(new ConsumoItem
        {
            Nome = nome.Trim(),
            Categoria = categoria,
            Preco = preco,
            Estoque = estoqueInicial
        });
    }

    public ConsumoItem ObterPorId(int id) =>
        _repo.ObterPorId(id) ?? throw new EntidadeNaoEncontradaException(nameof(ConsumoItem), id);

    public IReadOnlyList<ConsumoItem> Listar() => _repo.ListarTodos();

    public IEnumerable<ConsumoItem> ListarPorCategoria(CategoriaConsumo categoria) =>
        _repo.Buscar(i => i.Categoria == categoria);

    public ConsumoItem Repor(int id, int quantidade)
    {
        var item = ObterPorId(id);
        item.Repor(quantidade);
        return item;
    }

    public ConsumoItem AlterarPreco(int id, decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new RegraDeNegocioException("O preço deve ser maior que zero.");
        var item = ObterPorId(id);
        item.Preco = novoPreco;
        return item;
    }

    public IEnumerable<ConsumoItem> ListarEstoqueBaixo(int limite = 5) =>
        _repo.Buscar(i => i.Estoque <= limite);
}
