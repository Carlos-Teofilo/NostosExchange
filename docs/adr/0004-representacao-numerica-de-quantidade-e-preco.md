# 0004 — `Quantity` como `long`, `Price` como `decimal`

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

Com `decimal`, `Quantity(0.5m)` era aceito, mas ações da B3 são negociadas em unidades inteiras (lote padrão de 100, fracionário a partir de 1). Matching engines também costumam preferir aritmética inteira, por desempenho e para não ter arredondamento.

## Decisão

- **`Quantity.Value` é `long`.** A quantidade fracionária deixa de ser representável, e essa invariante passa a ser garantida pelo próprio tipo.
- **`Price.Value` continua `decimal`.** O preço tem casas decimais e `decimal` evita os erros de ponto flutuante de `double`.

## Consequências

- Ativos fracionáveis abaixo de 1, como cripto, não são suportados. Suportá-los exigiria representar a quantidade em unidades mínimas (por exemplo, satoshis).
- **Evolução prevista:** representar o preço em *ticks* (`long`) e validar o *tick size* por instrumento. Fica para quando existir o conceito de `Instrument`.
