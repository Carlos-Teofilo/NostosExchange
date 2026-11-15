using NostosExchange.Domain.Orders.DomainExceptions;

namespace NostosExchange.Domain.Orders.ValueObjects;

public record Price
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value <= 0)
        {
            throw new InvalidPriceException("Price must be greater than zero.");
        }

        Value = value;
    }
}