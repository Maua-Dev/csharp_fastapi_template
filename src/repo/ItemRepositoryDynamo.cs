namespace csharp_fastapi_template.repo;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.repo.interfaces;

public class ItemRepositoryDynamo : IItemRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public ItemRepositoryDynamo(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public IList<Item> GetAllItems()
    {
        var response = _dynamoDb.ScanAsync(new ScanRequest
        {
            TableName = _tableName
        }).GetAwaiter().GetResult();

        return response.Items.Select(FromDynamoItem).ToList();
    }

    public Item? GetItem(string itemId)
    {
        var response = _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "item_id", new AttributeValue { S = itemId } }
            }
        }).GetAwaiter().GetResult();

        if (response.Item is null || response.Item.Count == 0)
        {
            return null;
        }

        return FromDynamoItem(response.Item);
    }

    public Item CreateItem(Item item)
    {
        _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToDynamoItem(item),
            ConditionExpression = "attribute_not_exists(item_id)"
        }).GetAwaiter().GetResult();

        return item;
    }

    public Item? DeleteItem(string itemId)
    {
        var response = _dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "item_id", new AttributeValue { S = itemId } }
            },
            ReturnValues = ReturnValue.ALL_OLD
        }).GetAwaiter().GetResult();

        if (response.Attributes is null || response.Attributes.Count == 0)
        {
            return null;
        }

        return FromDynamoItem(response.Attributes);
    }

    public Item? UpdateItem(Item item)
    {
        var response = _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToDynamoItem(item),
            ConditionExpression = "attribute_exists(item_id)",
            ReturnValues = ReturnValue.ALL_OLD
        }).GetAwaiter().GetResult();

        if (response.Attributes is null || response.Attributes.Count == 0)
        {
            return null;
        }

        return item;
    }

    private static Dictionary<string, AttributeValue> ToDynamoItem(Item item)
    {
        return new Dictionary<string, AttributeValue>
        {
            { "item_id", new AttributeValue { S = item.ItemId } },
            { "name", new AttributeValue { S = item.Name } },
            { "price", new AttributeValue { N = item.Price.ToString(System.Globalization.CultureInfo.InvariantCulture) } },
            { "item_type", new AttributeValue { S = item.ItemType.ToString() } },
            { "admin_permission", new AttributeValue { BOOL = item.AdminPermission } }
        };
    }

    private static Item FromDynamoItem(Dictionary<string, AttributeValue> dynamoItem)
    {
        var itemId = dynamoItem["item_id"].S;
        var name = dynamoItem["name"].S;
        var price = float.Parse(dynamoItem["price"].N, System.Globalization.CultureInfo.InvariantCulture);
        var itemType = Enum.Parse<ItemTypeEnum>(dynamoItem["item_type"].S, ignoreCase: true);
        var adminPermission = dynamoItem["admin_permission"].BOOL;

        return new Item(itemId, name, price, itemType, adminPermission);
    }
}
