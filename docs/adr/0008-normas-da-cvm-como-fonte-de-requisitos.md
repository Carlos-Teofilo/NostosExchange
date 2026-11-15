# 0008 — Regulação do mercado como fonte de requisitos

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

O projeto modela a cadeia corretora → bolsa do mercado brasileiro. Essa cadeia é regulada pela CVM, principalmente pelas Resoluções [35/2021](../regulatory/cvm-35.md) (intermediação) e [135/2022](../regulatory/cvm-135.md) (mercados organizados). Até aqui, as normas tinham sido consultadas depois das decisões. O mapeamento mostrou que várias decisões tomadas por intuição, como os dois timestamps, têm base regulatória, e que existem exigências ainda não modeladas.

## Decisão

As leis do mercado e as normas da CVM passam a ser uma **fonte primária de requisitos** do domínio, seguidas o máximo possível. O conjunto mapeado inclui as Leis 6.385/1976, 9.613/1998 e 13.709/2018 (LGPD) e as Resoluções CVM 35, 50, 62 e 135 ([índice](../regulatory/README.md)):

1. **Antes** de projetar uma funcionalidade, identificar os artigos aplicáveis no texto oficial (Planalto para leis, CVM para resoluções), com citação de artigo e parágrafo.
2. Uma regra de negócio exigida por norma entra no **domínio** (invariante, Value Object ou transição de estado), não em validação de interface.
3. Quando o modelo **simplificar** ou for **mais restritivo** que a norma, isso deve ser registrado explicitamente no mapeamento de [docs/regulatory/](../regulatory/README.md).
4. Toda mudança que afete um artigo mapeado atualiza a situação (✅ 🟡 ⏳ ➖) no mesmo commit.
5. Em conflito entre desempenho e exigência normativa, a exigência prevalece no domínio. A otimização é buscada sem violar a regra (por exemplo, número de sequência em vez de abrir mão da ordem cronológica estrita).
6. Outras normas entram no mapeamento quando a funcionalidade correspondente for construída (ver "Ainda não mapeadas" no [índice](../regulatory/README.md)).

## Consequências

- ✅ Os requisitos deixam de ser inventados: cada regra relevante tem uma origem rastreável.
- ✅ O mapeamento funciona como roadmap: os itens ⏳ são o backlog regulatório.
- ❌ Algumas funcionalidades ficam maiores do que seriam num motor "de brinquedo" (por exemplo, distinguir o emissor da ordem ou a prioridade de pessoas não vinculadas).
- O projeto continua sendo **educacional**: seguir as normas como referência de modelagem não é declaração de conformidade.
