# 0011 — Estratégia de testes

- **Status:** Aceita
- **Data:** 2026-11-16

## Contexto

A `Order` já tem invariantes, transições de estado e regras de alteração. A próxima etapa ([ADR 0010](0010-arquitetura-orientada-a-eventos.md)) é refatorá-la para Event Sourcing. Antes de mexer na estrutura interna, é preciso uma rede de segurança: testes que provem que as regras continuam valendo depois da refatoração.

## Decisão

| Tema | Escolha |
| --- | --- |
| Framework | **xUnit v3** |
| Asserções | `Assert` nativo do xUnit |
| Tempo | `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) |
| Dados de teste | Padrão **Test Data Builder** (`OrderBuilder`) |
| Nomes | `Metodo_Cenario_ResultadoEsperado`, em inglês |
| Estrutura | `tests/NostosExchange.Domain.Tests/`, espelhando as pastas de `src/` |
| Executor | Microsoft.Testing.Platform, habilitado no `global.json` |

### Justificativas

- **xUnit v3:** é a versão atual e mantida do xUnit, o framework de testes mais usado em .NET moderno. O template `dotnet new xunit` ainda gera um projeto com xUnit v2. Por isso o projeto foi criado pelo template e os pacotes foram trocados para `xunit.v3`, que exige `OutputType=Exe`.
- **`Assert` nativo:** o FluentAssertions, muito popular, passou a ter licença comercial a partir da versão 8. O `Assert` nativo é suficiente, não tem custo de licença e não adiciona dependência.
- **`FakeTimeProvider`:** a `Order` recebe `TimeProvider` em todas as transições ([ADR 0006](0006-ciclo-de-vida-e-relogio.md)). Nos testes, um relógio controlado torna cada teste **determinístico**: o tempo só anda quando o teste chama `Advance`.
- **Test Data Builder:** criar uma `Order` exige 12 parâmetros. O builder fornece uma ordem válida por padrão, e cada teste altera só o que é relevante, o que deixa a intenção do teste visível.
- **Microsoft.Testing.Platform:** no SDK do .NET 10, o `dotnet test` só roda projetos xUnit v3 com o novo executor se ele for habilitado no `global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`). Os pacotes `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio` continuam no projeto para a integração com o Test Explorer das IDEs.

## Consequências

- ✅ As regras do domínio ficam documentadas também como testes executáveis.
- ✅ A refatoração para Event Sourcing poderá ser feita com segurança: os testes não dependem da estrutura interna da `Order`, só do comportamento público.
- ❌ `[InlineData]` não aceita `decimal`, porque atributos só aceitam constantes. Testes com `Price` precisam de `[MemberData]`/`TheoryData<decimal>` ou de conversão.
- O pacote `coverlet.collector` só funciona com o executor antigo (VSTest). A cobertura de código pelo `dotnet test` exigirá um ADR próprio quando for necessária.

## Alternativas consideradas

- **NUnit / MSTest:** igualmente maduros. O xUnit foi escolhido por ser o mais usado em projetos .NET modernos e de código aberto.
- **FluentAssertions:** descartado pela mudança de licença.
- **Shouldly:** alternativa de asserções fluentes com licença aberta. Pode ser reavaliada se as asserções nativas ficarem pouco legíveis.
