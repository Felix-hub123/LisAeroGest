using Microsoft.Maui.Storage;
using System.Text.Json;

namespace LisAeroGest.Mobile.Services;

/// <summary>
/// Guarda localmente os voos favoritos. É rápido, privado e funciona offline.
/// </summary>
public class FavoritesService
{
    private const string Key = "favorite_flight_ids";

    public IReadOnlyCollection<int> GetFavoriteFlightIds()
    {
        try
        {
            var json = Preferences.Get(Key, "[]");
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            Preferences.Remove(Key);
            return Array.Empty<int>();
        }
    }

    public bool IsFavorite(int flightId) => GetFavoriteFlightIds().Contains(flightId);

    public bool Toggle(int flightId)
    {
        var ids = GetFavoriteFlightIds().ToHashSet();
        var isFavorite = ids.Contains(flightId);

        if (isFavorite)
            ids.Remove(flightId);
        else
            ids.Add(flightId);

        Preferences.Set(Key, JsonSerializer.Serialize(ids.OrderBy(x => x).ToList()));
        return !isFavorite;
    }

    public void Remove(int flightId)
    {
        var ids = GetFavoriteFlightIds().ToHashSet();
        ids.Remove(flightId);
        Preferences.Set(Key, JsonSerializer.Serialize(ids.OrderBy(x => x).ToList()));
    }
}
