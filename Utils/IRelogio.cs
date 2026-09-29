namespace MotelPalace.Utils;

/// <summary>Abstração do relógio: permite simular a passagem do tempo (hora extra, feriados).</summary>
public interface IRelogio
{
    DateTime Agora { get; }
}

public class RelogioSistema : IRelogio
{
    public DateTime Agora => DateTime.Now;
}

public class RelogioSimulado : IRelogio
{
    private DateTime _atual;

    public RelogioSimulado(DateTime inicio) => _atual = inicio;

    public DateTime Agora => _atual;

    public void Avancar(TimeSpan intervalo)
    {
        if (intervalo < TimeSpan.Zero)
            throw new ArgumentException("Não é possível voltar no tempo.", nameof(intervalo));
        _atual += intervalo;
    }

    public void Definir(DateTime novaData) => _atual = novaData;
}
