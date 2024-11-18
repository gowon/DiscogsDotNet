# build tool

This build tool is a vendor-less, modern, extensible build pipeline for local development operations and CI/CD automations.

## Overview

This project is built using the [Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) pattern. The [System.CommandLine](https://github.com/dotnet/command-line-api) package is used for command line parsing, invocation, and rendering of terminal output. Other relevant packages include:

- [Bullseye](https://github.com/adamralph/bullseye) - running a target dependency graph.
- [Docker.DotNet](https://github.com/dotnet/Docker.DotNet) - .NET (C#) Client Library for Docker API.
- [Extensions.Options.AutoBinder](https://github.com/gowon/Extensions.Options.AutoBinder) - Automatically binding strongly-typed option classes to configuration data.
- [SimpleExec](https://github.com/adamralph/simple-exec) - Running external commands.
- [Spectre.Console](https://spectreconsole.net/) - Fancy CLI output.

### Command Line Directives

> The purpose of directives is to provide cross-cutting functionality that can apply across command-line apps. Because directives are syntactically distinct from the app's own syntax, they can provide functionality that applies across apps.
> Read more about directives [here](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#directives).

#### The `[config]` directive

When the `[config]` directive is used during the execution of the command, the tool will provider an output of the current application host configuration before resuming execution. This is useful while debugging, to validate that your `buildconfig.json` or environment variables in your shell environment and are being properly captured by the tool. For example:

```powershell
build.ps1 [config] docker --summary
```

> Note: since the application host needs to be executed in order for the configurations to be hydrated, it's middleware is placed lower in the execution order. This causes the built in `--help` and `--version` global options middleware to execute first and will prevent `[config]` middleware from executing when those parameters are passed.

#### The `[debug]` directive

When the `[debug]` directive is used, the tool will pause and wait for the user to attach a debugger to the process for resuming execution. This is useful during the development of build tool extensions and can be used to debug the build tool in place in other environments that would otherwise be difficult to produce. For example:

```powershell
build.ps1 [debug] targets run-tests-coverage
```

Given the dangers of having a debug feature like this expose, the user must first set the environment variable `DOTNET_COMMANDLINE_DEBUG_PROCESSES` in the shell to the name of the process that you are trying to debug ('build' by default). If the `DOTNET_COMMANDLINE_DEBUG_PROCESSES` variable is not set, or the value does not match the name of the build tool's process, then the build tool will refuse to execute. For example, to create the environment variable in a PowerShell session, use the following command:

```powershell
$Env:DOTNET_COMMANDLINE_DEBUG_PROCESSES="build"
```

### Extending Build Targets

TargetsCommand uses [Bullseye](https://github.com/adamralph/bullseye) framework to integrate a target dependency graph into the build tool. New build targets can easily be added to the graph by adding them to the source code in this file.

> See more by looking at the [Bullseye documentation](https://github.com/adamralph/bullseye#quick-start).

### Extending Commands

The `SystemCommandLineExtensions.RegisterCommandsInAssembly` helper will scan for any class that inherits from `Command`, with a parameter-less constructor in the assembly, that is not decorated with the [`NotMappedAttribute`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.notmappedattribute). Every class that meets that criteria will automatically be registered into the application host.

For example, given the following code:

```csharp
public class MyNewCommand : Command
{
    // ...
}

[NotMapped]
public class MyIgnoredCommand : Command
{
    // ...
}
```

When the build tool is compiled and executed, `MyNewCommand` will be added to the list of commands in the build tool and `MyIgnoredCommand` will not be registered.

### Extending Configurations

The build tool uses the built-in [Host Configuration pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#application-and-host-configuration) to allow modifying configuring the tool and command's behavior. This can be used to store/share complex configurations for local development or CI/CD. Options for the build tool and commands can be configured using:

1. `buildconfig.json` file (in the root of the working directory by default)
2. Environment Variables

The [Extensions.Options.AutoBinder](https://github.com/gowon/Extensions.Options.AutoBinder#declarative-binding) package is used to support declarative registration of options when adding new commands to the build tool that you want to be configurable. Create a strongly typed settings class, and then decorate it with the `AutoBindAttribute` to have the class registered in the application host.

For example, the following options class:

```csharp
[AutoBind]
public class MyNewCommandOptions
{
    public string StringVal { get; set; }
    public int? IntVal { get; set; }
    public bool? BoolVal { get; set; }
    public DateTime? DateVal { get; set; }
}
```

would automatically be configured from the following JSON in the `buildconfig.json` file:

```json
{
  "MyNewCommand": {
    "StringVal": "Orange",
    "IntVal": 999,
    "BoolVal": true,
    "DateVal": "2020-07-11T07:43:29-4:00"
  }
}
```

or the following environment variables:

```text
MyNewCommand__StringVal=Orange
MyNewCommand__IntVal=999
MyNewCommand__BoolVal=true
MyNewCommand__DateVal=2020-07-11T07:43:29-4:00
```

> We recommend following the naming and design conventions established for the [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
