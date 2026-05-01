namespace csharp_fastapi_template.tests.entities;

using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.errors;
using Xunit;

public class TestItem
{
    // podemos seguir nessa classe de testes um padrao em que
    // testamos apenas os metodos de validaçao do construtor
    // e a criação do próprio item. quero dizer que por exemplo
    // nao temos um validador no construtor referente ao admin
    // permission, entao nao tem necessidade de levantar um teste
    // para isso (alem do de criação), dado que o compilador ja 
    // cuida disso.
    private const string FixedId = "88f0920c-0de0-4e0a-bb46-abdb3705579d";

    [Fact]
    public void TestCorrectItemCreation()
    {
        var item = new Item(
            itemId: FixedId,
            name: "test",
            price: 1.0f,
            itemType: ItemTypeEnum.Food,
            adminPermission: true
        );

        Assert.Equal("test", item.Name);
        Assert.Equal(1.0f, item.Price);
        Assert.Equal(ItemTypeEnum.Food, item.ItemType);
        Assert.True(item.AdminPermission);
    }

    [Theory]
    [InlineData("")] // id vazio
    [InlineData("not-a-guid")] // formato que não é UUID
    [InlineData("00000000-0000-0000-0000")] // UUID inválido

    public void ItemInvalidGUID(string badItemId)
    {
        Assert.Throws<ParamNotValidatedException>(() =>
            new Item(
                itemId: badItemId,
                name: "valid",
                price: 1f,
                itemType: ItemTypeEnum.Food
            ));
    }

    [Theory]
    [InlineData("")] // nome vazio
    [InlineData("jo")] // nome menor que 3 letras
    public void ItemInvalidName(string badName)
    {
        Assert.Throws<ParamNotValidatedException>(() =>
        new Item(
            itemId: FixedId,
            name: badName,
            price: 1f,
            itemType: ItemTypeEnum.Food
        ));
    }

    [Theory]
    [InlineData(-1.0f)] // preço negativo
    public void ItemInvalidPrice(float badPrice)
    {
        Assert.Throws<ParamNotValidatedException>(() => 
        new Item(
            itemId: FixedId,
            name: "valid",
            price: badPrice,
            itemType: ItemTypeEnum.Toy
        ));
    }
}
