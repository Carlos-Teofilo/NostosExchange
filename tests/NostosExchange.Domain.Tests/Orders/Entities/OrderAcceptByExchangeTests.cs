using Microsoft.Extensions.Time.Testing;

using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.Enums;
using NostosExchange.Domain.Tests.Builders;

namespace NostosExchange.Domain.Tests.Orders.Entities;

/// <summary>
/// Cobre Order.AcceptByExchange (ADR 0006).
/// </summary>
public class OrderAcceptByExchangeTests
{
    // FakeTimeProvider é um relógio que só anda quando o teste manda.
    // Assim o teste é determinístico: roda igual hoje, amanhã e no servidor de CI.
    [Fact]
    public void AcceptByExchange_PendingNewOrder_StampsExchangeTimeAndMovesToNew()
    {
        // Arrange
        var clock = new FakeTimeProvider(OrderBuilder.DefaultBrokerReceivedAt);
        var order = new OrderBuilder().Build();
        clock.Advance(TimeSpan.FromMilliseconds(5));

        // Act
        order.AcceptByExchange(clock);

        // Assert
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(OrderBuilder.DefaultBrokerReceivedAt.AddMilliseconds(5), order.ExchangeReceivedAt);
    }

    // Invariante 7: ExchangeReceivedAt >= BrokerReceivedAt.
    [Fact]
    public void AcceptByExchange_ClockBeforeBrokerReceivedAt_ThrowsInvalidOrderException()
    {
        // Arrange: o relógio da bolsa está 1 ms ATRÁS do instante em que a corretora recebeu a ordem.
        var clock = new FakeTimeProvider(OrderBuilder.DefaultBrokerReceivedAt.AddMilliseconds(-1));
        var order = new OrderBuilder().Build();

        // Act + Assert
        Assert.Throws<InvalidOrderException>(() => order.AcceptByExchange(clock));
        Assert.Equal(OrderStatus.PendingNew, order.Status);   // a ordem não pode ter mudado
    }

    // TODO (exercício): os demais cenários estão em estudos/exercicios-testes.md.
}
