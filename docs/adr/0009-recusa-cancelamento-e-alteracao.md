# 0009 — Recusa, cancelamento e alteração de ordens

- **Status:** Aceita
- **Data:** 2026-11-16

## Contexto

A Res. CVM 35 (art. 21, § 1º, V e VII) exige que as regras do intermediário tratem de **procedimentos de recusa** e de **cancelamento ou alteração de ordens**, e que essas regras sejam claras e divulgadas previamente (§ 3º). A norma não diz *como* fazer: ela delega a definição a quem opera. Por isso, as regras abaixo são as **regras próprias do NostosExchange**, publicadas em [docs/domain/order.md](../domain/order.md#regras-de-recusa-cancelamento-e-alteração).

A alteração é a parte delicada, por dois motivos:

1. O `ClOrdId` é a identidade da ordem e a chave de idempotência ([ADR 0001](0001-clordid-como-chave-de-idempotencia.md)). Um pedido de alteração também pode ser reenviado em falhas de rede, e por isso também precisa de uma chave de idempotência própria.
2. Alterar o preço ou aumentar a quantidade muda a posição da oferta na fila de prioridade preço-tempo ([Res. CVM 135, art. 119, § 1º](../regulatory/cvm-135.md)).

## Decisão

- **Recusa** (`Reject`): só a partir de `PendingNew`, ou seja, antes de a ordem entrar no livro. O motivo é obrigatório (`RejectReason`).
- **Cancelamento** (`Cancel`): a partir de qualquer status ativo (`PendingNew`, `New`, `PartiallyFilled`). Cancela apenas o saldo; a quantidade já executada não é desfeita. O motivo é obrigatório (`CancelReason`), o que cobre também os cancelamentos automáticos do `TimeInForce` (IOC, FOK, Day).
- **Alteração** (`Replace`): segue o modelo **cancel/replace do protocolo FIX**.
  - A ordem original vai para `Replaced` (status final).
  - Uma **nova ordem**, com um novo `ClOrdId` gerado pelo cliente, é devolvida em `PendingNew` e precisa ser aceita pela bolsa. `OrigClOrdId` aponta para a original.
  - Só ordens **limitadas** que estão **no livro** (`New` ou `PartiallyFilled`) podem ser alteradas.
  - Só **preço e quantidade** podem mudar. Para mudar lado, ativo ou validade, o cliente cancela e envia uma ordem nova.
  - A quantidade informada é a **total**, incluindo o que já foi executado (como o `OrderQty` do FIX). Ela deve ser maior que `FilledQuantity`, e a nova ordem herda `FilledQuantity`.
  - O `BrokerReceivedAt` da nova ordem é o instante em que a corretora recebeu o pedido de alteração.
- Toda transição para um status final registra `ClosedAt`. Todo instante lido do relógio deve ser `>=` ao último evento da ordem.

## Consequências

- ✅ A identidade nunca muda: cada `ClOrdId` corresponde a uma única versão da ordem, e o histórico de alterações fica encadeado por `OrigClOrdId`.
- ✅ O vínculo ordem → alteração → negócio fica rastreável ([Res. CVM 35, art. 21, II](../regulatory/cvm-35.md)).
- ✅ Na fila preço-tempo, a nova ordem recebe um novo `ExchangeReceivedAt` e, portanto, vai para o fim da fila no seu preço.
- ❌ Algumas bolsas **mantêm a prioridade** quando a alteração apenas **reduz** a quantidade. Este modelo sempre perde a prioridade. Se essa regra for adotada, ela caberá ao livro de ofertas, que pode reaproveitar a posição da ordem original.
- ❌ Entre o pedido e o aceite da bolsa, a original já está `Replaced` e a nova ainda está `PendingNew`. Se a bolsa recusar a alteração, a ordem original não volta ao livro. O FIX resolve isso com um status intermediário (`PendingReplace`); aqui, isso fica para quando houver integração real com a bolsa.

## Alternativas consideradas

- **Alteração no próprio objeto** (mudar `Price`/`Quantity` da mesma `Order`): descartada. Ou o `ClOrdId` mudaria, quebrando a identidade da entidade, ou o pedido de alteração ficaria sem chave de idempotência.
- **Permitir alterar ordens `PendingNew`:** descartada por ora. Antes do aceite, o cliente pode cancelar e reenviar.
