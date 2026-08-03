using System;
using System.Linq;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Xunit;

namespace Jellyfin.Plugin.ProviderStuff.Tests;

public class ProviderCollectionPlannerTests
{
    [Fact]
    public void ProviderCollectionIncludesTaggedMoviesAndSeriesButNotEpisodes()
    {
        var movie = new Movie { Tags = new[] { "provider:Netflix" } };
        var series = new Series { Tags = new[] { "PROVIDER:NETFLIX" } };
        var episode = new Episode { Tags = new[] { "provider:Netflix" } };
        var untaggedMovie = new Movie { Tags = Array.Empty<string>() };

        Assert.True(ProviderCollectionPlanner.IsProviderCollectionItem(movie, "Netflix"));
        Assert.True(ProviderCollectionPlanner.IsProviderCollectionItem(series, "Netflix"));
        Assert.False(ProviderCollectionPlanner.IsProviderCollectionItem(episode, "Netflix"));
        Assert.False(ProviderCollectionPlanner.IsProviderCollectionItem(untaggedMovie, "Netflix"));
    }

    [Fact]
    public void SyncPlanAddsMissingAndRemovesStaleItems()
    {
        var retained = Guid.NewGuid();
        var missing = Guid.NewGuid();
        var stale = Guid.NewGuid();

        var plan = ProviderCollectionPlanner.CreateSyncPlan(
            new[] { retained, missing },
            new[] { retained, stale });

        Assert.Equal(new[] { missing }, plan.ItemIdsToAdd);
        Assert.Equal(new[] { stale }, plan.ItemIdsToRemove);
    }

    [Fact]
    public void SyncPlanIgnoresDuplicateIds()
    {
        var itemId = Guid.NewGuid();

        var plan = ProviderCollectionPlanner.CreateSyncPlan(
            Enumerable.Repeat(itemId, 2),
            new[] { itemId });

        Assert.Empty(plan.ItemIdsToAdd);
        Assert.Empty(plan.ItemIdsToRemove);
    }
}
