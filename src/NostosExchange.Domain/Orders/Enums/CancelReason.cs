namespace NostosExchange.Domain.Orders.Enums;

/// <summary>
/// Motivo do cancelamento de uma ordem (Res. CVM 35, art. 21, § 1º, VII).
/// </summary>
public enum CancelReason : byte
{
    /// <summary>Cancelamento solicitado pelo cliente.</summary>
    ClientRequest = 1,

    /// <summary>Fim da validade (ex.: ordem <see cref="TimeInForce.Day"/> no encerramento do pregão).</summary>
    Expired = 2,

    /// <summary>Saldo não executado de uma ordem <see cref="TimeInForce.Ioc"/>.</summary>
    UnfilledRemainder = 3,

    /// <summary>Ordem <see cref="TimeInForce.Fok"/> sem liquidez para execução integral.</summary>
    NotFullyFillable = 4,

    /// <summary>Cancelamento determinado pela bolsa (ex.: procedimento especial de negociação, Res. CVM 135, art. 120, IV).</summary>
    ExchangeAction = 5
}
