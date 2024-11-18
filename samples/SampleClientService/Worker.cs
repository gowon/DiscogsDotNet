namespace SampleClientService;

using DiscogsDotNet.V2;
using Newtonsoft.Json.Linq;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDiscogsClientV2 _client;

    public Worker(ILogger<Worker> logger, IDiscogsClientV2 client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var username = "gowon";
                
        var response = await _client.CollectionValueAsync(username, stoppingToken) as JToken;
                _logger.LogInformation($"response: {response!.ToString()}");

        var items = await _client.GetCollectionItemsByFolderAsync(username, "0", "added", "asc", stoppingToken) as JToken;

        foreach (var item in items["releases"]) {
            _logger.LogInformation($"response: {item!.ToString()}");
        }

    }
}
