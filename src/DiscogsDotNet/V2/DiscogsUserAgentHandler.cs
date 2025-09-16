namespace DiscogsDotNet.V2;

public class DiscogsUserAgentHandler : DelegatingHandler
{
    private readonly DiscogsClientV2Options _options;

    public DiscogsUserAgentHandler(DiscogsClientV2Options options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation("User-Agent",
            $"{_options.UserAgent}-{nameof(DiscogsDotNet)}/1.0 +https://github.com/gowon/DiscogsDotNet");
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}