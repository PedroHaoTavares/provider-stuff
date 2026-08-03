using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.ProviderStuff.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.ProviderStuff;

/// <summary>
/// Builds provider collection membership without changing library items.
/// </summary>
public static class ProviderCollectionPlanner
{
    /// <summary>
    /// Gets the configured collection name, falling back to the provider/tag name.
    /// </summary>
    /// <param name="provider">Provider configuration.</param>
    /// <returns>The normalized collection name.</returns>
    public static string GetCollectionName(Provider provider)
    {
        return string.IsNullOrWhiteSpace(provider.CollectionName)
            ? provider.Name.Trim()
            : provider.CollectionName.Trim();
    }

    /// <summary>
    /// Determines whether an item should appear in a provider collection.
    /// </summary>
    /// <param name="item">Library item.</param>
    /// <param name="providerName">Configured provider name.</param>
    /// <returns><see langword="true"/> for tagged movies and series; otherwise <see langword="false"/>.</returns>
    public static bool IsProviderCollectionItem(BaseItem item, string providerName)
    {
        if (item is not Movie && item is not Series)
        {
            return false;
        }

        var providerTag = $"provider:{providerName}";
        return item.Tags?.Contains(providerTag, StringComparer.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Calculates the changes required to synchronize a collection.
    /// </summary>
    /// <param name="desiredItemIds">IDs that should belong to the collection.</param>
    /// <param name="currentItemIds">IDs currently in the collection.</param>
    /// <returns>IDs to add and remove.</returns>
    public static ProviderCollectionSyncPlan CreateSyncPlan(IEnumerable<Guid> desiredItemIds, IEnumerable<Guid> currentItemIds)
    {
        var desired = desiredItemIds.ToHashSet();
        var current = currentItemIds.ToHashSet();

        return new ProviderCollectionSyncPlan(
            desired.Except(current).ToArray(),
            current.Except(desired).ToArray());
    }
}
