using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;
using MotelPalace.Utils;

namespace MotelPalace.Services;

public class ClienteService
{
    private readonly IRepositorio<Cliente> _repo;
    private readonly IRelogio _relogio;

    public ClienteService(IRepositorio<Cliente> repo, IRelogio relogio)
    {
        _repo = repo;
        _relogio = relogio;
    }

    public Cliente Cadastrar(string nome, string cpf, string telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraDeNegocioException("Nome inválido: informe ao menos 3 caracteres.");

        var cpfNumeros = CpfValidador.Normalizar(cpf);
        if (!CpfValidador.Validar(cpfNumeros))
            throw new RegraDeNegocioException("CPF inválido.");

        if (_repo.Buscar(c => c.Cpf == cpfNumeros).Any())
            throw new RegraDeNegocioException($"Já existe um cliente com o CPF {Formatar.Cpf(cpfNumeros)}.");

        if (!Validacoes.TelefoneValido(telefone))
            throw new RegraDeNegocioException("Telefone inválido: use DDD + número (10 ou 11 dígitos).");

        if (!Validacoes.EmailValido(email))
            throw new RegraDeNegocioException("E-mail inválido.");

        return _repo.Adicionar(new Cliente
        {
            Nome = nome.Trim(),
            Cpf = cpfNumeros,
            Telefone = Validacoes.SomenteDigitos(telefone),
            Email = email.Trim().ToLowerInvariant(),
            DataCadastro = _relogio.Agora
        });
    }

    public Cliente ObterPorId(int id) =>
        _repo.ObterPorId(id) ?? throw new EntidadeNaoEncontradaException(nameof(Cliente), id);

    public Cliente ObterPorCpf(string cpf)
    {
        var numeros = CpfValidador.Normalizar(cpf);
        return _repo.Buscar(c => c.Cpf == numeros).FirstOrDefault()
               ?? throw new EntidadeNaoEncontradaException(nameof(Cliente), Formatar.Cpf(numeros));
    }

    public IReadOnlyList<Cliente> Listar() => _repo.ListarTodos();

    public Cliente AtualizarContato(int id, string telefone, string email)
    {
        var cliente = ObterPorId(id);

        if (!Validacoes.TelefoneValido(telefone))
            throw new RegraDeNegocioException("Telefone inválido: use DDD + número (10 ou 11 dígitos).");
        if (!Validacoes.EmailValido(email))
            throw new RegraDeNegocioException("E-mail inválido.");

        cliente.Telefone = Validacoes.SomenteDigitos(telefone);
        cliente.Email = email.Trim().ToLowerInvariant();
        return cliente;
    }
}
