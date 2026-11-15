# 0010 — Arquitetura orientada a eventos com Event Sourcing

- **Status:** Aceita
- **Data:** 2026-11-16 (proposta e aceite)

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

Havia duas opções:

| Opção | Como funciona | A favor | Contra |
| --- | --- | --- | --- |
| **Event Sourcing** | Os eventos **são** a fonte da verdade. O estado é reconstruído reaplicando os eventos. | Histórico completo e reconstituível por construção; atende diretamente à Res. 135, art. 107, § 2º. | Mais complexo: versionamento de eventos, snapshots, projeções para leitura. |
| **Estado + eventos de domínio** | O estado atual é a fonte da verdade, e os eventos são publicados a cada mudança. | Mais simples de começar e de consultar. | O histórico depende de os eventos serem guardados à parte; estado e eventos podem divergir. |

## Decisão

Adotar **Event Sourcing**. Os eventos são a única fonte da verdade sobre os agregados.

### Regras

1. **O estado só muda por eventos.** Cada método público de um agregado (por exemplo, `Order.Cancel`) segue sempre três passos:

   ```text
   1. valida as regras de negócio      → pode lançar DomainException
   2. cria o evento (o fato)           → OrderCanceled { ... }
   3. Apply(evento)                    → único lugar que altera o estado
   ```

2. **O `Apply` nunca valida nem falha.** O evento representa algo que **já aconteceu**. Validar no `Apply` impediria reconstruir agregados a partir de eventos antigos, se uma regra mudasse depois.
3. **Eventos são imutáveis, append-only e nomeados no passado** (`OrderCreated`, `OrderAccepted`, `OrderRejected`, `OrderCanceled`, `OrderReplaced`). Um evento gravado nunca é editado nem apagado; correções viram eventos novos.
4. **Reconstrução** (*rehydration*): um agregado é recriado aplicando o seu histórico em ordem, por meio de `Order.FromHistory(eventos)`.
5. **Metadados de todo evento:**
   - `EventId`: identificador único do evento;
   - `OccurredAt`: instante do fato, vindo do `TimeProvider`;
   - **versão do agregado**: posição do evento no histórico daquela ordem (1, 2, 3...). É a base da **concorrência otimista**: se dois comandos tentarem gravar a versão 4 ao mesmo tempo, o segundo é rejeitado.
6. **Eventos não carregam dados cadastrais.** Só identificadores (`ClientId`, `TraderId`), conforme a [LGPD](../regulatory/lgpd.md).
7. **Determinismo:** aplicar a mesma sequência de eventos sempre produz o mesmo estado. Nada de `DateTime.Now` ou `Guid.NewGuid()` dentro do `Apply`; tudo o que varia vem dentro do evento.
8. **Event store inicial em memória**, implementado no próprio projeto, para fins de aprendizado.

## Consequências

- ✅ O histórico exigido pelas normas deixa de ser uma funcionalidade extra: **é** o modelo de persistência.
- ✅ É possível responder "qual era o estado às 13:02?" reaplicando os eventos até esse instante.
- ✅ Os eventos são a base natural para publicar market data (Res. 135, art. 119) e alimentar o monitoramento de PLD.
- ❌ A `Order` precisa ser refatorada para o padrão "validar → criar evento → `Apply`".
- ❌ Consultas ("todas as ordens ativas do cliente X") não podem ser feitas direto nos eventos; exigem **projeções**.
- ❌ Mudar o formato de um evento já gravado exige estratégia de evolução (*upcasting*).

## Ordem de implementação

1. **Testes** das regras atuais da `Order`, antes da migração. Eles provam que a refatoração não quebrou nenhuma regra.
2. Refatorar a `Order` para Event Sourcing, com os eventos e `FromHistory`.
3. Event store em memória, com concorrência otimista.

## Decisões futuras (ADRs próprios)

- Tecnologia de persistência do event store (por exemplo, KurrentDB ou Marten sobre PostgreSQL).
- Projeções e CQRS para leitura.
- Snapshots, se a reconstrução ficar lenta.
- Evolução e versionamento de eventos (*upcasting*).
- Número de sequência **global** para a ordem de entrada no livro ([Res. CVM 135, art. 119, § 1º](../regulatory/cvm-135.md)), que é diferente da versão de cada agregado.
