using System.Text.Json;

namespace CoffeeShop.Mobile.Services;

public sealed class FavoriteService
{
    private const string StorageKey = "coffee_shop_favorite_product_ids";

    public IReadOnlySet<int> GetProductIds()
    {
        var stored = Preferences.Get(StorageKey, "[]");
        var ids = JsonSerializer.Deserialize<HashSet<int>>(stored);
        return ids ?? [];
    }

    public bool Contains(int productId) => GetProductIds().Contains(productId);

    public bool Toggle(int productId)
    {
        var ids = GetProductIds().ToHashSet();
        var isAdded = ids.Add(productId);
        if (!isAdded)
            ids.Remove(productId);
        Preferences.Set(StorageKey, JsonSerializer.Serialize(ids));
        return isAdded;
    }

    public void Remove(int productId)
    {
        var ids = GetProductIds().ToHashSet();
        if (ids.Remove(productId))
            Preferences.Set(StorageKey, JsonSerializer.Serialize(ids));
    }
}
