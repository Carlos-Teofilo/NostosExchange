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

    /// <summary>
    /// <c>ClOrdId</c> da ordem substituída, quando esta ordem nasceu de uma alteração (<see cref="Replace"/>).
    /// </summary>
    public Guid? OrigClOrdId { get; }

    /// <summary>Momento em que a ordem atingiu um status final (UTC). Nulo enquanto ativa.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>Preenchido quando <see cref="Status"/> é <see cref="OrderStatus.Rejected"/>.</summary>
    public RejectReason? RejectReason { get; private set; }

    /// <summary>Preenchido quando <see cref="Status"/> é <see cref="OrderStatus.Canceled"/>.</summary>
    public CancelReason? CancelReason { get; private set; }

    /// <summary>Indica se a ordem ainda pode ser executada, cancelada ou alterada.</summary>
    public bool IsActive => Status is OrderStatus.PendingNew or OrderStatus.New or OrderStatus.PartiallyFilled;

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
        DateTimeOffset brokerReceivedAt,
        Guid? origClOrdId = null,
        long filledQuantity = 0)
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
        OrigClOrdId = origClOrdId;
        FilledQuantity = filledQuantity;
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
        if (Status != OrderStatus.PendingNew)
        {
            throw new InvalidOrderStateException(
                $"Only orders in {OrderStatus.PendingNew} can be accepted by the exchange. Current status: {Status}.");
        }

        ExchangeReceivedAt = Now(clock);

        // Uma ordem que nasceu de uma alteração herda a quantidade já executada da original.
        Status = FilledQuantity > 0 ? OrderStatus.PartiallyFilled : OrderStatus.New;
    }

    /// <summary>
    /// Recusa a ordem antes de ela entrar no livro (Res. CVM 35, art. 21, § 1º, V).
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Quando a ordem não está em <see cref="OrderStatus.PendingNew"/>.</exception>
    public void Reject(RejectReason reason, TimeProvider clock)
    {
        EnsureDefined(reason, nameof(Enums.RejectReason));

        if (Status != OrderStatus.PendingNew)
        {
            throw new InvalidOrderStateException(
                $"Only orders in {OrderStatus.PendingNew} can be rejected. Current status: {Status}.");
        }

        Close(OrderStatus.Rejected, Now(clock));
        RejectReason = reason;
    }

    /// <summary>
    /// Cancela o saldo não executado da ordem (Res. CVM 35, art. 21, § 1º, VII).
    /// A quantidade já executada (<see cref="FilledQuantity"/>) não é desfeita.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Quando a ordem já está em um status final.</exception>
    public void Cancel(CancelReason reason, TimeProvider clock)
    {
        EnsureDefined(reason, nameof(Enums.CancelReason));
        EnsureActive("canceled");

        Close(OrderStatus.Canceled, Now(clock));
        CancelReason = reason;
    }

    /// <summary>
    /// Altera o preço e/ou a quantidade de uma ordem que está no livro (Res. CVM 35, art. 21, § 1º, VII).
    /// Segue o modelo cancel/replace do FIX: esta ordem passa a <see cref="OrderStatus.Replaced"/> e uma nova
    /// ordem, com novo <c>ClOrdId</c>, é devolvida em <see cref="OrderStatus.PendingNew"/>. Ver ADR 0009.
    /// </summary>
    /// <param name="newClOrdId">Identificador da nova ordem, gerado pelo cliente (chave de idempotência da alteração).</param>
    /// <param name="newQuantity">Quantidade total da nova ordem, incluindo o que já foi executado.</param>
    /// <param name="newPrice">Novo preço limite.</param>
    /// <param name="clock">Relógio da corretora, que registra o recebimento da alteração.</param>
    /// <exception cref="InvalidOrderStateException">Quando a ordem não está ativa no livro.</exception>
    /// <exception cref="InvalidOrderException">Quando a alteração viola alguma regra.</exception>
    public Order Replace(Guid newClOrdId, Quantity newQuantity, Price newPrice, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(newQuantity);
        ArgumentNullException.ThrowIfNull(newPrice);

        if (Status is not (OrderStatus.New or OrderStatus.PartiallyFilled))
        {
            throw new InvalidOrderStateException(
                $"Only orders resting in the book ({OrderStatus.New} or {OrderStatus.PartiallyFilled}) can be replaced. Current status: {Status}.");
        }

        if (OrderType != OrderType.Limit)
        {
            throw new InvalidOrderException("Only limit orders can be replaced.");
        }

        if (newClOrdId == Guid.Empty || newClOrdId == ClOrdId)
        {
            throw new InvalidOrderException("A replacement requires a new, non-empty ClOrdId.");
        }

        if (newQuantity.Value <= FilledQuantity)
        {
            throw new InvalidOrderException("The new quantity must be greater than the quantity already filled.");
        }

        if (newQuantity == Quantity && newPrice == Price)
        {
            throw new InvalidOrderException("A replacement must change the price or the quantity.");
        }

        var receivedAt = Now(clock);

        var replacement = new Order(
            newClOrdId,
            ClientId,
            TraderId,
            Symbol,
            Side,
            OrderType,
            newQuantity,
            newPrice,
            TimeInForce,
            OrderSource,
            ExecutionMethod,
            receivedAt,
            origClOrdId: ClOrdId,
            filledQuantity: FilledQuantity);

        Close(OrderStatus.Replaced, receivedAt);

        return replacement;
    }

    private void EnsureActive(string operation)
    {
        if (!IsActive)
        {
            throw new InvalidOrderStateException($"Only active orders can be {operation}. Current status: {Status}.");
        }
    }

    private void Close(OrderStatus finalStatus, DateTimeOffset closedAt)
    {
        Status = finalStatus;
        ClosedAt = closedAt;
    }

    /// <summary>
    /// Lê o relógio e garante que o novo instante não seja anterior ao último evento registrado na ordem.
    /// </summary>
    private DateTimeOffset Now(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        var now = clock.GetUtcNow();
        var lastEvent = ExchangeReceivedAt ?? BrokerReceivedAt;

        if (now < lastEvent)
        {
            throw new InvalidOrderException("A new timestamp cannot be earlier than the last event recorded on the order.");
        }

        return now;
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
