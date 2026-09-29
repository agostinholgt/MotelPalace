using MotelPalace.Utils;

namespace MotelPalace.Models;

/// <summary>Classe 1 — Cliente do motel.</summary>
public class Cliente : Entidade
{
    public string Nome { get; set; } = string.Empty;
    /// <summary>Armazenado somente com dígitos.</summary>
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }

    public override string ToString() =>
        $"#{Id} {Nome} | CPF {Formatar.Cpf(Cpf)} | {Formatar.Telefone(Telefone)} | {Email}";
}
