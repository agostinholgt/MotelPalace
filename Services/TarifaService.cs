using MotelPalace.Enums;
using MotelPalace.Exceptions;
using MotelPalace.Models;
using MotelPalace.Repositories;

namespace MotelPalace.Services;

public class TarifaService
{
    private readonly IRepositorio<Tarifa> _repo;

    public TarifaService(IRepositorio<Tarifa> repo) => _repo = repo;

    public Tarifa Cadastrar(TipoSuite tipoSuite, PeriodoHospedagem periodo, TipoDia tipoDia,
                            decimal valor, decimal valorHoraExtra)
    {
        if (valor <= 0)
            throw new RegraDeNegocioException("O valor da tarifa deve ser maior que zero.");
        if (valorHoraExtra < 0)
            throw new RegraDeNegocioException("O valor da hora extra não pode ser negativo.");
        if (_repo.Buscar(t => t.TipoSuite == tipoSuite && t.Periodo == periodo && t.TipoDia == tipoDia).Any())
            throw new RegraDeNegocioException(
                $"Já existe tarifa para {tipoSuite} / {periodo.Descricao()} / {tipoDia}.");

        return _repo.Adicionar(new Tarifa
        {
            TipoSuite = tipoSuite,
            Periodo = periodo,
            TipoDia = tipoDia,
            Valor = valor,
            ValorHoraExtra = valorHoraExtra
        });
    }

    /// <summary>
    /// Consulta a tarifa por tipo de suíte + período + tipo de dia.
    /// Se não houver tarifa específica de feriado, aplica a de fim de semana.
    /// </summary>
    public Tarifa Obter(TipoSuite tipoSuite, PeriodoHospedagem periodo, TipoDia tipoDia)
    {
        var tarifa = Buscar(tipoSuite, periodo, tipoDia);

        if (tarifa is null && tipoDia == TipoDia.Feriado)
            tarifa = Buscar(tipoSuite, periodo, TipoDia.FimDeSemana);

        return tarifa ?? throw new RegraDeNegocioException(
            $"Não há tarifa cadastrada para {tipoSuite} / {periodo.Descricao()} / {tipoDia}.");
    }

    public IReadOnlyList<Tarifa> Listar() => _repo.ListarTodos();

    private Tarifa? Buscar(TipoSuite tipoSuite, PeriodoHospedagem periodo, TipoDia tipoDia) =>
        _repo.Buscar(t => t.TipoSuite == tipoSuite && t.Periodo == periodo && t.TipoDia == tipoDia)
             .FirstOrDefault();
}
