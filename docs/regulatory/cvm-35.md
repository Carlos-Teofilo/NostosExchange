# Resolução CVM 35/2021 — Intermediação

Dispõe sobre a intermediação de operações com valores mobiliários em mercados regulamentados. No projeto, ela orienta **o que a corretora precisa registrar sobre cada ordem** antes de enviá-la à bolsa.

Ver também: [índice regulatório](README.md) · [modelo de domínio da `Order`](../domain/order.md)

## Mapeamento

| Artigo | Exigência (resumo) | Como o projeto trata | Situação |
| --- | --- | --- | --- |
| Art. 2º, X | Define **ordem**: ato prévio pelo qual o cliente determina que o intermediário negocie em seu nome, nas condições que especificar. | Entidade `Order`. | ✅ |
| Art. 12, caput | Só se executa negócio para o cliente **mediante ordem prévia**. | A ordem nasce como `PendingNew` na corretora (`Order.Create`) e só depois é aceita pela bolsa (`AcceptByExchange`). | ✅ |
| Art. 12, § 1º | A ordem pode ser transmitida por voz, por escrito ou por acesso direto ao mercado (DMA). | `OrderSource` registra o canal; `FixApi` corresponde ao DMA. Voz e presencial ainda não têm valor próprio. | 🟡 |
| Art. 12, § 2º | Toda ordem deve ser registrada com **horário de recebimento**, **cliente emissor** e **condições de execução**. | `BrokerReceivedAt`, `ClientId` e as condições (`Side`, `OrderType`, `Quantity`, `Price`, `TimeInForce`), todos obrigatórios na criação. | ✅ |
| Art. 12, § 4º | Identificar e registrar o **emissor** da ordem (cliente, procurador, representante ou pessoa autorizada). | Só o `TraderId` (mesa) é registrado. Falta distinguir procurador e representante. | 🟡 |
| Art. 13 | Arquivar os registros das ordens e das condições de execução, garantindo confidencialidade, autenticidade, integridade e disponibilidade. | Persistência ainda não implementada. | ⏳ |
| Art. 15 | Ordem presencial: data e horário, assinatura, quem recebeu, natureza e tipo, prazo de validade, valor mobiliário, quantidade e preço. | Tipo (`OrderType`), validade (`TimeInForce`), ativo (`Symbol`), quantidade e preço são modelados. Assinatura e recebedor ficam fora do motor. | 🟡 |
| Art. 16, § 1º, I | Em DMA, identificar a origem das ordens e rastrear o emissor. | `OrderSource` + `ExecutionMethod` + `ClientId`. | ✅ |
| Art. 16, § 1º, II e § 2º | Controle de **risco pré-operacional**: limites operacionais por cliente e parâmetros para detectar ordens enviadas por erro. | Ainda não implementado (por exemplo, limite de quantidade/financeiro e banda de preço por cliente). | ⏳ |
| Art. 20 | **Melhor execução**: buscar o melhor resultado para o cliente (preço, custo, rapidez, probabilidade de execução etc.). | O projeto tem um único ambiente de negociação, então não há roteamento entre mercados. | ➖ |
| Art. 21, § 1º, I e IV | As regras da corretora devem definir os **tipos de ordens aceitas** e o **prazo de validade** delas. | `OrderType` (`Limit`, `Market`) e `TimeInForce` (`Day`, `Gtc`, `Ioc`, `Fok`), com as combinações válidas no [modelo](../domain/order.md#invariantes-da-entidade). | ✅ |
| Art. 21, § 1º, V e VII | Procedimentos de **recusa**, **cancelamento** e **alteração** de ordens. | `OrderStatus.Rejected` e `Canceled` existem; as transições ainda não. | ⏳ |
| Art. 21, II | Permitir, a qualquer tempo, **vincular** a ordem à oferta e ao negócio realizado. | `ClOrdId` é a chave de ligação; a entidade de negócio (execução) ainda não existe. | ⏳ |
| Art. 21, § 2º, I | Ordens concorrentes: prioridade pelo **critério cronológico**. | `BrokerReceivedAt` fornece a base; a fila da corretora ainda não existe. | ⏳ |
| Art. 21, § 2º, II | Em ordens simultâneas, as de clientes **não vinculados** têm prioridade sobre as de **pessoas vinculadas** ao intermediário. | Requer indicar se o cliente é pessoa vinculada. | ⏳ |
| Art. 23, caput e § 2º | Identificar o **comitente final** em toda ordem. Negócios da mesa podem identificá-lo em até 30 minutos. | `ClientId` é obrigatório já na criação, inclusive para a mesa. O modelo é **mais restritivo** que a norma e não suporta a identificação posterior. | ✅ |
| Art. 37, II | Vedado aceitar ordens de clientes **não cadastrados** ou com **cadastro desatualizado**. | O cadastro de clientes fica fora do domínio de ordens; a verificação caberá à camada de aplicação. | ⏳ |
| Art. 48 | Manter documentos, registros de ordens e trilhas de auditoria por **no mínimo 5 anos**. | Indica um histórico imutável (event store). | ⏳ |

## Lacunas priorizadas

1. **Risco pré-operacional** (art. 16): limites por cliente antes de a ordem sair da corretora.
2. **Cancelamento, alteração e recusa** (art. 21, § 1º): transições de estado da `Order`.
3. **Vínculo ordem → negócio** (art. 21, II): entidade de execução referenciando o `ClOrdId`.
4. **Pessoa vinculada** (art. 21, § 2º, II): dado necessário para a regra de prioridade da corretora.
