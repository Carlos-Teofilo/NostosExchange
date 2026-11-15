using NostosExchange.Domain.Shared.DomainExceptions;

namespace NostosExchange.Domain.Orders.DomainExceptions;

/// <summary>Operação não permitida no status atual da ordem.</summary>
public class InvalidOrderStateException(string message) : DomainException(message);
