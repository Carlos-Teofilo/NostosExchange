using NostosExchange.Domain.Orders.DomainExceptions;
using NostosExchange.Domain.Orders.Enums;
using NostosExchange.Domain.Orders.ValueObjects;

namespace NostosExchange.Domain.Orders.Entities;

/// <summary>
/// Raiz do agregado de ordem. Toda instância válida respeita as invariantes
/// documentadas em docs/domain/order.md.
/// </summary>
public sealed class Order
{
    /// <summary>Client Order ID: identidade de negócio e chave de idempotência (ver ADR 0001).</summary>
    public Guid ClOrdId { get; }

    public Guid ClientId { get; }

    /// <summary>Preenchido somente quando a ordem passa pela mesa de operação.</summary>
    public Guid? TraderId { get; }

    public Symbol Symbol { get; }

    public Side Side { get; }

    public OrderType OrderType { get; }

    public Quantity Quantity { get; }

    /// <summary>Obrigatório para <see cref="OrderType.Limit"/>; nulo para <see cref="OrderType.Market"/>.</summary>
    public Price? Price { get; }

    public TimeInForce TimeInForce { get; }

    public OrderSource OrderSource { get; }

    public ExecutionMethod ExecutionMethod { get; }

    /// <summary>Momento em que a corretora recebeu a ordem (UTC).</summary>
    public DateTimeOffset BrokerReceivedAt { get; }

    /// <summary>Momento em que a bolsa aceitou a ordem (UTC). Nulo enquanto <see cref="OrderStatus.PendingNew"/>.</summary>
    public DateTimeOffset? ExchangeReceivedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public long FilledQuantity { get; private set; }

    public long LeavesQuantity => Quantity.Value - FilledQuantity;

    private Order(
        Guid clOrdId,
        Guid clientId,
        Guid? traderId,
        Symbol symbol,
        Side side,
        OrderType orderType,
        Quantity quantity,
        Price? price,
        TimeInForce timeInForce,
        OrderSource orderSource,
        ExecutionMethod executionMethod,
        DateTimeOffset brokerReceivedAt)
    {
        ClOrdId = clOrdId;
        ClientId = clientId;
        TraderId = traderId;
        Symbol = symbol;
        Side = side;
        OrderType = orderType;
        Quantity = quantity;
        Price = price;
        TimeInForce = timeInForce;
        OrderSource = orderSource;
        ExecutionMethod = executionMethod;
        BrokerReceivedAt = brokerReceivedAt;
        Status = OrderStatus.PendingNew;
    }

    /// <summary>
    /// Cria uma ordem recebida pela corretora, ainda não enviada à bolsa.
    /// </summary>
    /// <exception cref="InvalidOrderException">Quando alguma invariante da ordem é violada.</exception>
    public static Order Create(
        Guid clOrdId,
        Guid clientId,
        Guid? traderId,
        Symbol symbol,
        Side side,
        OrderType orderType,
        Quantity quantity,
        Price? price,
        TimeInForce timeInForce,
        OrderSource orderSource,
        ExecutionMethod executionMethod,
        DateTimeOffset brokerReceivedAt)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        ArgumentNullException.ThrowIfNull(quantity);

        EnsureIdentifiers(clOrdId, clientId, traderId);
        EnsureDefined(side, nameof(Side));
        EnsureDefined(orderType, nameof(OrderType));
        EnsureDefined(timeInForce, nameof(TimeInForce));
        EnsureDefined(orderSource, nameof(OrderSource));
        EnsureDefined(executionMethod, nameof(ExecutionMethod));
        EnsurePriceMatchesType(orderType, price, timeInForce);
        EnsureOriginConsistency(traderId, orderSource, executionMethod);

        if (brokerReceivedAt == default)
        {
            throw new InvalidOrderException("BrokerReceivedAt must be informed.");
        }

        return new Order(
            clOrdId,
            clientId,
            traderId,
            symbol,
            side,
            orderType,
            quantity,
            price,
            timeInForce,
            orderSource,
            executionMethod,
            brokerReceivedAt.ToUniversalTime());
    }

    /// <summary>
    /// Registra o aceite da ordem pela bolsa, carimbando <see cref="ExchangeReceivedAt"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Quando a ordem não está em <see cref="OrderStatus.PendingNew"/>.</exception>
    /// <exception cref="InvalidOrderException">Quando o relógio da bolsa indica um instante anterior ao recebimento pela corretora.</exception>
    public void AcceptByExchange(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (Status != OrderStatus.PendingNew)
        {
            throw new InvalidOrderStateException(
                $"Only orders in {OrderStatus.PendingNew} can be accepted by the exchange. Current status: {Status}.");
        }

        var receivedAt = clock.GetUtcNow();

        if (receivedAt < BrokerReceivedAt)
        {
            throw new InvalidOrderException("ExchangeReceivedAt cannot be earlier than BrokerReceivedAt.");
        }

        ExchangeReceivedAt = receivedAt;
        Status = OrderStatus.New;
    }

    private static void EnsureIdentifiers(Guid clOrdId, Guid clientId, Guid? traderId)
    {
        if (clOrdId == Guid.Empty)
        {
            throw new InvalidOrderException("ClOrdId must be informed.");
        }

        if (clientId == Guid.Empty)
        {
            throw new InvalidOrderException("ClientId must be informed.");
        }

        if (traderId == Guid.Empty)
        {
            throw new InvalidOrderException("TraderId, when informed, cannot be empty.");
        }
    }

    private static void EnsureDefined<TEnum>(TEnum value, string name)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new InvalidOrderException($"{name} has an invalid value: {value}.");
        }
    }

    private static void EnsurePriceMatchesType(OrderType orderType, Price? price, TimeInForce timeInForce)
    {
        switch (orderType)
        {
            case OrderType.Limit when price is null:
                throw new InvalidOrderException("Limit orders require a price.");

            case OrderType.Market when price is not null:
                throw new InvalidOrderException("Market orders cannot have a price.");

            case OrderType.Market when timeInForce is not (TimeInForce.Ioc or TimeInForce.Fok):
                throw new InvalidOrderException("Market orders only accept Ioc or Fok as TimeInForce.");
        }
    }

    private static void EnsureOriginConsistency(Guid? traderId, OrderSource orderSource, ExecutionMethod executionMethod)
    {
        var isTradingDesk = orderSource == OrderSource.TradingDesk;

        if (isTradingDesk && traderId is null)
        {
            throw new InvalidOrderException("Orders from the trading desk require a TraderId.");
        }

        if (!isTradingDesk && traderId is not null)
        {
            throw new InvalidOrderException("TraderId is only allowed for orders from the trading desk.");
        }

        if (orderSource == OrderSource.Algorithm && executionMethod != ExecutionMethod.Automated)
        {
            throw new InvalidOrderException("Orders from an algorithm must use the Automated execution method.");
        }
    }
}
