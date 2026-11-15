# Lei 13.709/2018 — Lei Geral de Proteção de Dados (LGPD)

Regula o tratamento de dados pessoais. Ela interessa ao projeto por dois motivos: as ordens estão ligadas a pessoas (o comitente final), e as normas do mercado exigem guardar esses registros por anos.

Ver também: [índice regulatório](README.md)

## Mapeamento

| Artigo | Conteúdo (resumo) | Como o projeto trata | Situação |
| --- | --- | --- | --- |
| Art. 5º, I | **Dado pessoal**: informação relacionada a pessoa natural identificada ou **identificável**. | O `ClientId` não é nome nem CPF, mas identifica uma pessoa quando cruzado com o cadastro. **Continua sendo dado pessoal.** | — |
| Art. 6º, III | Princípio da **necessidade**: tratar o mínimo necessário para a finalidade. | A `Order` carrega só identificadores (`ClientId`, `TraderId`), nunca nome, CPF ou contato. | ✅ |
| Art. 6º, VII e art. 46 | **Segurança**: medidas técnicas e administrativas contra acessos não autorizados. | Logs, mensagens de exceção e eventos não devem conter dados cadastrais. | 🟡 |
| Art. 7º, II | Base legal: cumprimento de **obrigação legal ou regulatória**. | É a base para registrar as ordens: as Res. CVM 35 e 50 e a Lei 9.613 exigem o registro. | ✅ |
| Art. 16, I | Os dados podem ser **conservados** após o fim do tratamento para cumprir obrigação legal ou regulatória. | Concilia a LGPD com a retenção de 5 anos exigida pelas normas do mercado. | ✅ |
| Art. 48 | Comunicar à ANPD e ao titular a ocorrência de **incidente de segurança**. | Processo operacional, fora do domínio. | ➖ |

## Consequências para o modelo

- **Separação de dados:** o domínio de negociação referencia clientes **somente por identificador**. Dados cadastrais ficam em outro contexto (cadastro), com controle de acesso próprio.
- **Mensagens de erro sem dados pessoais:** as exceções de domínio já seguem isso. Elas citam campos (`ClientId must be informed`), não valores pessoais.
- **Retenção versus eliminação:** a LGPD pede eliminar dados ao fim do tratamento (art. 16), mas o próprio art. 16, I permite conservar o que é exigido por norma. O histórico de ordens fica retido pelo prazo regulatório (5 anos), e não "para sempre".
