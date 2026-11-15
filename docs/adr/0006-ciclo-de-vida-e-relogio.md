# 0006 — Aceite pela bolsa via método e `TimeProvider`

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

`ExchangeReceivedAt` é carimbado quando a ordem **chega à bolsa**, depois de ela já ter sido criada pela corretora. Exigir esse valor na criação forçaria quem cria a ordem a inventar um instante que ainda não aconteceu.

## Decisão

- A ordem nasce por `Order.Create(...)` com `Status = PendingNew` e `ExchangeReceivedAt = null`. O construtor é privado: a factory é a única forma de criar uma ordem.
- O aceite é um comportamento da entidade: `AcceptByExchange(TimeProvider clock)`.
  - Só é permitido a partir de `PendingNew`; caso contrário, lança `InvalidOrderStateException`.
  - Carimba `ExchangeReceivedAt` com `clock.GetUtcNow()` e exige que seja `>= BrokerReceivedAt`.
  - Leva a ordem para `New`.
- O relógio é injetado por meio de **`TimeProvider`** (.NET 8+), não lido de `DateTimeOffset.UtcNow`. Os testes usam `FakeTimeProvider` para controlar o tempo.
- Timestamps são `DateTimeOffset` em UTC, com resolução de 100 ns.

## Consequências

- O status e os timestamps contam a mesma história: uma ordem `PendingNew` não tem `ExchangeReceivedAt`, e uma ordem `New` tem.
- Relógios dessincronizados entre corretora e bolsa podem fazer `ExchangeReceivedAt < BrokerReceivedAt`. Hoje isso é rejeitado. Se virar um problema real, a alternativa é registrar o desvio em vez de rejeitar a ordem.
