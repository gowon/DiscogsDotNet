namespace SampleClientService;

using DiscogsDotNet.V2;
using NPOI.XSSF.UserModel;
using Polly.RateLimiting;
using System.IO.Abstractions;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IDiscogsClientV2 _client;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<Worker> _logger;
    private readonly WorkerOptions _options;

    public Worker(ILogger<Worker> logger, IDiscogsClientV2 client, IHostApplicationLifetime applicationLifetime,
        WorkerOptions options, IFileSystem fileSystem)
    {
        _logger = logger;
        _client = client;
        _applicationLifetime = applicationLifetime;
        _options = options;
        _fileSystem = fileSystem;
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
            var collection = await _client.GetCollectionItemsByFolderAsync(_options.DiscogsUser, "0", i, 100, "added",
                "asc", stoppingToken);
            releases.AddRange(collection.Releases);
            next = collection.Pagination.Urls.Next;
        } while (!string.IsNullOrEmpty(next));

        _logger.LogInformation("Retrieved {TotalReleases} releases from user {User}'s collection", releases.Count,
            _options.DiscogsUser);

        var records = new List<Record>();

        try
        {
            foreach (var release in releases)
            {
                var record = new Record
                {
                    Artist = release.Basic_information.Artists.First().Name,
                    Title = release.Basic_information.Title,
                    Year = release.Basic_information.Year,
                    Format = release.Basic_information.Formats.First().Name
                };

                var priceSuggestions = await _client.GetPriceSuggestionsAsync(release.Id.ToString(), stoppingToken);

                record.FairMarketValue = Math.Round(priceSuggestions.Fair__F.Value, 2);
                record.VeryGoodMarketValue = Math.Round(priceSuggestions.Very_Good__VG.Value, 2);
                record.MintMarketValue = Math.Round(priceSuggestions.Mint__M.Value, 2);

                records.Add(record);
            }
        }
        catch (RateLimiterRejectedException ex)
        {
            Console.WriteLine($"Operation ultimately failed due to rate limiting: {ex.Message}");
        }

        var path = _fileSystem.Path.Join(_fileSystem.Directory.GetCurrentDirectory(),
            $"{_options.DiscogsUser}-collection-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
        _logger.LogInformation("Creating new spreadsheet `{FilePath}`", path);
        CreateExcelWithHeaders(records, path);
        _logger.LogInformation("File created.");
       
        _applicationLifetime.StopApplication();
    }

    private void CreateExcelWithHeaders(IEnumerable<Record> records, string filePath)
    {
        // Create a new Excel workbook (XSSFWorkbook for .xlsx, HSSFWorkbook for .xls)
        var workbook = new XSSFWorkbook();

        // Create a new sheet
        var sheet = workbook.CreateSheet();

        // Create the header row
        var headerRow = sheet.CreateRow(0);

        // Define your header names
        string[] headers =
        [
            nameof(Record.Artist), nameof(Record.Title), nameof(Record.Year), nameof(Record.Format),
            nameof(Record.FairMarketValue), nameof(Record.VeryGoodMarketValue), nameof(Record.MintMarketValue)
        ];

        // Populate the header row cells
        for (var i = 0; i < headers.Length; i++)
        {
            headerRow.CreateCell(i).SetCellValue(headers[i]);
        }

        // write rows
        foreach (var (record, index) in records.Select((item, i) => (item, i)))
        {
            var row = sheet.CreateRow(index + 1);
            row.CreateCell(0).SetCellValue(record.Artist);
            row.CreateCell(1).SetCellValue(record.Title);
            row.CreateCell(2).SetCellValue(record.Year);
            row.CreateCell(3).SetCellValue(record.Format);

            if (record.FairMarketValue.HasValue)
            {
                row.CreateCell(4).SetCellValue(record.FairMarketValue.Value);
            }
            else
            {
                row.CreateCell(4).SetBlank();
            }

            if (record.VeryGoodMarketValue.HasValue)
            {
                row.CreateCell(5).SetCellValue(record.VeryGoodMarketValue.Value);
            }
            else
            {
                row.CreateCell(5).SetBlank();
            }

            if (record.MintMarketValue.HasValue)
            {
                row.CreateCell(6).SetCellValue(record.MintMarketValue.Value);
            }
            else
            {
                row.CreateCell(6).SetBlank();
            }
        }

        // Save the workbook to a file
        using (var stream = _fileSystem.FileStream.New(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(stream);
        }
    }
}