using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.ValueObjects;

namespace NostosExchange.Domain.Tests.Orders.ValueObjects;

/// <summary>
/// Cobre a regra de <c>Quantity</c> (<c>&gt; 0</c>, unidades inteiras) da tabela de Value Objects
/// em docs/domain/order.md e o ADR 0004.
/// </summary>
public class QuantityTests
{
    [Theory]
    [MemberData(nameof(PositiveQuantities))]
    public void Constructor_PositiveValue_ExposesValue(long value)
    {
        Assert.Equal(value, new Quantity(value).Value);
    }

    [Theory]
    [MemberData(nameof(ZeroOrNegativeQuantities))]
    public void Constructor_ZeroOrNegativeValue_ThrowsInvalidQuantityException(long value)
    {
        Assert.Throws<InvalidQuantityException>(() => new Quantity(value));
    }

    public static TheoryData<long> ZeroOrNegativeQuantities => new()
    {
        0,              // limite: em cima da fronteira
        -1,             // limite: logo abaixo
        long.MinValue,  // extremo do tipo
    };

    public static TheoryData<long> PositiveQuantities => new()
    {
        1,              // limite: menor quantidade possível
        99,             // maior quantidade do mercado fracionário
        100,            // lote padrão
        // Aceito hoje porque o domínio não tem limite superior. Esse limite é papel do
        // risco pré-negociação (Res. CVM 35, art. 16), com RejectReason.RiskLimitExceeded.
        long.MaxValue,
    };
}
