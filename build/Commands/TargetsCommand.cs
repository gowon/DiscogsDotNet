namespace build.Commands;

using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Bullseye;
using Extensions;
using Microsoft.Extensions.DependencyInjection;
using static Bullseye.Targets;
using static SimpleExec.Command;

public sealed class TargetsCommand : Command
{
    private static readonly Option<string> AdditionalArgumentsOption =
        new(new[] { "--additional-args", "-a" }, "Additional arguments to be processed by the targets");

    private static readonly Option<string> ConfigurationOption =
        new(new[] { "--configuration", "-C" }, () => "Release", "The configuration to run the target");

    public TargetsCommand() : base("targets", "Execute build targets")
    {
        AddOption(AdditionalArgumentsOption);
        AddOption(ConfigurationOption);

        ImportBullseyeConfigurations();

        this.SetHandler(async context =>
        {
            // pre-processing
            var provider = context.GetHost().Services;
            var options = provider.GetRequiredService<TargetsCommandOptions>();
            var additionalArgs = context.ParseResult.GetValueForOption(AdditionalArgumentsOption);
            var configuration = context.ParseResult.GetValueForOption(ConfigurationOption);
            var workingDirectory = context.GetWorkingDirectory();

            // find most-explicit project/solution path for dotnet commands
            // ref: https://developercommunity.visualstudio.com/t/docker-compose-project-confuses-dotnet-build/615379
            // ref: https://developercommunity.visualstudio.com/t/multiple-docker-compose-dcproj-in-a-visual-studio/252877
            var findDotnetSolution = Directory.GetFiles(workingDirectory).FirstOrDefault(s => s.EndsWith(".sln")) ??
                                     string.Empty;
            var dotnetSolutionPath = Path.Combine(workingDirectory, findDotnetSolution);

            Target(Targets.RestoreTools, "Restore .NET command line tools",
                async () => { await RunAsync("dotnet", "tool restore"); });

            Target(Targets.CleanArtifactsOutput, "Delete all artifacts and folder", () =>
            {
                if (Directory.Exists(options.ArtifactsDirectory))
                {
                    Directory.Delete(options.ArtifactsDirectory, true);
                }
            });

            Target(Targets.CleanTestsOutput, "Delete all test artifacts and folder", () =>
            {
                if (Directory.Exists(options.TestResultsDirectory))
                {
                    Directory.Delete(options.TestResultsDirectory, true);
                }
            });

            Target(Targets.CleanBuildOutput, "Clear solution of all build artifacts",
                async () =>
                {
                    await RunAsync("dotnet", $"clean {dotnetSolutionPath} -c {configuration} -v m --nologo");
                });

            Target(Targets.CleanAll, "Execute all 'Clean' operations",
                DependsOn(Targets.CleanArtifactsOutput, Targets.CleanTestsOutput, Targets.CleanBuildOutput));

            Target(Targets.Build, "Build all projects in the solution", DependsOn(Targets.CleanBuildOutput),
                async () => { await RunAsync("dotnet", $"build {dotnetSolutionPath} -c {configuration} --nologo"); });

            Target(Targets.Pack, "Package projects for deployment",
                DependsOn(Targets.CleanArtifactsOutput, Targets.Build), async () =>
                {
                    await RunAsync("dotnet",
                        $"pack {dotnetSolutionPath} -c {configuration} -o {Directory.CreateDirectory(options.ArtifactsDirectory).FullName} --no-build --nologo");
                });

            Target(Targets.PublishArtifacts, "Publish artifacts", DependsOn(Targets.Pack), async () =>
            {
                await RunAsync("dotnet",
                    $"nuget push .\\{options.ArtifactsDirectory}\\*.nupkg --api-key {options.Nuget.ApiKey} --source {options.Nuget.Source} --skip-duplicate");
            });

            Target("default", DependsOn(Targets.RunTests, Targets.Pack));

            Target(Targets.RunTests, "Run automated tests for the solution",
                DependsOn(Targets.CleanTestsOutput, Targets.Build), async () =>
                {
                    await RunAsync("dotnet",
                        $"test {dotnetSolutionPath} -c {configuration} --no-build --nologo --collect:\"XPlat Code Coverage\" --results-directory {options.TestResultsDirectory} {additionalArgs}");
                });

            Target(Targets.RunTestsCoverage, "Run automated tests for the solution and generate code coverage report",
                DependsOn(Targets.RestoreTools, Targets.RunTests), () =>
                    Run("dotnet",
                        $"reportgenerator -reports:{options.TestResultsDirectory}/**/*cobertura.xml -targetdir:{options.TestResultsDirectory}/coveragereport -reporttypes:HtmlSummary"));

            await RunBullseyeTargetsAsync(context);
        });
    }

    private void ImportBullseyeConfigurations()
    {
        Add(new Argument<string[]>("targets")
        {
            Description =
                "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed. Target names may be abbreviated. For example, \"b\" for \"build\"."
        });

        foreach (var (aliases, description) in Bullseye.Options.Definitions)
        {
            Add(new Option<bool>(aliases.ToArray(), description));
        }
    }

    private async Task RunBullseyeTargetsAsync(InvocationContext context)
    {
        var targets = context.ParseResult.CommandResult.Tokens.Select(token => token.Value);
        var options = new Options(Bullseye.Options.Definitions.Select(definition => (definition.Aliases[0],
            context.ParseResult.GetValueForOption(Options.OfType<Option<bool>>()
                .Single(option => option.HasAlias(definition.Aliases[0]))))));
        await RunTargetsWithoutExitingAsync(targets, options);
    }
}

internal static class Targets
{
    public const string RunTestsCoverage = "run-tests-coverage";
    public const string RestoreTools = "restore-tools";
    public const string CleanBuildOutput = "clean-build-output";
    public const string CleanArtifactsOutput = "clean-artifacts-output";
    public const string CleanTestsOutput = "clean-test-output";
    public const string CleanAll = "clean";
    public const string Build = "build";
    public const string Pack = "pack";
    public const string PublishArtifacts = "publish-artifacts";
    public const string RunTests = "run-tests";
}