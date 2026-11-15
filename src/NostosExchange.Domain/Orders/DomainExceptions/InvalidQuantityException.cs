using NostosExchange.Domain.Shared.DomainExceptions;

namespace NostosExchange.Domain.Orders.DomainExceptions;

public sealed class InvalidQuantityException(string message) : DomainException(message);
