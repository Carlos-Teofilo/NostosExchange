# Resolução CVM 62/2022 — Práticas vedadas

Proíbe a criação de condições artificiais de demanda, oferta ou preço, a manipulação de preços, as operações fraudulentas e as práticas não equitativas. Regulamenta os arts. 4º, V e 27-C da [Lei 6.385](lei-6385.md) e substituiu a antiga Instrução CVM 8/1979.

Ver também: [índice regulatório](README.md)

## Mapeamento

| Artigo | Conteúdo (resumo) | Como o projeto trata | Situação |
| --- | --- | --- | --- |
| Art. 2º, I | **Condições artificiais**: negociações pelas quais participantes provocam, de forma dolosa, alterações no **fluxo de ordens** de compra ou venda. | O motor não julga intenção, mas deve tornar o fluxo de ordens **rastreável**: quem enviou, quando, por qual canal (`ClientId`, `BrokerReceivedAt`, `OrderSource`). | 🟡 |
| Art. 2º, II | **Manipulação de preços**: processo ou artifício para elevar, manter ou baixar a cotação, induzindo terceiros a negociar. | Prevenção de autonegociação e histórico completo de ofertas, que viabilizam a detecção posterior. | ⏳ |
| Art. 2º, III | **Operação fraudulenta**: ardil para induzir terceiros em erro e obter vantagem ilícita. | Cabe à vigilância de mercado, a partir dos eventos registrados. | ➖ |
| Art. 2º, IV | **Prática não equitativa**: tratamento que coloque uma parte em posição de desequilíbrio ou desigualdade frente às demais. | O matching deve tratar todas as ofertas pela mesma regra de prioridade preço-tempo, sem atalhos por origem, corretora ou cliente (em linha com a [Res. 135, art. 120, II](cvm-135.md)). | ⏳ |
| Art. 3º | As práticas acima são vedadas a intermediários e demais participantes. | Vale para as duas pontas modeladas: corretora e bolsa. | — |

## Consequências para o modelo

- **Determinismo:** a mesma sequência de ofertas deve produzir sempre os mesmos negócios. É a forma técnica de provar tratamento equitativo.
- **Sem atalhos no livro:** nenhuma origem (`OrderSource`) ou tipo de cliente recebe prioridade fora da regra preço-tempo.
- **Prevenção de autonegociação:** ver a [Lei 6.385, art. 27-C](lei-6385.md).
