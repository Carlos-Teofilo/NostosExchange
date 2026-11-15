# 0010 — Arquitetura orientada a eventos

- **Status:** Proposta
- **Data:** 2026-11-16

## Contexto

O NostosExchange tem como objetivo uma **arquitetura orientada a eventos**: cada mudança relevante no domínio é registrada como um fato imutável (um evento de domínio), e as outras partes do sistema reagem a esses fatos.

Além de ser um objetivo técnico do projeto, as normas mapeadas apontam na mesma direção:

| Exigência | Norma |
| --- | --- |
| Registrar **toda** operação, independentemente do valor | [Res. CVM 50, art. 25](../regulatory/pld.md) |
| Manter registros e trilhas de auditoria por **no mínimo 5 anos** | [Res. CVM 35, art. 48](../regulatory/cvm-35.md); [Res. CVM 135, art. 186](../regulatory/cvm-135.md); [Lei 9.613, art. 10, § 2º](../regulatory/pld.md) |
| Armazenar ofertas e operações de forma que permita sua **consulta ou reconstituição** | [Res. CVM 135, art. 107, § 2º](../regulatory/cvm-135.md) |
| Vincular, a qualquer tempo, a ordem à oferta e ao negócio | [Res. CVM 35, art. 21, II](../regulatory/cvm-35.md) |
| Dar pronta **visibilidade e disseminação pública** às ofertas e negócios | [Res. CVM 135, art. 119, II e III](../regulatory/cvm-135.md) |
| Tratamento equitativo, que na prática exige **determinismo** | [Res. CVM 62, art. 2º, IV](../regulatory/cvm-62.md) |

## Decisão (proposta)

- Cada transição de estado de um agregado produz um **evento de domínio** imutável, nomeado no passado (por exemplo, `OrderCreated`, `OrderAccepted`, `OrderRejected`, `OrderCanceled`, `OrderReplaced`).
- Os eventos carregam apenas identificadores de pessoas (`ClientId`, `TraderId`), nunca dados cadastrais ([LGPD](../regulatory/lgpd.md)).
- O matching engine deve ser **determinístico**: a mesma sequência de eventos de entrada sempre produz os mesmos eventos de saída.

## Em aberto

A decisão principal ainda precisa ser tomada:

| Opção | Como funciona | A favor | Contra |
| --- | --- | --- | --- |
| **Event Sourcing** | Os eventos **são** a fonte da verdade. O estado da `Order` é reconstruído reaplicando os eventos. | Histórico completo e reconstituível por construção; encaixa diretamente na Res. 135, art. 107, § 2º. | Mais complexo: versionamento de eventos, snapshots, projeções para leitura. |
| **Estado + eventos de domínio** | O estado atual é a fonte da verdade, e os eventos são publicados a cada mudança (por exemplo, via Transactional Outbox). | Mais simples de começar e de consultar. | O histórico depende de os eventos serem guardados à parte; é possível o estado e os eventos divergirem se houver erro. |

Este ADR passa a **Aceita** quando essa escolha for feita, e será atualizado com a opção escolhida e suas consequências.
