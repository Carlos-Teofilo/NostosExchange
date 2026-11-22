using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.ValueObjects;

namespace NostosExchange.Domain.Tests.Orders.ValueObjects;

/// <summary>
/// Cobre a regra de <c>Price</c> (<c>&gt; 0</c>) da tabela de Value Objects em docs/domain/order.md.
/// </summary>
public class PriceTests
{
    [Theory]
    [MemberData(nameof(PositivePrices))]
    public void Constructor_PositiveValue_ExposesValue(decimal value)
    {
        var price = new Price(value);

        Assert.Equal(value, price.Value);
    }

    [Theory]
    [MemberData(nameof(ZeroOrNegativePrices))]
    public void Constructor_ZeroOrNegativeValue_ThrowsInvalidPriceException(decimal value)
    {
        Assert.Throws<InvalidPriceException>(() => new Price(value));
    }

    [Fact]
    public void Equality_SameValue_AreEqualButNotSame()
    {
        // Arrange
        var priceValue1 = 38.50m;
        var priceValue2 = 38.50m;

        // Act
        var price1 = new Price(priceValue1);
        var price2 = new Price(priceValue2);

        // Assert
        Assert.Equal(price1, price2);
        Assert.NotSame(price1, price2);
    }

    // O decimal guarda a escala (38.5 tem 1 casa, 38.50 tem 2), mas a igualdade a ignora.
    [Fact]
    public void Value_DifferentScale_IsEqual()
    {
        var price = new Price(38.5m);

        Assert.Equal(38.50m, price.Value);
    }

    public static TheoryData<decimal> ZeroOrNegativePrices => new()
    {
        0.0m,
        -1.0m,
        -0.00001m,
    };

    public static TheoryData<decimal> PositivePrices => new()
    {
        // Aceito hoje porque ainda não existe validação de tick size (na B3, ações usam R$ 0,01).
        // Quando o Instrument existir (ADR 0004), este caso passará a ser inválido.
        0.00001m,
        1.0m,
        38.50m,
    };
}
