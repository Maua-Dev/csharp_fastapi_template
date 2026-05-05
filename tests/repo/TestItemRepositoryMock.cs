namespace csharp_fastapi_template.tests.repo;

using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.repo;
using Xunit;

public class TestItemRepositoryMock
{
    private const string ExistingItemId = "b11af449-22c7-43db-b0e4-dbfbbe7fdbd7";
    private const string MissingItemId = "11111111-1111-1111-1111-111111111111";

    // Como o repositório mock usa lista em memória, criamos uma instância nova
    // em cada teste para manter isolamento entre cenários.
    private static ItemRepositoryMock CreateRepo() => new();

    [Fact]
    public void GetAllItems_ShouldReturnInitialItems()
    {
        var repo = CreateRepo();

        var items = repo.GetAllItems();

        Assert.Equal(4, items.Count);
    }

    [Theory]
    [InlineData(ExistingItemId, true)]
    [InlineData(MissingItemId, false)]
    public void GetItem_ShouldMatchExpectedExistence(string itemId, bool shouldExist)
    {
        var repo = CreateRepo();

        var item = repo.GetItem(itemId);

        if (shouldExist)
        {
            Assert.NotNull(item);
            Assert.Equal(itemId, item!.ItemId);
            return;
        }

        Assert.Null(item);
    }

    [Fact]
    public void CreateItem_ShouldAddItemToRepository()
    {
        var repo = CreateRepo();
        var newItem = new Item(
            itemId: "74fc6d4f-44c6-42dc-a739-5169a9c33f11",
            name: "Notebook",
            price: 1200.00f,
            itemType: ItemTypeEnum.Games,
            adminPermission: false
        );

        var created = repo.CreateItem(newItem);
        var found = repo.GetItem(newItem.ItemId);

        Assert.Equal(newItem, created);
        Assert.NotNull(found);
        Assert.Equal("Notebook", found!.Name);
        Assert.Equal(5, repo.GetAllItems().Count);
    }

    [Fact]
    public void DeleteItem_ShouldRemoveAndReturnDeletedItem()
    {
        var repo = CreateRepo();

        var deleted = repo.DeleteItem(ExistingItemId);
        var afterDelete = repo.GetItem(ExistingItemId);

        Assert.NotNull(deleted);
        Assert.Equal(ExistingItemId, deleted!.ItemId);
        Assert.Null(afterDelete);
        Assert.Equal(3, repo.GetAllItems().Count);
    }

    [Fact]
    public void DeleteItem_WhenItemDoesNotExist_ShouldReturnNull()
    {
        var repo = CreateRepo();

        var deleted = repo.DeleteItem(MissingItemId);

        Assert.Null(deleted);
        Assert.Equal(4, repo.GetAllItems().Count);
    }

    [Fact]
    public void UpdateItem_ShouldReplaceStoredItem()
    {
        var repo = CreateRepo();
        var updatedItem = new Item(
            itemId: ExistingItemId,
            name: "Barbie Collector",
            price: 99.99f,
            itemType: ItemTypeEnum.Toy,
            adminPermission: true
        );

        var result = repo.UpdateItem(updatedItem);
        var stored = repo.GetItem(ExistingItemId);

        Assert.NotNull(result);
        Assert.NotNull(stored);
        Assert.Equal("Barbie Collector", stored!.Name);
        Assert.Equal(99.99f, stored.Price);
        Assert.True(stored.AdminPermission);
    }

    [Fact]
    public void UpdateItem_WhenItemDoesNotExist_ShouldReturnNull()
    {
        var repo = CreateRepo();
        var updatedItem = new Item(
            itemId: "2e9b9e3e-91f5-4ae9-9f75-b85e9f8ec0bd",
            name: "Not Found",
            price: 10f,
            itemType: ItemTypeEnum.Food
        );

        var result = repo.UpdateItem(updatedItem);

        Assert.Null(result);
        Assert.Equal(4, repo.GetAllItems().Count);
    }
}
