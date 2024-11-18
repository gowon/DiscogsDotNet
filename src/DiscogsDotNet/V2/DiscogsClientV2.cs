namespace DiscogsDotNet.V2;

using Boiler.QuartzWorker.RestApi.Client;
using System.Text;

public partial class DiscogsClientV2
{
    //public DiscogsClientV2(HttpClient httpClient) : this(httpClient)
    //{
    //}


    private Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url,
        CancellationToken cancellationToken)
    {
        // todo: move to DelegatedHandler
        //request.Headers.TryAddWithoutValidation(Constants.ApiVersionHeader, Constants.V1.ApiVersion);
        request.Headers.TryAddWithoutValidation("User-Agent", $"{nameof(DiscogsDotNet)}/1.0 +https://github.com/gowon/DiscogsDotNet");
        return Task.CompletedTask;
    }

    private Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        //todo: Add debug logging for rate limiter headers
        return Task.CompletedTask;
    }
}