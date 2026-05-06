# Testes de repositório (DynamoDB)

Este diretório contém testes de integração do `ItemRepositoryDynamo`.

## Pré-requisitos

- Docker e Docker Compose instalados
- `.NET SDK` instalado
- AWS CLI instalada (necessária para o script de seed em `iac/local/dynamo/load_items.sh`)

## 1) Subir DynamoDB local

Na raiz do projeto:

```bash
docker compose -f iac/local/dynamo/docker-compose.yml up -d
```

## 2) Configurar variáveis de ambiente

Garanta que o `.env` tenha pelo menos:

```env
DYNAMO_TABLE_NAME=cs-fastapi-test-dynamo-table
DYNAMO_ENDPOINT_URL=http://localhost:8000
AWS_REGION=us-east-1
```

## 3) Criar tabela e carregar seed

Ainda na raiz do projeto:

```bash
bash iac/local/dynamo/load_items.sh
```

> O script cria a tabela se ela ainda nao existir e carrega os mesmos itens do repositório mock.

## 4) Rodar os testes do repo Dynamo

```bash
dotnet test tests/csharp_fastapi_template.Tests.csproj --filter TestItemRepositoryDynamo
```

## 5) Derrubar Dynamo local (opcional)

```bash
docker compose -f iac/local/dynamo/docker-compose.yml down
```
