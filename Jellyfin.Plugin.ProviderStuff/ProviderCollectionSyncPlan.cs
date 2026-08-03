using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.ProviderStuff;

/// <summary>
/// Changes required to synchronize a provider collection.
/// </summary>
/// <param name="ItemIdsToAdd">Item IDs to add.</param>
/// <param name="ItemIdsToRemove">Item IDs to remove.</param>
public sealed record ProviderCollectionSyncPlan(
    IReadOnlyCollection<Guid> ItemIdsToAdd,
    IReadOnlyCollection<Guid> ItemIdsToRemove);
