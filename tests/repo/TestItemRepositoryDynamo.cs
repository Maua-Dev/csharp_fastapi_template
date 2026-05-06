namespace csharp_fastapi_template.tests.repo;

using Amazon.DynamoDBv2;
using Amazon.Runtime;
using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.repo;
using Xunit;


// mesma coisa que pytest markskip - se quiser rodar esse teste
// use dotnet test --filter "Category=DynamoIntegration"

[Trait("Category", "DynamoIntegration")]
public class TestItemRepositoryDynamo : IDisposable
{
    private const string BarbieId = "b11af449-22c7-43db-b0e4-dbfbbe7fdbd7";
    private const string HamburguerId = "b21af449-22c7-43db-b0e4-dbfbbe7fdbd7";
    private const string TShirtId = "b31af449-22c7-43db-b0e4-dbfbbe7fdbd7";
    private const string SuperMarioId = "b41af449-22c7-43db-b0e4-dbfbbe7fdbd7";
    private const string CreatedItemId = "2f8ea77a-839c-4f14-8eb3-90f140f9d3e1";

    private readonly IAmazonDynamoDB _client;
    private readonly ItemRepositoryDynamo _repo;

    public TestItemRepositoryDynamo()
    {
        var tableName = Environment.GetEnvironmentVariable("DYNAMO_TABLE_NAME") ?? "cs-fastapi-test-dynamo-table";
        var endpointUrl = Environment.GetEnvironmentVariable("DYNAMO_ENDPOINT_URL") ?? "http://localhost:8000";
        var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = endpointUrl,
            AuthenticationRegion = awsRegion
        };

        _client = new AmazonDynamoDBClient(new BasicAWSCredentials("dummy", "dummy"), config);
        _repo = new ItemRepositoryDynamo(_client, tableName);
    }

    [Fact]
    public void GetAllItems_ShouldReturnSeededItems()
    {
        var items = _repo.GetAllItems();

        Assert.Contains(items, item => item.ItemId == BarbieId);
        Assert.Contains(items, item => item.ItemId == HamburguerId);
        Assert.Contains(items, item => item.ItemId == TShirtId);
        Assert.Contains(items, item => item.ItemId == SuperMarioId);
    }

    [Fact]
    public void GetItem_ShouldReturnExpectedItem()
    {
        var item = _repo.GetItem(HamburguerId);

        Assert.NotNull(item);
        Assert.Equal("Hamburguer", item!.Name);
        Assert.Equal(ItemTypeEnum.Food, item.ItemType);
    }

    [Fact]
    public void CreateItem_ShouldPersistItem()
    {
        _repo.DeleteItem(CreatedItemId);
        var item = new Item(
            itemId: CreatedItemId,
            name: "Notebook",
            price: 1999.99f,
            itemType: ItemTypeEnum.Games,
            adminPermission: false
        );

        _repo.CreateItem(item);
        var stored = _repo.GetItem(item.ItemId);
        _repo.DeleteItem(item.ItemId);

        Assert.NotNull(stored);
        Assert.Equal(item.Name, stored!.Name);
    }

    [Fact]
    public void UpdateItem_ShouldReplaceState()
    {
        var original = new Item(
            itemId: BarbieId,
            name: "Barbie",
            price: 48.9f,
            itemType: ItemTypeEnum.Toy,
            adminPermission: false
        );
        var updated = new Item(
            itemId: BarbieId,
            name: "Barbie Deluxe",
            price: 99.9f,
            itemType: ItemTypeEnum.Toy,
            adminPermission: true
        );

        var result = _repo.UpdateItem(updated);
        Assert.NotNull(result);

        var stored = _repo.GetItem(updated.ItemId);
        _repo.UpdateItem(original);

        Assert.NotNull(stored);
        Assert.Equal("Barbie Deluxe", stored!.Name);
        Assert.True(stored.AdminPermission);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
