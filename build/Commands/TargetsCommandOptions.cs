namespace build.Commands;

using global::Extensions.Options.AutoBinder;

[AutoBind("BuildTargets")]
public class TargetsCommandOptions
{
    public NugetOptions Nuget { get; set; } = new();

    // based defaults in https://github.com/github/gitignore/blob/master/VisualStudio.gitignore
    public string ArtifactsDirectory { get; set; } = "artifacts";

    // based defaults in https://github.com/github/gitignore/blob/master/VisualStudio.gitignore
    public string TestResultsDirectory { get; set; } = "testresults";
}

public class NugetOptions
{
    public string? Source { get; set; }
    public string? ApiKey { get; set; }
}