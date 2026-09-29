namespace MotelPalace.Dtos;

/// <summary>Extrato da conta de uma reserva em um determinado instante.</summary>
public record ContaReserva(
    int ReservaId,
    DateTime Momento,
    decimal Hospedagem,
    int HorasExtras,
    decimal HoraExtra,
    decimal Consumo,
    decimal Total,
    decimal Pago,
    decimal Saldo);
