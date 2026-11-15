# 0007 — `Symbol` no padrão B3, começando pelo mercado à vista

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

A regra inicial do `Symbol` (1 a 12 caracteres alfanuméricos) aceitava valores que não existem na B3, como `ABC` ou `X1Y2Z3W4`. Cada mercado da B3 tem um formato de código próprio:

| Mercado | Formato | Exemplos |
| --- | --- | --- |
| À vista | raiz de 4 caracteres + tipo de papel + `F` opcional | `PETR4`, `TAEE11F`, `AAPL34` |
| Opções | raiz + letra de mês/tipo + código de strike + `W1`–`W5` opcional | `PETRA290`, `PETRM230W2` |
| Futuros | ativo-objeto + letra do mês de vencimento + ano | `WINZ25`, `WDOF26` |

Os formatos são diferentes porque **os instrumentos são diferentes**: opções têm strike, vencimento e call/put, e futuros têm vencimento, multiplicador e preço em pontos.

## Decisão

- O `Symbol` aceita **apenas códigos do mercado à vista**. Isso é coerente com `Quantity` em unidades inteiras ([ADR 0004](0004-representacao-numerica-de-quantidade-e-preco.md)) e com o livro de lote padrão + fracionário.
- Formato: `^[A-Z][A-Z0-9]{3}` (raiz) + `[1-9][0-9]?` (tipo) + `F?` (fracionário). A raiz pode conter dígitos, como em `B3SA3` e `M1TA34`.
- O tipo de papel precisa estar numa lista fechada: 1–11, 31–35 e 39.
- O `Symbol` expõe as partes do código: `Root`, `TypeCode` e `IsFractional`. O matching engine vai precisar de `IsFractional` para separar o livro fracionário do lote padrão.

## Consequências

- ✅ Códigos que não seguem o padrão da B3 são rejeitados na fronteira do domínio.
- ❌ Códigos válidos que não estejam na lista são rejeitados. Os códigos de direitos e recibos de FIIs, por exemplo, ainda precisam ser conferidos na documentação oficial da B3. Para incluir um código, basta ajustar `ValidTypeCodes`.
- ❌ O formato garante que o código é **bem formado**, não que ele **existe**. `ZZZZ3` passa na validação.
- O sufixo `F` não é cruzado com o tipo de papel: `HGLG11F` é aceito, mesmo que o papel não tenha mercado fracionário.

## Evolução prevista

Introduzir um conceito de **`Instrument`** (cadastro de instrumentos, alimentado pelos arquivos de instrumentos da B3), que será a fonte de verdade para:

- se o código existe e está ativo para negociação;
- o tipo real do papel (resolvendo a ambiguidade do 11);
- o lote padrão, o *tick size* e a existência de mercado fracionário;
- o suporte a opções e futuros, como tipos de instrumento próprios.

Nesse modelo, o `Symbol` continua validando só o formato, e a `Order` passa a referenciar um instrumento válido.

## Alternativas consideradas

- **Um único `Symbol` para todos os mercados:** descartado neste momento. Viraria uma união de formatos e espalharia regras específicas de cada mercado pela `Order`.
- **Validar só o formato, sem lista de tipos:** descartado. Aceitaria `PETR99`.
