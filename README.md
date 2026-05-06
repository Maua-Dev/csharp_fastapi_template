# csharp_fastapi_template
Template de API em C# / .NET inspirado no simple_fastapi_template, estendendo o escopo de linguagens além de Python/FastAPI para uma visão mais amplia de stacks. Também vai servir para estudar como seria a implementação da nossa arquitetura em um ambiente / linguagem diferente

## Testes

Os testes de integração com Dynamo estão marcados com:

- `Category=DynamoIntegration`

Comandos úteis:

- Rodar todos os testes:
  - `dotnet test`
- Rodar sem testes Dynamo:
  - `dotnet test --filter "Category!=DynamoIntegration"`
- Rodar somente testes Dynamo:
  - `dotnet test --filter "Category=DynamoIntegration"`
