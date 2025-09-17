namespace DiscogsDotNet.V2;

public class RateLimitingRequestHandler : DelegatingHandler
{
    private readonly TimeSpan _delayBetweenRequests;
    private readonly SemaphoreSlim _throttler;
    private readonly bool _useJitter;
    private readonly Random _random = new();

    public RateLimitingRequestHandler(int maxConcurrentRequests, TimeSpan delayBetweenRequests,
        bool useJitter = false)
    {
        _useJitter = useJitter;
        _throttler = new SemaphoreSlim(maxConcurrentRequests);
        _delayBetweenRequests = delayBetweenRequests;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Requests are throttled by the server by source IP to 60 per minute for authenticated requests
        // ref: https://www.discogs.com/developers#page:home,header:home-rate-limiting
        // apply up to 250ms of jitter
        var delay = _useJitter
            ? _delayBetweenRequests + TimeSpan.FromMilliseconds(_random.Next(0, 250))
            : _delayBetweenRequests;

        await _throttler.WaitAsync(cancellationToken);
        try
        {
            await Task.Delay(delay, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _throttler.Release();
        }
    }
}