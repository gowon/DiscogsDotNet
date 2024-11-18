namespace build.Commands;

using System.CommandLine;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;

public sealed class OpenApiCommand : Command
{
    public OpenApiCommand() : base("openapi", "OpenAPI operations")
    {
        AddCommand(new ProcessSamplesCommand());
    }
}

[NotMapped]
public sealed class ProcessSamplesCommand : Command
{
    const string schemaPadding = "    ";
    const string dataPadding = "      ";

    public readonly Argument<DirectoryInfo> InputDirectoryArgument =
        new("input", "Input directory");

    public ProcessSamplesCommand() : base("process-samples",
        "")
    {
        AddArgument(InputDirectoryArgument);

        this.SetHandler(context =>
        {
            // download file
            // extract samples
            // convert samples to yaml
            // process yaml files
            var inputDirectory = context.ParseResult.GetValueForArgument(InputDirectoryArgument);
            var lines = new List<string>() { "components:", "  schemas:" };

            foreach (var file in inputDirectory.GetFiles())
            {
                lines.Add(schemaPadding + ConvertFileNameToSchemaName(Path.GetFileNameWithoutExtension(file.Name)));
                lines.AddRange(File.ReadLines(file.FullName)
                    .Select(line => dataPadding + line));
            }

            // combine into openapi components chunk
            File.WriteAllLines(Path.Combine(inputDirectory.FullName, "openapi.schemas.yaml"), lines);

            return Task.CompletedTask;
        });
    }

    private static string ConvertFileNameToSchemaName(string filename)
    {
        // ex. spec\samples\yaml\valid_artist_releases.yaml
        filename = filename.Replace("valid_", "");
        return new CultureInfo("en").TextInfo.ToTitleCase(filename.ToLower().Replace("_", " ")).Replace(" ", "") + ":";
    }
}