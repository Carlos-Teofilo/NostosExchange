using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.ValueObjects;

namespace NostosExchange.Domain.Tests.Orders.ValueObjects;

/// <summary>
/// Cobre a tabela de exemplos da seção "Symbol" em docs/domain/order.md e o ADR 0007.
/// </summary>
public class SymbolTests
{
    // [Theory] executa o mesmo teste uma vez para cada [InlineData].
    // Útil quando a regra é a mesma e só os dados mudam.
    [Theory]
    [InlineData("PETR4", "PETR", 4, false)]
    [InlineData("PETR4F", "PETR", 4, true)]
    [InlineData("VALE3", "VALE", 3, false)]
    [InlineData("B3SA3", "B3SA", 3, false)]     // raiz com dígito
    [InlineData("M1TA34", "M1TA", 34, false)]   // BDR com raiz com dígito
    [InlineData("TAEE11F", "TAEE", 11, true)]   // unit no fracionário
    [InlineData("BOVA11", "BOVA", 11, false)]   // ETF
    [InlineData("IVVB39", "IVVB", 39, false)]   // BDR de ETF
    public void Constructor_ValidCashMarketSymbol_ExposesItsParts(
        string value, string expectedRoot, int expectedTypeCode, bool expectedIsFractional)
    {
        // Act
        var symbol = new Symbol(value);

        // Assert
        Assert.Equal(value, symbol.Value);
        Assert.Equal(expectedRoot, symbol.Root);
        Assert.Equal(expectedTypeCode, symbol.TypeCode);
        Assert.Equal(expectedIsFractional, symbol.IsFractional);
    }

    [Theory]
    [InlineData(" petr4f ", "PETR4F")]
    [InlineData("vale3", "VALE3")]
    [InlineData("\tB3sa3\n", "B3SA3")]
    public void Constructor_LowercaseOrPaddedValue_IsNormalized(string value, string expected)
    {
        var symbol = new Symbol(value);

        Assert.Equal(expected, symbol.Value);
    }

    [Theory]
    [InlineData("PETR04")]    // zero à esquerda no código
    [InlineData("PETR12")]    // código de tipo não suportado
    [InlineData("PETR99")]    // código de tipo não suportado
    [InlineData("PETR")]      // sem código de tipo
    [InlineData("PE TR4")]    // espaço no meio
    [InlineData("1ABC3")]     // raiz começando com dígito
    [InlineData("PETR4FF")]   // sufixo duplicado
    [InlineData("PETR$4")]    // caractere inválido
    [InlineData("WINZ25")]    // futuro: fora do escopo (ADR 0007)
    [InlineData("PETRA290")]  // opção: fora do escopo (ADR 0007)
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidValue_ThrowsInvalidSymbolException(string value)
    {
        Assert.Throws<InvalidSymbolException>(() => new Symbol(value));
    }

    [Fact]
    public void Constructor_NullValue_ThrowsInvalidSymbolException()
    {
        Assert.Throws<InvalidSymbolException>(() => new Symbol(null!));
    }

    [Fact]
    public void Equality_SameNormalizedValue_AreEqual()
    {
        // Value Objects são comparados por valor, não por referência.
        var a = new Symbol("petr4");
        var b = new Symbol("PETR4");

        Assert.Equal(a, b);
        Assert.NotSame(a, b);
    }
}
