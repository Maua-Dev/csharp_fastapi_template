using csharp_fastapi_template.enums;
using csharp_fastapi_template.errors;

namespace csharp_fastapi_template.entities;

public class Item
{
    // Propriedades — equivalente aos atributos da entidade em Python
    // Em C# o { get; private set; } significa: qualquer um pode LER,
    // mas só a própria classe pode ESCREVER. Encapsulamento nativo!
    public string ItemId { get; private set; }
    public string Name { get; private set; }
    public float Price { get; private set; }
    public ItemTypeEnum ItemType { get; private set; }
    public bool AdminPermission { get; private set; }

    // Construtor — equivalente ao __init__ do Python
    public Item(
        string itemId,
        string name,
        float price,
        ItemTypeEnum itemType,
        bool adminPermission = false // default value, igual ao Python
    )
    {
        // Cada Validate lança a exceção internamente se inválido,
        // então não precisamos do if/raise em cada linha
        ValidateItemId(itemId);
        ItemId = itemId;

        ValidateName(name);
        Name = name;

        ValidatePrice(price);
        Price = price;

        // ItemType não precisa de validação extra: o compilador
        // já garante que só um valor do enum pode ser passado!
        ItemType = itemType;

        // Aqui admin permission nunca poderá ser Null.
        AdminPermission = adminPermission;
    }

    // Métodos de validação — static, igual ao @staticmethod do Python
    // Em vez de retornar (bool, string), lançamos a exceção direto.
    // Isso é mais idiomático em C#.
    private static void ValidateItemId(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ParamNotValidatedException("ItemId", "Item id is required");

        if (!Guid.TryParse(itemId, out _))
            throw new ParamNotValidatedException("ItemId", "Item id must be a valid UUID");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ParamNotValidatedException("Name", "Name is required");

        if (name.Length < 3)
            throw new ParamNotValidatedException("Name", "Name must be at least 3 characters");
    }

    private static void ValidatePrice(float price)
    {
        if (price < 0)
            throw new ParamNotValidatedException("Price", "Price must be positive");
    }

    // Equivalente ao to_dict() do Python
    public Dictionary<string, object> ToDict()
    {
        return new Dictionary<string, object>
        {
            { "item_id", ItemId },
            { "name", Name },
            { "price", Price },
            { "item_type", ItemType.ToString() },
            { "admin_permission", AdminPermission }
        };
    }

    // Equivalente ao __eq__ do Python
    public override bool Equals(object? obj)
    {
        if (obj is not Item other) return false;

        return Name == other.Name &&
               Price == other.Price &&
               ItemType == other.ItemType &&
               AdminPermission == other.AdminPermission;
    }

    // Em C# sempre que sobrescreve Equals, precisa sobrescrever isso também
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Price, ItemType, AdminPermission);
    }

    // Equivalente ao __repr__ do Python
    public override string ToString()
    {
        return $"Item(Name={Name}, Price={Price}, ItemType={ItemType}, AdminPermission={AdminPermission})";
    }
}