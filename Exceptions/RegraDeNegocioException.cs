namespace MotelPalace.Exceptions;

/// <summary>Violação de uma regra de negócio do Motel Palace.</summary>
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem) { }
}
