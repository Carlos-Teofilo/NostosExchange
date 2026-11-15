namespace NostosExchange.Domain.Orders.Enums;

/// <summary>
/// Lado da ordem. Começa em 1 para que <c>default(Side)</c> seja inválido e detectável.
/// </summary>
public enum Side : byte
{
    Buy = 1,
    Sell = 2
}
