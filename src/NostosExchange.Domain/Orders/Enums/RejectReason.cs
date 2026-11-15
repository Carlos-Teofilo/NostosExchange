namespace NostosExchange.Domain.Orders.Enums;

/// <summary>
/// Motivo da recusa de uma ordem antes de ela entrar no livro (Res. CVM 35, art. 21, § 1º, V).
/// </summary>
public enum RejectReason : byte
{
    /// <summary>Cliente não cadastrado ou com cadastro desatualizado (Res. CVM 35, art. 37, II).</summary>
    ClientNotEligible = 1,

    /// <summary>Ordem excede os limites operacionais do cliente (Res. CVM 35, art. 16, § 1º, II).</summary>
    RiskLimitExceeded = 2,

    /// <summary>O ativo não existe ou não está disponível para negociação.</summary>
    UnknownSymbol = 3,

    /// <summary>Ordem recebida fora do horário de negociação.</summary>
    MarketClosed = 4,

    /// <summary>Já existe uma ordem com o mesmo <c>ClOrdId</c> para o cliente (ADR 0001).</summary>
    DuplicateClOrdId = 5
}
