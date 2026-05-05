namespace csharp_fastapi_template.repo;

using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.errors;
using csharp_fastapi_template.repo.interfaces;

public class ItemRepositoryMock : IItemRepository
{
    private List<Item> _items;

    public ItemRepositoryMock()
    {
        _items = new List<Item>
        {
            new Item(
                itemId: "b11af449-22c7-43db-b0e4-dbfbbe7fdbd7",
                name: "Barbie",
                price: 48.90f,
                itemType: ItemTypeEnum.Toy,
                adminPermission: false
            ),
            new Item(
                itemId: "b21af449-22c7-43db-b0e4-dbfbbe7fdbd7",
                name: "Hamburguer",
                price: 38.00f,
                itemType: ItemTypeEnum.Food,
                adminPermission: false
            ),
            new Item(
                itemId: "b31af449-22c7-43db-b0e4-dbfbbe7fdbd7",
                name: "T-shirt",
                price: 22.95f,
                itemType: ItemTypeEnum.Clothes,
                adminPermission: false
            ),
            new Item(
                itemId: "b41af449-22c7-43db-b0e4-dbfbbe7fdbd7",
                name: "Super Mario Bros",
                price: 55.00f,
                itemType: ItemTypeEnum.Games,
                adminPermission: true
            )
        };
    }

    public IList<Item> GetAllItems() => _items;

    public Item? GetItem(string itemId) =>
        _items.FirstOrDefault(item => item.ItemId == itemId);

     public Item CreateItem(Item item)
    {
        _items.Add(item);
        return item;
    }

    public Item? DeleteItem(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.ItemId == itemId);
        if (item is null) return null;

        _items.Remove(item);
        return item;
    }

    public Item? UpdateItem(Item item)
    {
        var index = _items.FindIndex(i => i.ItemId == item.ItemId);
        if (index < 0) return null;

        _items[index] = item;
        return item;
    }

}