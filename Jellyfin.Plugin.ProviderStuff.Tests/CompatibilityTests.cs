using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Jellyfin.Plugin.ProviderStuff.Configuration;
using Jellyfin.Plugin.ProviderStuff.ScheduledTasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Xunit;

namespace Jellyfin.Plugin.ProviderStuff.Tests;

public class CompatibilityTests
{
    [Fact]
    public void PluginGuidMatchesConfigurationPage()
    {
        var plugin = (Plugin)RuntimeHelpers.GetUninitializedObject(typeof(Plugin));
        using var stream = typeof(Plugin).Assembly.GetManifestResourceStream(
            "Jellyfin.Plugin.ProviderStuff.Configuration.configPage.html");

        Assert.NotNull(stream);
        using var reader = new StreamReader(stream);
        var configurationPage = reader.ReadToEnd();

        Assert.Equal(Guid.Parse("2be7759b-4e1b-4965-94ad-37d80c84b506"), plugin.Id);
        Assert.Contains(plugin.Id.ToString(), configurationPage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProviderCollectionsAreEnabledByDefault()
    {
        Assert.True(new PluginConfiguration().EnableProviderCollections);
    }

    [Fact]
    public void ConfigurationPageAppearsInDashboardMenu()
    {
        var plugin = (Plugin)RuntimeHelpers.GetUninitializedObject(typeof(Plugin));

        var page = Assert.Single(plugin.GetPages());

        Assert.True(page.EnableInMainMenu);
    }

    [Fact]
    public void ConfigurationPageContainsAccessibleProviderControls()
    {
        using var stream = typeof(Plugin).Assembly.GetManifestResourceStream(
            "Jellyfin.Plugin.ProviderStuff.Configuration.configPage.html");

        Assert.NotNull(stream);
        using var reader = new StreamReader(stream);
        var configurationPage = reader.ReadToEnd();

        Assert.Contains("createElement('fieldset')", configurationPage, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", configurationPage, StringComparison.Ordinal);
        Assert.Contains("Nome exibido da coleção", configurationPage, StringComparison.Ordinal);
    }

    [Fact]
    public void ProviderCollectionIdentityPersistsInConfiguration()
    {
        var collectionId = Guid.NewGuid();
        var provider = new Provider
        {
            Name = "Netflix",
            CollectionName = "Minha Netflix",
            CollectionId = collectionId,
            UpdateCollectionImage = true
        };
        var serializer = new XmlSerializer(typeof(Provider));
        using var writer = new StringWriter();
        serializer.Serialize(writer, provider);
        using var reader = new StringReader(writer.ToString());

        var restored = Assert.IsType<Provider>(serializer.Deserialize(reader));

        Assert.Equal(collectionId, restored.CollectionId);
        Assert.Equal("Minha Netflix", restored.CollectionName);
        Assert.True(restored.UpdateCollectionImage);
    }

    [Fact]
    public void GetItemListUsesJellyfin1011Contract()
    {
        var method = typeof(ILibraryManager).GetMethod(
            nameof(ILibraryManager.GetItemList),
            new[] { typeof(InternalItemsQuery) });

        Assert.NotNull(method);
        Assert.Equal(typeof(IReadOnlyList<BaseItem>), method.ReturnType);
    }

    [Fact]
    public void DefaultTriggerIsDailyAtThreeAm()
    {
        var task = (ApplyProviderTagsTask)RuntimeHelpers.GetUninitializedObject(typeof(ApplyProviderTagsTask));

        var trigger = Assert.Single(task.GetDefaultTriggers());

        Assert.Equal(TaskTriggerInfoType.DailyTrigger, trigger.Type);
        Assert.Equal(TimeSpan.FromHours(3).Ticks, trigger.TimeOfDayTicks);
    }
}
