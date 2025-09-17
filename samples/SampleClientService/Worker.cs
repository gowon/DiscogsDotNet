namespace SampleClientService;

using System.IO.Abstractions;
using DiscogsDotNet.V2;
using NPOI.XSSF.UserModel;

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

            if (priceSuggestions.Fair__F != null)
            {
                record.FairMarketValue = Math.Round(priceSuggestions.Fair__F.Value, 2);
            }

            if (priceSuggestions.Very_Good__VG != null)
            {
                record.VeryGoodMarketValue = Math.Round(priceSuggestions.Very_Good__VG.Value, 2);
            }

            if (priceSuggestions.Mint__M != null)
            {
                record.MintMarketValue = Math.Round(priceSuggestions.Mint__M.Value, 2);
            }

            records.Add(record);
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

        // Create a cell style
        // ref: https://stackoverflow.com/a/45566070/7644876
        var currencyCellStyle = workbook.CreateCellStyle();
        currencyCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("$#,##0.00");

        // Create a new sheet
        var sheet = workbook.CreateSheet($"Discogs Collection - {_options.DiscogsUser}");

        // Create and freeze the header row
        var headerRow = sheet.CreateRow(0);
        sheet.CreateFreezePane(0, 1, 0, 1);

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
                var cell = row.CreateCell(4);
                cell.SetCellValue(record.FairMarketValue.Value);
                cell.CellStyle = currencyCellStyle;
            }
            else
            {
                row.CreateCell(4).SetBlank();
            }

            if (record.VeryGoodMarketValue.HasValue)
            {
                var cell = row.CreateCell(5);
                cell.SetCellValue(record.VeryGoodMarketValue.Value);
                cell.CellStyle = currencyCellStyle;
            }
            else
            {
                row.CreateCell(5).SetBlank();
            }

            if (record.MintMarketValue.HasValue)
            {
                var cell = row.CreateCell(6);
                cell.SetCellValue(record.MintMarketValue.Value);
                cell.CellStyle = currencyCellStyle;
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