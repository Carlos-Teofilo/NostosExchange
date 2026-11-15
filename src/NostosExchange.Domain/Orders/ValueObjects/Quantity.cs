using NostosExchange.Domain.Orders.DomainExceptions;

namespace NostosExchange.Domain.Orders.ValueObjects;

/// <summary>
/// Quantidade em unidades inteiras do ativo (ações na B3 não são fracionáveis abaixo de 1).
/// </summary>
public record Quantity
{
    public long Value { get; }

    public Quantity(long value)
    {
        if (value <= 0)
        {
            throw new InvalidQuantityException("Quantity must be greater than zero.");
        }

        Value = value;
    }
}
