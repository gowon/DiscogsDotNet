namespace DiscogsDotNet.V2;

using System.Text;
using Boiler.QuartzWorker.RestApi.Client;

public partial class DiscogsClientV2
{
    private Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url,
        CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(Constants.ApiVersionHeader, Constants.V1.ApiVersion);
        return Task.CompletedTask;
    }

    private Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}