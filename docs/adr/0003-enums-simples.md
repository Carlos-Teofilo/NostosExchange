# 0003 — Enums simples (inclusive `TimeInForce`)

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

Havia a dúvida entre modelar `TimeInForce` como `enum` ou como *Smart Enum*, uma classe com instâncias estáticas e comportamento próprio.

## Decisão

- **`TimeInForce` é um `enum` simples.** O comportamento de cada valor (cancelar o restante no IOC, verificar a liquidez antes no FOK, expirar no fim do pregão no Day) depende do **estado do livro de ofertas**. Esse comportamento pertence ao matching engine, que decide com um `switch`.
- Convenções para todos os enums:
  - **Nome no singular** (`Side`, não `Sides`), conforme as diretrizes do .NET para enums sem `[Flags]`.
  - **Valores explícitos começando em 1.** Assim `default(T)` é `0`, um valor inválido, e uma ordem não inicializada é rejeitada em vez de virar uma compra silenciosa.
  - **`byte` como tipo subjacente:** menos memória por ordem no livro.
  - **Validação com `Enum.IsDefined`** na entidade, porque `(Side)99` compila.

## Consequências

- Nenhuma alocação e comparação barata no caminho crítico do matching.
- Se surgir uma regra que dependa *só* do TIF, ela pode entrar como método de extensão sem mudar o tipo.

## Alternativas consideradas

- **Smart Enum:** descartado. A lógica do livro vazaria para dentro do tipo, ou ele viraria só um rótulo com mais cerimônia e alocação.
