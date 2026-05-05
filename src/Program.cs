using Amazon.Lambda.AspNetCoreServer.Hosting;
using csharp_fastapi_template.adapters.contracts;
using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;
using csharp_fastapi_template.errors;
using csharp_fastapi_template.repo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// injeta repositório mock como singleton em memória
builder.Services.AddSingleton<IItemRepository, ItemRepositoryMock>();

// habilita execução como Lambda (API Gateway HTTP API)
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.MapGet("/items/get_all_items", (IItemRepository repo) =>
{
    var items = repo.GetAllItems().Select(i => i.ToDict());
    return Results.Ok(new { items });
});

app.MapPost("/items/create_item", (CreateItemRequest request, IItemRepository repo) =>
{

    // validações de controller e usecase

    if (repo.GetItem(request.ItemId) is not null)
    {
        return Results.Conflict(new { detail = "Item already exists" });
    }

    if (!Enum.TryParse<ItemTypeEnum>(request.ItemType, ignoreCase: true, out var itemType))
    {
        return Results.BadRequest(new { detail = "Item type is not a valid one" });
    }

    try
    {
        var item = new Item(
            itemId: request.ItemId,
            name: request.Name,
            price: request.Price,
            itemType: itemType,
            adminPermission: request.AdminPermission
        );

        var createdItem = repo.CreateItem(item);
        return Results.Created($"/items/{createdItem.ItemId}", new
        {
            item_id = createdItem.ItemId,
            item = createdItem.ToDict()
        });
    }
    catch (ParamNotValidatedException err)
    {
        return Results.BadRequest(new { detail = err.Message });
    }
});

app.Run();