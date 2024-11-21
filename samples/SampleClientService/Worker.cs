namespace SampleClientService;

using DiscogsDotNet.V2;

public class Worker : BackgroundService
{
    private readonly WorkerOptions _options;
    private readonly ILogger<Worker> _logger;
    private readonly IDiscogsClientV2 _client;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public Worker(ILogger<Worker> logger, IDiscogsClientV2 client, IHostApplicationLifetime applicationLifetime, WorkerOptions options)
    {
        _logger = logger;
        _client = client;
        _applicationLifetime = applicationLifetime;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // get collection value
        var value = await _client.CollectionValueAsync(_options.DiscogsUser, stoppingToken);
        _logger.LogInformation("User {User}'s collection Value Range: {MinValue}, {MedianValue}, {MaxValue}",
            _options.DiscogsUser, value.Minimum, value.Median, value.Maximum);

        // page to get all collection items
        var i = 0;
        string next;
        var releases = new List<releases2>();
        do
        {
            i++;
            var collection = await _client.GetCollectionItemsByFolderAsync(_options.DiscogsUser, "0", i, 100, "added", "asc", stoppingToken);
            releases.AddRange(collection.Releases);
            next = collection.Pagination.Urls.Next;
        } while (!string.IsNullOrEmpty(next));
        _logger.LogInformation("Retrieved {TotalReleases} releases from user {User}'s collection", releases.Count(), _options.DiscogsUser);

        foreach (var (index, release) in releases.Select((r, i) => new KeyValuePair<int, releases2>(i,r)))
        {
            _logger.LogInformation("[{Index}]: {Artist} - {Title}", 
                index, release.Basic_information.Artists.First().Name, release.Basic_information.Title);
        }


        foreach (var release in releases.Take(5))
        {
            var priceSuggestions = await _client.GetPriceSuggestionsAsync(release.Id.ToString(), stoppingToken);
            _logger.LogInformation("Price Suggestion for {Title}: ${Price}",
                release.Basic_information.Title, priceSuggestions.Mint__M);
        }

        _applicationLifetime.StopApplication();
    }
}
