namespace DiscogsDotNet.V2;
using System.Net.Http.Headers;

public class DiscogsPersonalAccessTokenAuthenticationHandler : DelegatingHandler
{
    private readonly DiscogsClientV2Options _options;

    public DiscogsPersonalAccessTokenAuthenticationHandler(DiscogsClientV2Options options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get the token or other type of credentials here
        if (!request.Headers.Contains("Authorization"))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Discogs", $"token={_options.PersonalAccessToken}");
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}