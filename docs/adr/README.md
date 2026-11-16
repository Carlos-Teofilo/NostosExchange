# Architecture Decision Records

Cada ADR registra uma decisão de arquitetura: o contexto, o que foi decidido e as consequências. Uma decisão revista não é apagada: ela recebe o status **Substituída** e passa a apontar para a ADR nova.

| # | Decisão | Status |
| --- | --- | --- |
| [0001](0001-clordid-como-chave-de-idempotencia.md) | `ClOrdId` como identidade e chave de idempotência | Aceita |
| [0002](0002-hierarquia-de-excecoes-de-dominio.md) | Hierarquia de exceções de domínio | Aceita |
| [0003](0003-enums-simples.md) | Enums simples (inclusive `TimeInForce`) | Aceita |
| [0004](0004-representacao-numerica-de-quantidade-e-preco.md) | `Quantity` como `long`, `Price` como `decimal` | Aceita |
| [0005](0005-value-objects-como-record-class.md) | Value Objects como `record` (classe) | Aceita |
| [0006](0006-ciclo-de-vida-e-relogio.md) | Aceite pela bolsa via método e `TimeProvider` | Aceita |
| [0007](0007-symbol-no-padrao-b3-mercado-a-vista.md) | `Symbol` no padrão B3, começando pelo mercado à vista | Aceita |
| [0008](0008-normas-da-cvm-como-fonte-de-requisitos.md) | Regulação do mercado como fonte de requisitos | Aceita |
| [0009](0009-recusa-cancelamento-e-alteracao.md) | Recusa, cancelamento e alteração (cancel/replace) | Aceita |
| [0010](0010-arquitetura-orientada-a-eventos.md) | Arquitetura orientada a eventos com Event Sourcing | Aceita |
| [0011](0011-estrategia-de-testes.md) | Estratégia de testes (xUnit v3, `FakeTimeProvider`, Test Data Builder) | Aceita |

## Modelo

```markdown
# NNNN — Título

- **Status:** Proposta | Aceita | Substituída por NNNN
- **Data:** AAAA-MM-DD

## Contexto
## Decisão
## Consequências
## Alternativas consideradas
```
