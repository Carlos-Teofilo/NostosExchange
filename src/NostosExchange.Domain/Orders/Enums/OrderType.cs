namespace NostosExchange.Domain.Orders.Enums;

public enum OrderType : byte
{
    /// <summary>Executa ao preço informado ou melhor. Exige <c>Price</c>.</summary>
    Limit = 1,

    /// <summary>Executa contra a melhor oferta disponível. Não possui <c>Price</c>.</summary>
    Market = 2
}
