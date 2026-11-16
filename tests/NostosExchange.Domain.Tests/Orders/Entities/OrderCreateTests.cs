using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.Enums;
using NostosExchange.Domain.Tests.Builders;

namespace NostosExchange.Domain.Tests.Orders.Entities;

/// <summary>
/// Cobre Order.Create e as invariantes da seção "Invariantes da entidade" em docs/domain/order.md.
/// </summary>
public class OrderCreateTests
{
    // Padrão Arrange-Act-Assert (AAA):
    //   Arrange: prepara o cenário.
    //   Act:     executa UMA ação, a que está sendo testada.
    //   Assert:  verifica o resultado.
    [Fact]
    public void Create_ValidLimitOrder_StartsAsPendingNew()
    {
        // Arrange
        var builder = new OrderBuilder();

        // Act
        var order = builder.Build();

        // Assert
        Assert.Equal(OrderStatus.PendingNew, order.Status);
        Assert.Null(order.ExchangeReceivedAt);
        Assert.Null(order.ClosedAt);
        Assert.Equal(0, order.FilledQuantity);
        Assert.Equal(order.Quantity.Value, order.LeavesQuantity);
        Assert.True(order.IsActive);
    }

    // Invariante 4: OrderType == Limit ⇒ Price informado.
    [Fact]
    public void Create_LimitOrderWithoutPrice_ThrowsInvalidOrderException()
    {
        // Arrange
        var builder = new OrderBuilder()
            .WithOrderType(OrderType.Limit)
            .WithPrice(null);

        // Act + Assert
        // Assert.Throws exige o tipo EXATO da exceção e a devolve para verificações extras.
        var exception = Assert.Throws<InvalidOrderException>(() => builder.Build());
        Assert.Contains("price", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Invariante 2: TraderId informado ⇔ OrderSource == TradingDesk.
    [Fact]
    public void Create_TraderIdOutsideTradingDesk_ThrowsInvalidOrderException()
    {
        var builder = new OrderBuilder()
            .WithOrderSource(OrderSource.HomeBroker)
            .WithTraderId(Guid.NewGuid());

        Assert.Throws<InvalidOrderException>(() => builder.Build());
    }

    // TODO (exercício): as demais invariantes e cenários estão em estudos/exercicios-testes.md.
}
