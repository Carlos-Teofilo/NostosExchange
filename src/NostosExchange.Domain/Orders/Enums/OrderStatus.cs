namespace NostosExchange.Domain.Orders.Enums;

public enum OrderStatus : byte
{
    /// <summary>Criada pela corretora, ainda não aceita pela bolsa.</summary>
    PendingNew = 1,

    /// <summary>Aceita pela bolsa e ativa no livro.</summary>
    New = 2,

    PartiallyFilled = 3,
    Filled = 4,
    Canceled = 5,
    Rejected = 6,

    /// <summary>Substituída por uma nova ordem (alteração de preço ou quantidade).</summary>
    Replaced = 7
}
