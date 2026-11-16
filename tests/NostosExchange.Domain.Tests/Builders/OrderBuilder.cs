using NostosExchange.Domain.Orders.Entities;
using NostosExchange.Domain.Orders.Enums;
using NostosExchange.Domain.Orders.ValueObjects;

namespace NostosExchange.Domain.Tests.Builders;

/// <summary>
/// Test Data Builder: cria uma <see cref="Order"/> válida com valores padrão.
/// Cada teste altera só o que importa para o cenário, o que deixa a intenção do teste evidente.
/// </summary>
/// <example>
/// <code>
/// var order = new OrderBuilder().AsMarket().WithTimeInForce(TimeInForce.Ioc).Build();
/// </code>
/// </example>
public sealed class OrderBuilder
{
    /// <summary>Instante padrão de recebimento pela corretora. Use-o como início do <c>FakeTimeProvider</c>.</summary>
    public static readonly DateTimeOffset DefaultBrokerReceivedAt = new(2026, 11, 16, 13, 0, 0, TimeSpan.Zero);

    private Guid _clOrdId = Guid.NewGuid();
    private Guid _clientId = Guid.NewGuid();
    private Guid? _traderId;
    private Symbol _symbol = new("PETR4");
    private Side _side = Side.Buy;
    private OrderType _orderType = OrderType.Limit;
    private Quantity _quantity = new(100);
    private Price? _price = new(38.50m);
    private TimeInForce _timeInForce = TimeInForce.Day;
    private OrderSource _orderSource = OrderSource.HomeBroker;
    private ExecutionMethod _executionMethod = ExecutionMethod.Manual;
    private DateTimeOffset _brokerReceivedAt = DefaultBrokerReceivedAt;

    public OrderBuilder WithClOrdId(Guid clOrdId)
    {
        _clOrdId = clOrdId;
        return this;
    }

    public OrderBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public OrderBuilder WithTraderId(Guid? traderId)
    {
        _traderId = traderId;
        return this;
    }

    public OrderBuilder WithSymbol(string symbol)
    {
        _symbol = new Symbol(symbol);
        return this;
    }

    public OrderBuilder WithSide(Side side)
    {
        _side = side;
        return this;
    }

    public OrderBuilder WithOrderType(OrderType orderType)
    {
        _orderType = orderType;
        return this;
    }

    public OrderBuilder WithQuantity(long quantity)
    {
        _quantity = new Quantity(quantity);
        return this;
    }

    public OrderBuilder WithPrice(decimal? price)
    {
        _price = price is null ? null : new Price(price.Value);
        return this;
    }

    public OrderBuilder WithTimeInForce(TimeInForce timeInForce)
    {
        _timeInForce = timeInForce;
        return this;
    }

    public OrderBuilder WithOrderSource(OrderSource orderSource)
    {
        _orderSource = orderSource;
        return this;
    }

    public OrderBuilder WithExecutionMethod(ExecutionMethod executionMethod)
    {
        _executionMethod = executionMethod;
        return this;
    }

    public OrderBuilder WithBrokerReceivedAt(DateTimeOffset brokerReceivedAt)
    {
        _brokerReceivedAt = brokerReceivedAt;
        return this;
    }

    /// <summary>Atalho para uma ordem a mercado válida: sem preço e com <see cref="TimeInForce.Ioc"/>.</summary>
    public OrderBuilder AsMarket()
    {
        _orderType = OrderType.Market;
        _price = null;
        _timeInForce = TimeInForce.Ioc;
        return this;
    }

    /// <summary>Atalho para uma ordem da mesa de operações válida: com <c>TraderId</c>.</summary>
    public OrderBuilder FromTradingDesk()
    {
        _orderSource = OrderSource.TradingDesk;
        _traderId = Guid.NewGuid();
        return this;
    }

    public Order Build() => Order.Create(
        _clOrdId,
        _clientId,
        _traderId,
        _symbol,
        _side,
        _orderType,
        _quantity,
        _price,
        _timeInForce,
        _orderSource,
        _executionMethod,
        _brokerReceivedAt);
}
