# Referências regulatórias

O NostosExchange modela uma cadeia de negociação real: o **intermediário** (corretora) recebe a ordem do cliente e a envia à **entidade administradora de mercado organizado** (bolsa), onde ela é casada no livro de ofertas. Essa cadeia é regulada por leis federais e pelas resoluções da CVM editadas com base nelas.

```text
 Cliente ──ordem──► Corretora ──oferta──► Bolsa (livro de ofertas)
                    CVM 35                CVM 135
                    BrokerReceivedAt      ExchangeReceivedAt

 Valem para as duas pontas: Lei 6.385 · CVM 62 · Lei 9.613 / CVM 50 · LGPD
```

> ⚠️ **Aviso.** Este é um projeto educacional. As normas servem de **referência de modelagem**: o projeto não é um sistema homologado, não passou por auditoria e não deve ser usado como prova de conformidade regulatória.

## Hierarquia

**Leis** são aprovadas pelo Congresso. **Resoluções da CVM** são normas editadas pela CVM com base nas leis, para detalhá-las. Quando as duas tratam do mesmo assunto, a resolução diz *como* cumprir o que a lei exige.

## Normas mapeadas

### Leis

| Norma | Assunto | Onde aparece no projeto |
| --- | --- | --- |
| [Lei 6.385/1976](lei-6385.md) | Mercado de valores mobiliários; cria a CVM; crimes contra o mercado | Escopo do projeto; prevenção de autonegociação |
| [Lei 9.613/1998](pld.md) | Lavagem de dinheiro | Registro de todas as operações; retenção de 5 anos |
| [Lei 13.709/2018 (LGPD)](lgpd.md) | Proteção de dados pessoais | Ordens referenciam clientes só por identificador |

### Resoluções CVM

| Norma | Assunto | Onde aparece no projeto |
| --- | --- | --- |
| [Res. CVM 35/2021](cvm-35.md) | Intermediação (corretoras) | Criação e registro da `Order` pela corretora |
| [Res. CVM 50/2021](pld.md) | PLD/FTP no mercado de valores mobiliários | Dados para monitoramento de operações atípicas |
| [Res. CVM 62/2022](cvm-62.md) | Manipulação, fraude e práticas não equitativas | Matching determinístico e sem atalhos |
| [Res. CVM 135/2022](cvm-135.md) | Mercados organizados (bolsa) | Aceite pela bolsa, prioridade preço-tempo, livro de ofertas |

### Ainda não mapeadas

| Norma | Por que ainda não | Quando entra |
| --- | --- | --- |
| Res. CVM 30/2021 (adequação ao perfil do cliente, *suitability*) | Depende do cadastro e do perfil do cliente | Com o agregado `Client` |
| Lei 10.214/2001 (sistema de pagamentos, câmaras de compensação) | Compensação e liquidação estão fora do escopo do motor | Se o projeto modelar a pós-negociação |
| Lei 6.404/1976 (sociedades por ações) | Trata das companhias emissoras, não da negociação | ➖ |

## Como ler os documentos

| Situação | Significado |
| --- | --- |
| ✅ Modelado | O domínio já representa a exigência. |
| 🟡 Parcial | Parte da exigência está representada; o que falta está descrito. |
| ⏳ Planejado | Ainda não implementado, mas previsto no roadmap. |
| ➖ Fora do escopo | Não se aplica a um motor de negociação ou foi deixado de fora conscientemente. |

## O que as normas já ensinaram sobre o modelo

- **Por que existem dois timestamps.** A Res. 35 (art. 21, § 2º) exige que a *corretora* priorize ordens concorrentes por ordem cronológica, e para isso usa `BrokerReceivedAt`. A Res. 135 (art. 119, § 1º) exige que a *bolsa* priorize o melhor preço e, no mesmo preço, a ordem de entrada no sistema, e para isso usa `ExchangeReceivedAt`.
- **Prioridade preço-tempo** é a regra central do matching engine (Res. 135, art. 119, § 1º).
- **Prevenção de autonegociação.** Operações simuladas são crime (Lei 6.385, art. 27-C), e operações entre as mesmas partes são uma atipicidade de PLD (Res. 50, art. 20, II, a). O livro não deve casar compra e venda do mesmo comitente.
- **Matching determinístico.** Tratamento equitativo (Res. 62, art. 2º, IV; Res. 135, art. 120, II) significa que a mesma sequência de ofertas sempre produz os mesmos negócios.
- **Histórico imutável de 5 anos**, registrando toda operação independentemente do valor (Res. 35, art. 48; Res. 135, art. 186; Lei 9.613, art. 10, § 2º; Res. 50, arts. 25 e 26). Isso favorece um modelo orientado a eventos. A LGPD (art. 16, I) permite essa retenção por ser obrigação regulatória.
- **Só identificadores no domínio.** O `ClientId` é dado pessoal (LGPD, art. 5º, I), mas não expõe cadastro. Nome e CPF ficam em outro contexto.

## Backlog regulatório consolidado

| Prioridade | Item | Normas |
| --- | --- | --- |
| 1 | Transições de recusa, cancelamento e alteração da `Order` | Res. 35, art. 21, § 1º |
| 2 | Livro com prioridade preço-tempo e número de sequência | Res. 135, art. 119, § 1º; Res. 62, art. 2º, IV |
| 3 | Prevenção de autonegociação | Lei 6.385, art. 27-C; Res. 62, art. 2º; Res. 50, art. 20, II, a |
| 4 | Eventos de domínio imutáveis (ordens e negócios, com as duas partes) | Res. 35, art. 48; Res. 135, arts. 107 e 186; Res. 50, art. 25 |
| 5 | Risco pré-negociação e túnel de preço | Res. 35, art. 16; Res. 135, art. 105, VII; Res. 50, art. 20, II, k |
| 6 | Emissor da ordem e forma de transmissão | Res. 35, art. 12, §§ 1º e 4º |
| 7 | Pessoa vinculada (agregado `Client`) | Res. 35, art. 21, § 2º, II |

## Versões consultadas

| Norma | Versão | Fonte |
| --- | --- | --- |
| Lei 6.385/1976 | Texto compilado | [planalto.gov.br](https://www.planalto.gov.br/ccivil_03/leis/l6385compilada.htm) |
| Lei 9.613/1998 | Texto compilado | [planalto.gov.br](https://www.planalto.gov.br/ccivil_03/leis/l9613compilado.htm) |
| Lei 13.709/2018 | Texto compilado | [planalto.gov.br](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm) |
| Res. CVM 35 | Texto consolidado (inclui Res. 134/2022 e 179/2023) | [cvm.gov.br](https://conteudo.cvm.gov.br/export/sites/cvm/legislacao/resolucoes/anexos/001/resol035consolid.pdf) |
| Res. CVM 50 | Texto consolidado | [cvm.gov.br](https://conteudo.cvm.gov.br/export/sites/cvm/legislacao/resolucoes/anexos/001/resol050consolid.pdf) |
| Res. CVM 62 | Texto original de 19/01/2022 | [cvm.gov.br](https://conteudo.cvm.gov.br/export/sites/cvm/legislacao/resolucoes/anexos/001/resol062.pdf) |
| Res. CVM 135 | Texto original de 10/06/2022 | [cvm.gov.br](https://conteudo.cvm.gov.br/export/sites/cvm/legislacao/resolucoes/anexos/100/resol135.pdf) |

A Res. 135 foi alterada depois (por exemplo, pela Res. CVM 220/2024). Os artigos citados devem ser conferidos contra o texto consolidado mais recente no site da CVM.
