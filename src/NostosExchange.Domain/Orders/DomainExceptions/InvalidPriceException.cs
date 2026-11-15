using NostosExchange.Domain.Shared.DomainExceptions;

namespace NostosExchange.Domain.Orders.DomainExceptions;

public sealed class InvalidPriceException(string message) : DomainException(message);
