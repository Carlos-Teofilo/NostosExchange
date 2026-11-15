# 0002 — Hierarquia de exceções de domínio

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

Exceções genéricas (`ArgumentException`, `InvalidOperationException`) não distinguem uma regra de negócio violada de um erro de programação. As camadas externas, como API e gateway FIX, precisam dessa distinção para traduzir o erro corretamente (por exemplo, em `400`/`Reject` em vez de `500`).

## Decisão

- Base única: `DomainException` (`NostosExchange.Domain.Shared.DomainExceptions`).
- Exceções específicas por conceito, ficando cada uma junto do seu agregado:

| Exceção | Quando ocorre |
| --- | --- |
| `InvalidPriceException` | `Price` inválido |
| `InvalidQuantityException` | `Quantity` inválida |
| `InvalidSymbolException` | `Symbol` inválido |
| `InvalidOrderException` | Invariante entre campos da `Order` violada |
| `InvalidOrderStateException` | Transição de estado não permitida |

- `ArgumentNullException` continua em uso para **erros de programação** (passar `null` onde o tipo não aceita). Isso não é regra de negócio.

## Consequências

- Um único `catch (DomainException)` na borda converte qualquer violação de regra em resposta de negócio.
- O nome da exceção já indica o conceito violado, o que facilita testes e logs.
