using System.Collections.Frozen;
using System.Globalization;
using System.Text.RegularExpressions;

using NostosExchange.Domain.Orders.DomainExceptions;

namespace NostosExchange.Domain.Orders.ValueObjects;

/// <summary>
/// Código de negociação do mercado à vista da B3 (ex.: PETR4, B3SA3, TAEE11, AAPL34, PETR4F).
/// Formato: raiz de 4 caracteres + código do tipo de papel + sufixo "F" opcional (mercado fracionário).
/// O valor é normalizado para maiúsculas e sem espaços nas extremidades. Ver ADR 0007.
/// </summary>
public partial record Symbol
{
    public const int MaxLength = 7;

    /// <summary>
    /// Códigos de tipo de papel aceitos:
    /// 1–2 direitos de subscrição; 3 ON; 4 PN; 5–8 PN classes A–D; 9–10 recibos de subscrição;
    /// 11 units, FIIs e ETFs; 31–35 BDRs; 39 BDRs de ETF.
    /// </summary>
    private static readonly FrozenSet<int> ValidTypeCodes =
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 31, 32, 33, 34, 35, 39 }.ToFrozenSet();

    /// <summary>Código completo, como negociado (ex.: PETR4F).</summary>
    public string Value { get; }

    /// <summary>Raiz de 4 caracteres que identifica o emissor (ex.: PETR, B3SA).</summary>
    public string Root { get; }

    /// <summary>Número que identifica o tipo de papel (ex.: 3 para ON, 4 para PN, 11 para units/FIIs/ETFs).</summary>
    public int TypeCode { get; }

    /// <summary>Indica se o código é do mercado fracionário (sufixo "F").</summary>
    public bool IsFractional { get; }

    public Symbol(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        var match = Format().Match(normalized);

        if (!match.Success)
        {
            throw new InvalidSymbolException(
                $"'{value}' is not a valid B3 cash market symbol. " +
                "Expected a 4-character root, a type code and an optional 'F' suffix (e.g. PETR4, B3SA3, TAEE11F).");
        }

        var typeCode = int.Parse(match.Groups["code"].ValueSpan, NumberStyles.None, CultureInfo.InvariantCulture);

        if (!ValidTypeCodes.Contains(typeCode))
        {
            throw new InvalidSymbolException($"'{typeCode}' is not a valid B3 security type code.");
        }

        Value = normalized;
        Root = match.Groups["root"].Value;
        TypeCode = typeCode;
        IsFractional = match.Groups["fractional"].Success;
    }

    public override string ToString() => Value;

    // Raiz começa com letra e pode conter dígitos (B3SA, M1TA). Código sem zero à esquerda (rejeita PETR04).
    [GeneratedRegex("^(?<root>[A-Z][A-Z0-9]{3})(?<code>[1-9][0-9]?)(?<fractional>F)?$")]
    private static partial Regex Format();
}
