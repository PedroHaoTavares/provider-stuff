using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Jellyfin.Plugin.ProviderStuff;
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
