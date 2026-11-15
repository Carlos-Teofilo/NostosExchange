# 0005 — Value Objects como `record` (classe)

- **Status:** Aceita
- **Data:** 2026-11-15

## Contexto

Value Objects podem ser `record` (tipo por referência) ou `readonly record struct` (tipo por valor).

## Decisão

Usar **`record` (classe)** com propriedades somente leitura (`{ get; }`) e validação no construtor.

## Consequências

- ✅ A invariante não pode ser contornada: não existe `default(Price)` com valor `0`, e `with { Value = ... }` não compila, porque não há `init`.
- ❌ Cada instância é alocada no heap. Com milhões de ordens no livro, isso aumenta a pressão sobre o GC.

## Alternativas consideradas

- **`readonly record struct`:** elimina a alocação, mas `default(Price)` produz `Value = 0` sem passar pelo construtor e fura a invariante. Pode ser reavaliado quando houver benchmarks (BenchmarkDotNet) mostrando que a alocação importa no caminho crítico. Nesse caso, a estrutura interna do livro pode usar primitivos e manter os VOs só nas fronteiras.
