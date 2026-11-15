namespace NostosExchange.Domain.Orders.Enums;

/// <summary>Canal por onde a ordem entrou na corretora.</summary>
public enum OrderSource : byte
{
    HomeBroker = 1,
    TradingDesk = 2,
    Algorithm = 3,
    MobileApp = 4,
    FixApi = 5
}
