namespace NostosExchange.Domain.Orders.Enums;

/// <summary>Indica se a decisão de enviar a ordem foi tomada por uma pessoa ou por um sistema.</summary>
public enum ExecutionMethod : byte
{
    Automated = 1,
    Manual = 2
}
