using NostosExchange.Domain.Shared.DomainExceptions;

namespace NostosExchange.Domain.Orders.DomainExceptions;

public class InvalidSymbolException(string message) : DomainException(message);
