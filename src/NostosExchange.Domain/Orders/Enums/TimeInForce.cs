namespace NostosExchange.Domain.Orders.Enums;

/// <summary>
/// Validade da ordem. O comportamento de cada valor depende do estado do livro
/// e, por isso, é implementado pelo matching engine, não por este tipo.
/// </summary>
public enum TimeInForce : byte
{
    /// <summary>Válida até o fim do pregão.</summary>
    Day = 1,

    /// <summary>Good Till Canceled: válida até ser cancelada.</summary>
    Gtc = 2,

    /// <summary>Immediate Or Cancel: executa o que for possível e cancela o restante.</summary>
    Ioc = 3,

    /// <summary>Fill Or Kill: executa integralmente de imediato ou é cancelada por completo.</summary>
    Fok = 4
}
