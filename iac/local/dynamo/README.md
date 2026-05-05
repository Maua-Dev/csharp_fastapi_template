# DynamoDB local

## Subir o DynamoDB local

No diretório do projeto:

```bash
docker compose -f iac/local/dynamo/docker-compose.yml up -d
```

## Criar tabela e carregar itens iniciais

O script abaixo cria a tabela (se não existir) e carrega os mesmos itens do `ItemRepositoryMock`.

```bash
bash iac/local/dynamo/load_items.sh
```

## Derrubar o ambiente local

```bash
docker compose -f iac/local/dynamo/docker-compose.yml down
```
