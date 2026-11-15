# Prevenção à lavagem de dinheiro — Lei 9.613/1998 e Resolução CVM 50/2021

A Lei 9.613 define o crime de lavagem de dinheiro e as obrigações de quem atua no sistema financeiro. A Res. CVM 50 detalha essas obrigações (PLD/FTP) para o mercado de valores mobiliários.

Ver também: [índice regulatório](README.md)

## Quem está sujeito

- **Lei 9.613, art. 9º:** inclui quem atua na negociação e intermediação de valores mobiliários e, no parágrafo único, as **bolsas de valores**.
- **Res. CVM 50, art. 3º, I e II:** intermediários e entidades administradoras de mercados organizados.

Ou seja, as duas pontas modeladas no projeto (corretora e bolsa) têm obrigações de PLD.

## Mapeamento

| Norma e artigo | Exigência (resumo) | Como o projeto trata | Situação |
| --- | --- | --- | --- |
| Lei 9.613, art. 10, I | **Identificar os clientes** e manter o cadastro atualizado. | O cadastro fica fora do domínio de ordens; a `Order` exige `ClientId` válido. | 🟡 |
| Lei 9.613, art. 10, § 2º | Conservar cadastros e registros por **no mínimo 5 anos**. | Mesma exigência da [Res. 35, art. 48](cvm-35.md) e da [Res. 135, art. 186](cvm-135.md): histórico imutável. | ⏳ |
| Lei 9.613, art. 11, I | Dar **atenção especial** a operações que possam indicar indícios de crime. | Cabe ao monitoramento, alimentado pelos eventos de domínio. | ⏳ |
| Lei 9.613, art. 11, II | Comunicar ao **COAF**, sem dar ciência ao envolvido. | Processo de compliance, fora do motor. | ➖ |
| Res. 50, art. 20, II, a | Atipicidade: operações **entre as mesmas partes** ou em benefício delas, com ganhos ou perdas seguidos. | Exige que cada negócio registre comprador e vendedor (`ClientId` das duas ofertas). | ⏳ |
| Res. 50, art. 20, II, b | Atipicidade: **oscilação significativa** de volume ou frequência de negócios. | Exige histórico por cliente, com horário. | ⏳ |
| Res. 50, art. 20, II, k | Atipicidade: operações **fora do preço de mercado**. | Reforça a necessidade de túnel de preço / banda de oscilação ([Res. 135](cvm-135.md)). | ⏳ |
| Res. 50, art. 25 | Manter registro de **toda** operação com valores mobiliários, **independentemente do valor**. | Toda ordem e todo negócio viram eventos persistidos; não há limite mínimo. | ⏳ |
| Res. 50, art. 26 | Manter a documentação à disposição da CVM por **no mínimo 5 anos**. | Retenção do event store. | ⏳ |

## Consequências para o modelo

- **O motor não decide se algo é lavagem**, mas precisa produzir os dados que tornam o monitoramento possível. Cada negócio deve registrar as duas partes, o preço, a quantidade e o horário.
- **Nenhum filtro por valor:** o art. 25 da Res. 50 proíbe registrar "só as operações grandes".
