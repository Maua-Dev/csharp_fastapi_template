namespace csharp_fastapi_template.adapters.contracts;

public record CreateItemRequest(
    string ItemId,
    string Name,
    float Price,
    string ItemType,
    bool AdminPermission
);
