using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ProviderStuff.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jellyfin.Plugin.ProviderStuff.Tests;

public class ProviderServiceTests
{
    [Fact]
    public async Task GetProvidersForAsyncReturnsDistinctProvidersForConfiguredCountry()
    {
        const string Json = """
            {
              "results": {
                "BR": {
                  "flatrate": [{ "provider_id": 8, "provider_name": "Example", "logo_path": null }],
                  "rent": [{ "provider_id": 8, "provider_name": "Example", "logo_path": null }],
                  "buy": [{ "provider_id": 337, "provider_name": "Other", "logo_path": null }]
                }
              }
            }
            """;
        using var client = new HttpClient(new StubHandler(Json));
        var service = new ProviderService(client, NullLogger<ProviderService>.Instance);
        var config = new PluginConfiguration
        {
            TmdbApiKey = "test-key",
            TmdbCountry = "BR"
        };

        var result = await service.GetProvidersForAsync("123", "movie", config, CancellationToken.None);

        Assert.Equal(new[] { 8, 337 }, result.Order());
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _response;

        public StubHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response)
            });
        }
    }
}
