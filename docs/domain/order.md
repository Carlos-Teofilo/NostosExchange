# Agregado `Order`

A `Order` é a raiz do agregado de ordens. Uma instância só existe em estado válido: todas as regras abaixo são verificadas na criação (`Order.Create`) e em cada transição de estado.

Código: [`src/NostosExchange.Domain/Orders`](../../src/NostosExchange.Domain/Orders)

## Campos

A coluna **Origem** indica quem fornece o dado. Ela define o que entra na criação e o que é preenchido depois, por um comportamento da entidade.

| Campo | Tipo | Origem | Regras |
| --- | --- | --- | --- |
| `ClOrdId` | `Guid` | Cliente | Client Order ID. Identidade de negócio e chave de idempotência ([ADR 0001](../adr/0001-clordid-como-chave-de-idempotencia.md)). Não pode ser `Guid.Empty`. |
| `ClientId` | `Guid` | Corretora | Comitente final, o dono da conta ([Res. CVM 35, art. 23](../regulatory/cvm-35.md)). Não pode ser `Guid.Empty`. |
| `TraderId` | `Guid?` | Corretora | Preenchido **se e somente se** `OrderSource == TradingDesk`. Quando informado, não pode ser `Guid.Empty`. |
| `Symbol` | `Symbol` | Cliente | Ver [Value Objects](#value-objects). |
| `Side` | `Side` | Cliente | Compra ou venda. |
| `OrderType` | `OrderType` | Cliente | Limitada ou a mercado. |
| `Quantity` | `Quantity` | Cliente | Ver [Value Objects](#value-objects). |
| `Price` | `Price?` | Cliente | Obrigatório para `Limit`; deve ser nulo para `Market`. |
| `TimeInForce` | `TimeInForce` | Cliente | Ordens `Market` só aceitam `Ioc` ou `Fok`. |
| `OrderSource` | `OrderSource` | Corretora | Canal por onde a ordem entrou. |
| `ExecutionMethod` | `ExecutionMethod` | Corretora | Quem decidiu enviar a ordem: pessoa (`Manual`) ou sistema (`Automated`). |
| `BrokerReceivedAt` | `DateTimeOffset` | Corretora | Instante de recebimento na corretora, normalizado para UTC. Obrigatório ([Res. CVM 35, art. 12, § 2º](../regulatory/cvm-35.md)). Base da prioridade cronológica da corretora. |
| `ExchangeReceivedAt` | `DateTimeOffset?` | **Bolsa** | Carimbado por `AcceptByExchange`. Nulo enquanto a ordem está `PendingNew`. Deve ser `>= BrokerReceivedAt`. Base da prioridade de tempo no livro ([Res. CVM 135, art. 119, § 1º](../regulatory/cvm-135.md)). |
| `Status` | `OrderStatus` | Entidade | Controlado pelas transições de estado. |
| `FilledQuantity` | `long` | Entidade | Quantidade já executada. Começa em 0. |
| `LeavesQuantity` | `long` | Calculado | `Quantity - FilledQuantity`. |

### Precisão dos timestamps

`DateTimeOffset` tem resolução de **100 ns** (1 tick), o que cobre microssegundos. Precisão de nanossegundos exigiria `long` com nanossegundos desde a época, ao custo da legibilidade, e está fora do escopo por enquanto. Latência interna se mede com `Stopwatch.GetTimestamp()`, não com esses campos. Ver [ADR 0006](../adr/0006-ciclo-de-vida-e-relogio.md).

## Value Objects

Imutáveis, comparados por valor (`record`) e validados no construtor ([ADR 0005](../adr/0005-value-objects-como-record-class.md)).

| Tipo | Valor | Regra | Exceção |
| --- | --- | --- | --- |
| `Price` | `decimal` | `> 0` | `InvalidPriceException` |
| `Quantity` | `long` | `> 0`, unidades inteiras ([ADR 0004](../adr/0004-representacao-numerica-de-quantidade-e-preco.md)) | `InvalidQuantityException` |
| `Symbol` | `string` | Código do mercado à vista da B3. Ver [Symbol](#symbol) ([ADR 0007](../adr/0007-symbol-no-padrao-b3-mercado-a-vista.md)) | `InvalidSymbolException` |

### Symbol

Segue o formato dos códigos de negociação do **mercado à vista** da B3. Antes da validação, o valor passa por `Trim()` e `ToUpperInvariant()`.

```text
  P E T R   4   F
  └──┬──┘   │   └─ sufixo opcional: mercado fracionário
   raiz     └───── código do tipo de papel
```

| Parte | Regra | Propriedade |
| --- | --- | --- |
| Raiz | 4 caracteres; começa com letra e pode conter dígitos (`B3SA`, `M1TA`) | `Root` |
| Tipo de papel | Um dos códigos da tabela abaixo, sem zero à esquerda | `TypeCode` |
| Fracionário | Sufixo `F` opcional | `IsFractional` |

| Código | Tipo de papel |
| --- | --- |
| 1, 2 | Direitos de subscrição (ON, PN) |
| 3 | Ação ordinária (ON) |
| 4 | Ação preferencial (PN) |
| 5 – 8 | Ações preferenciais classes A a D (PNA, PNB, PNC, PND) |
| 9, 10 | Recibos de subscrição (ON, PN) |
| 11 | Units, FIIs e ETFs |
| 31 – 35 | BDRs |
| 39 | BDRs de ETF |

| Entrada | Resultado |
| --- | --- |
| `" petr4f "` | ✅ `PETR4F` · `Root = PETR` · `TypeCode = 4` · `IsFractional = true` |
| `B3SA3`, `M1TA34`, `TAEE11F` | ✅ |
| `PETR04` | ❌ zero à esquerda no código |
| `PETR12` | ❌ código de tipo não suportado |
| `PETR`, `PE TR4`, `1ABC3`, `PETR4FF` | ❌ formato inválido |
| `WINZ25` (futuro), `PETRA290` (opção) | ❌ fora do escopo: só o mercado à vista é suportado |

O código **11 é ambíguo**: pelo ticker não dá para saber se o papel é unit, FII ou ETF. A classificação correta virá do cadastro de instrumentos, não do `Symbol`.

## Enums

Todos começam em `1` e usam `byte` como tipo subjacente ([ADR 0003](../adr/0003-enums-simples.md)).

| Enum | Valores |
| --- | --- |
| `Side` | `Buy`, `Sell` |
| `OrderType` | `Limit`, `Market` |
| `TimeInForce` | `Day`, `Gtc`, `Ioc`, `Fok` |
| `OrderSource` | `HomeBroker`, `TradingDesk`, `Algorithm`, `MobileApp`, `FixApi` |
| `ExecutionMethod` | `Automated`, `Manual` |
| `OrderStatus` | `PendingNew`, `New`, `PartiallyFilled`, `Filled`, `Canceled`, `Rejected` |

### Semântica do `TimeInForce`

| Valor | Comportamento (implementado pelo matching engine) |
| --- | --- |
| `Day` | Fica no livro até o fim do pregão. |
| `Gtc` | Fica no livro até ser cancelada. |
| `Ioc` | Executa o que for possível de imediato e cancela o restante. |
| `Fok` | Executa tudo de imediato, ou nada. Exige checar a liquidez antes de executar. |

### `OrderSource` × `ExecutionMethod`

Os dois campos são **independentes**: um descreve o *canal*, o outro descreve *quem tomou a decisão*. Por exemplo, um trader da mesa pode disparar uma ordem manualmente ou via robô; um cliente pode enviar ordens pela `FixApi` de forma manual ou automatizada. A única combinação contraditória é `Algorithm + Manual`, que é rejeitada.

## Invariantes da entidade

Regras que envolvem mais de um campo. Uma violação lança `InvalidOrderException`.

1. `ClOrdId` e `ClientId` diferentes de `Guid.Empty`.
2. `TraderId` informado ⇔ `OrderSource == TradingDesk`.
3. `OrderSource == Algorithm` ⇒ `ExecutionMethod == Automated`.
4. `OrderType == Limit` ⇒ `Price` informado.
5. `OrderType == Market` ⇒ `Price` nulo **e** `TimeInForce ∈ { Ioc, Fok }`.
6. Todo enum recebido deve ter valor definido (`Enum.IsDefined`), porque um cast de `int` aceita qualquer número.
7. `ExchangeReceivedAt >= BrokerReceivedAt`.

## Ciclo de vida

```text
                Create()            AcceptByExchange()
  (corretora) ───────────► PendingNew ─────────────────► New ──► PartiallyFilled ──► Filled
                               │                          │            │
                               └──► Rejected              └────────────┴──► Canceled
```

| Transição | Método | Situação |
| --- | --- | --- |
| → `PendingNew` | `Order.Create(...)` | ✅ Implementado |
| `PendingNew` → `New` | `AcceptByExchange(TimeProvider)` | ✅ Implementado |
| `PendingNew` → `Rejected` | — | ⏳ Planejado |
| `New`/`PartiallyFilled` → `PartiallyFilled`/`Filled` | — | ⏳ Planejado (execução) |
| `New`/`PartiallyFilled` → `Canceled` | — | ⏳ Planejado (cancelamento, IOC e expiração de `Day`) |

Uma transição inválida (por exemplo, aceitar uma ordem que já está `New`) lança `InvalidOrderStateException`.
