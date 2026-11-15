using NostosExchange.Domain.Shared.DomainExceptions;

namespace NostosExchange.Domain.Orders.DomainExceptions;

/// <summary>Violação de uma invariante que envolve mais de um campo da ordem.</summary>
public class InvalidOrderException(string message) : DomainException(message);
