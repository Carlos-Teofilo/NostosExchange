# 0001 — `ClOrdId` como identidade e chave de idempotência

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

Quando a rede falha, o cliente não sabe se a ordem chegou e reenvia. Se o servidor gerar o identificador (`IDENTITY` no banco), o reenvio vira uma **segunda ordem**, e no mercado financeiro isso significa exposição em dobro.

## Decisão

- A `Order` usa o **`ClOrdId` (`Guid`)**, gerado pelo cliente ou gateway, como identidade de negócio.
- O mesmo valor serve de **chave de idempotência**: uma ordem com `ClOrdId` já conhecido é tratada como reenvio, não como ordem nova.
- Na persistência, a restrição de unicidade será **`(ClientId, ClOrdId)`**. No protocolo FIX, o `ClOrdID` é único apenas dentro da sessão do cliente. Usar a chave composta deixa o modelo fiel ao protocolo e não custa nada a mais com `Guid`.

## Consequências

- O cliente é responsável por gerar o identificador e por reutilizá-lo nos reenvios.
- A entidade nasce com identidade, sem depender do banco de dados.
- Se o sistema passar a se integrar com FIX real, em que `ClOrdID` é `string`, o tipo precisará ser revisto. A bolsa também deverá gerar o próprio `OrderId`, como acontece nas bolsas reais.

## Alternativas consideradas

- **ID gerado pelo banco:** descartado, porque não oferece idempotência.
- **Hash do conteúdo da ordem:** descartado, porque duas ordens idênticas e legítimas teriam o mesmo hash.
