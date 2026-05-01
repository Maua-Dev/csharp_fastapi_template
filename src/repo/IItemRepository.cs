namespace csharp_fastapi_template.repo;

using csharp_fastapi_template.entities;
using csharp_fastapi_template.enums;

public interface IItemRepository {

    /// <summary>
    /// Return all stored items.
    /// </summary>
    /// <returns>Collection of all items currently persisted.</returns>
    IList<Item> GetAllItems();

    /// <summary>
    /// Retrieve a single item by its identifier.
    /// </summary>
    /// <param name="itemId">UUID string stored in the entity attribute <c>item.ItemId</c>.</param>
    /// <returns>The matching item when found, otherwise <c>null</c>.</returns>
    Item? GetItem(string itemId);

    /// <summary>
    /// Persist a new item.
    /// </summary>
    /// <param name="item">Fully validated item entity.</param>
    /// <returns>The persisted item.</returns>
    Item CreateItem(Item item);

    /// <summary>
    /// Delete an item by its identifier.
    /// </summary>
    /// <param name="itemId">UUID string of the target item.</param>
    /// <returns>Deleted item when found, otherwise <c>null</c>.</returns>
    Item? DeleteItem(string itemId);

    /// <summary>
    /// Replaces the item identified by <paramref name="itemId"/> with the supplied state.
    /// Full resource replacement (HTTP <c>PUT</c>), not a sparse partial update (HTTP <c>PATCH</c>).
    /// </summary>
    /// <remarks>
    /// Callers should pass the complete intended values for mutable fields so the persisted item
    /// matches the new representation end-to-end, consistent with <c>PUT</c>.
    /// </remarks>
    /// <param name="item">Fully validated item entity.</param>
    /// <returns>Replaced item when found, otherwise <c>null</c>.</returns>
    Item? UpdateItem(Item item);
}