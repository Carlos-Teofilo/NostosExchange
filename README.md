# NostosExchange

Motor de emparelhamento de ordens (*Matching Engine* / *Limit Order Book*) em .NET, com foco em integridade de domínio, concorrência e arquitetura orientada a eventos.

> Projeto de estudo e portfólio. O domínio se inspira no mercado de ações brasileiro (B3) e na terminologia do protocolo FIX.

**Escopo atual:** mercado à vista da B3 (ações, units, FIIs, ETFs e BDRs), com lote padrão e fracionário. Opções e futuros estão planejados ([ADR 0007](docs/adr/0007-symbol-no-padrao-b3-mercado-a-vista.md)).

## Status

| Etapa | Situação |
| --- | --- |
| Domínio: Value Objects (`Price`, `Quantity`, `Symbol`) | ✅ Concluído |
| Domínio: Enums da ordem | ✅ Concluído |
| Domínio: Entidade `Order` (criação e aceite pela bolsa) | ✅ Concluído |
| Testes unitários do domínio | ⏳ Próximo passo |
| Execução, cancelamento e expiração da ordem | ⏳ Planejado |
| Livro de ofertas e matching engine | ⏳ Planejado |
| Cadastro de instrumentos (`Instrument`), opções e futuros | ⏳ Planejado |

## Princípios

- **Domínio rico:** as regras de negócio vivem nas entidades e nos Value Objects, que nunca existem em estado inválido.
- **Guiado pela regulação:** as principais leis do mercado (6.385, 9.613 e LGPD) e as Resoluções CVM 35, 50, 62 e 135 são fontes de requisitos, mapeadas artigo por artigo em [docs/regulatory](docs/regulatory/README.md) ([ADR 0008](docs/adr/0008-normas-da-cvm-como-fonte-de-requisitos.md)).
- **Decisões registradas:** toda escolha relevante de arquitetura tem um [ADR](docs/adr/README.md).

## Stack

- .NET 10 / C# 14
- Solution no formato `.slnx`
- Nullable reference types habilitado

## Estrutura

```text
NostosExchange/
├── NostosExchange.slnx
├── README.md
├── docs/
│   ├── domain/            # Modelagem do domínio (campos, regras e invariantes)
│   ├── regulatory/        # Leis e normas da CVM usadas como referência de modelagem
│   └── adr/               # Architecture Decision Records
└── src/
    └── NostosExchange.Domain/
        ├── Shared/
        │   └── DomainExceptions/      # DomainException (base)
        └── Orders/
            ├── DomainExceptions/      # Exceções específicas do agregado
            ├── Entities/              # Order (raiz do agregado)
            ├── Enums/                 # Side, OrderType, TimeInForce, OrderSource, ExecutionMethod, OrderStatus
            └── ValueObjects/          # Price, Quantity, Symbol
```

Cada agregado tem a própria pasta, com nome no plural (`Orders`). Assim o namespace `NostosExchange.Domain.Orders` não entra em conflito com a classe `Order`.

## Como compilar

```bash
dotnet build NostosExchange.slnx
```

## Documentação

- [Modelo de domínio: Order](docs/domain/order.md): campos, origem de cada dado, Value Objects, enums, invariantes e ciclo de vida.
- [Decisões de arquitetura (ADRs)](docs/adr/README.md): o que foi decidido, por quê e quais alternativas foram descartadas.
- [Referências regulatórias](docs/regulatory/README.md): como as leis do mercado e as resoluções da CVM se refletem no modelo, artigo por artigo, com o backlog regulatório.

> Projeto educacional: as normas são usadas como referência de modelagem, não como declaração de conformidade.
