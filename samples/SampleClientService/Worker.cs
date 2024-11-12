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
        var response = await _client.ReleasesGETAsync("249504", curr_abbr: "USD", cancellationToken: stoppingToken) as JToken; // returns JToken
        _logger.LogInformation($"response: {response!.ToString()}");
    }
}
