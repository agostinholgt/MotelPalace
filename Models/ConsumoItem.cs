using MotelPalace.Enums;
using MotelPalace.Exceptions;

namespace MotelPalace.Models;

/// <summary>Classe 5 — Item de frigobar/cardápio com controle de estoque.</summary>
public class ConsumoItem : Entidade
{
    public string Nome { get; set; } = string.Empty;
    public CategoriaConsumo Categoria { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("A quantidade deve ser maior que zero.");
        if (quantidade > Estoque)
            throw new RegraDeNegocioException(
                $"Estoque insuficiente de '{Nome}': disponível {Estoque}, solicitado {quantidade}.");
        Estoque -= quantidade;
    }

    public void Repor(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("A quantidade de reposição deve ser maior que zero.");
        Estoque += quantidade;
    }
}
