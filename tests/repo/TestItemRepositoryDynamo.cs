namespace csharp_fastapi_template.tests.repo;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.repo;
using Xunit;


// classe usada para resetar o estado da tabela em cada teste
public class DynamoFixture : IDisposable
{
    // Nome da tabela de teste. Pode vir do ambiente para facilitar customização local/CI.
    public readonly string TableName = Environment.GetEnvironmentVariable("DYNAMO_TABLE_NAME") ?? "cs-fastapi-test-dynamo-table";
    public readonly IAmazonDynamoDB Client;

    public DynamoFixture()
    {
        // Configura cliente Dynamo local (localhost:8000) por padrão.
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = Environment.GetEnvironmentVariable("DYNAMO_ENDPOINT_URL") ?? "http://localhost:8000",
            AuthenticationRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1"
        };

        // Credenciais dummy: Dynamo local não valida IAM de verdade.
        var credentials = new Amazon.Runtime.BasicAWSCredentials("dummy", "dummy");
        Client = new AmazonDynamoDBClient(credentials, config);

        // Cada classe de teste começa com tabela limpa + seed conhecido!
        // Isso evita problemas em por exemplo teste delete -> teste get_all
        RecreateTable();
        SeedMockItems();
    }

    private void RecreateTable()
    {
        try
        {
            // Remove tabela anterior para garantir estado determinístico.
            Client.DeleteTableAsync(new DeleteTableRequest { TableName = TableName }).GetAwaiter().GetResult();
            WaitForTableDeletion();
        }
        catch (ResourceNotFoundException)
        {
            // tabela ainda não existe, não é problema
        }

        Client.CreateTableAsync(new CreateTableRequest
        {
            TableName = TableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("item_id", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("item_id", KeyType.HASH)
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        }).GetAwaiter().GetResult();

        // Aguarda tabela ficar ACTIVE antes de gravar/consultar dados.
        WaitForTableActive();
    }

    private void SeedMockItems()
    {
        // Carrega os mesmos itens iniciais do ItemRepositoryMock.
        Client.BatchWriteItemAsync(new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                {
                    TableName,
                    new List<WriteRequest>
                    {
                        CreateWriteRequest("b11af449-22c7-43db-b0e4-dbfbbe7fdbd7", "Barbie", "48.9", "Toy", false),
                        CreateWriteRequest("b21af449-22c7-43db-b0e4-dbfbbe7fdbd7", "Hamburguer", "38", "Food", false),
                        CreateWriteRequest("b31af449-22c7-43db-b0e4-dbfbbe7fdbd7", "T-shirt", "22.95", "Clothes", false),
                        CreateWriteRequest("b41af449-22c7-43db-b0e4-dbfbbe7fdbd7", "Super Mario Bros", "55", "Games", true)
                    }
                }
            }
        }).GetAwaiter().GetResult();
    }

    private static WriteRequest CreateWriteRequest(string id, string name, string price, string itemType, bool admin)
    {
        // Helper para reduzir repetição no seed.
        return new WriteRequest
        {
            PutRequest = new PutRequest
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    { "item_id", new AttributeValue { S = id } },
                    { "name", new AttributeValue { S = name } },
                    { "price", new AttributeValue { N = price } },
                    { "item_type", new AttributeValue { S = itemType } },
                    { "admin_permission", new AttributeValue { BOOL = admin } }
                }
            }
        };
    }

    private void WaitForTableActive()
    {
        // poll para evitar race condition após CreateTable.
        for (var i = 0; i < 20; i++)
        {
            var status = Client.DescribeTableAsync(new DescribeTableRequest
            {
                TableName = TableName
            }).GetAwaiter().GetResult().Table.TableStatus;

            if (status == TableStatus.ACTIVE)
            {
                return;
            }

            Thread.Sleep(200);
        }
    }

    private void WaitForTableDeletion()
    {
        // Poll para evitar recriar tabela antes da deleção concluir.
        for (var i = 0; i < 20; i++)
        {
            try
            {
                Client.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = TableName
                }).GetAwaiter().GetResult();
                Thread.Sleep(200);
            }
            catch (ResourceNotFoundException)
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        // Fecha conexões/recursos do client ao final da execução da fixture.
        Client.Dispose();
    }
}

public class TestItemRepositoryDynamo : IClassFixture<DynamoFixture>
{
    private readonly ItemRepositoryDynamo _repo;

    public TestItemRepositoryDynamo(DynamoFixture fixture)
    {
        // Usa o client e a tabela preparados pela fixture compartilhada da classe.
        _repo = new ItemRepositoryDynamo(fixture.Client, fixture.TableName);
    }

    [Fact]
    public void GetAllItems_ShouldReturnSeededItems()
    {
        var items = _repo.GetAllItems();
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void GetItem_ShouldReturnExpectedItem()
    {
        var item = _repo.GetItem("b21af449-22c7-43db-b0e4-dbfbbe7fdbd7");
        Assert.NotNull(item);
        Assert.Equal("Hamburguer", item!.Name);
        Assert.Equal(ItemTypeEnum.Food, item.ItemType);
    }

    [Fact]
    public void CreateItem_ShouldPersistItem()
    {
        var item = new Item(
            itemId: "2f8ea77a-839c-4f14-8eb3-90f140f9d3e1",
            name: "Notebook",
            price: 1999.99f,
            itemType: ItemTypeEnum.Games,
            adminPermission: false
        );

        _repo.CreateItem(item);
        var stored = _repo.GetItem(item.ItemId);

        Assert.NotNull(stored);
        Assert.Equal(item.Name, stored!.Name);
    }

    [Fact]
    public void UpdateItem_ShouldReplaceState()
    {
        var updated = new Item(
            itemId: "b11af449-22c7-43db-b0e4-dbfbbe7fdbd7",
            name: "Barbie Deluxe",
            price: 99.9f,
            itemType: ItemTypeEnum.Toy,
            adminPermission: true
        );

        var result = _repo.UpdateItem(updated);
        Assert.NotNull(result);

        var stored = _repo.GetItem(updated.ItemId);
        Assert.NotNull(stored);
        Assert.Equal("Barbie Deluxe", stored!.Name);
        Assert.True(stored.AdminPermission);
    }
}
