namespace MotelPalace.Exceptions;

public class EntidadeNaoEncontradaException : RegraDeNegocioException
{
    public EntidadeNaoEncontradaException(string entidade, object identificador)
        : base($"{entidade} não encontrado(a): {identificador}.") { }
}
