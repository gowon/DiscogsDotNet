namespace SampleClientService;

using System.IO.Abstractions;
using System.Net.Http.Headers;
using DiscogsDotNet.V2;
using Extensions.Options.AutoBinder;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Layouts.ClefJsonLayout;
using Testably.Abstractions;

public class Program
{
    public static void Main(string[] args)
    {
        var bootstrapLogger = LogManager.Setup().LoadConfiguration(builder =>
            builder.ForLogger().WriteToColoredConsole(new CompactJsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("@SourceContext", Layout.FromString("${logger}"))
                }
            })).GetCurrentClassLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            #region Logging

            builder.Logging.ClearProviders();

            LogManager.Setup().LoadConfigurationFromSection(builder.Configuration);
            LogManager.ReconfigExistingLoggers();
            builder.Logging.AddNLog();

            #endregion Logging

            builder.Services.AddResilienceEnricher();

            // ref: https://github.com/Testably/Testably.Abstractions?tab=readme-ov-file#getting-started
            builder.Services
                .AddSingleton<IFileSystem, RealFileSystem>()
                .AddSingleton<IRandomSystem, RealRandomSystem>()
                .AddSingleton<ITimeSystem, RealTimeSystem>();

            builder.Services.AddOptions<DiscogsClientV2Options>().AutoBind().ValidateOnStart();
            builder.Services.AddSingleton<DiscogsPersonalAccessTokenAuthenticationHandler>();
            builder.Services.AddSingleton<DiscogsUserAgentHandler>();
            builder.Services.AddSingleton(new RateLimitingRequestHandler(60, TimeSpan.FromSeconds(1), true));

            builder.Services.AddHttpClient<IDiscogsClientV2, DiscogsClientV2>((provider, client) =>
                {
                    var options = provider.GetRequiredService<DiscogsClientV2Options>();
                    client.BaseAddress = new Uri(options.BasePath);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(DiscogsMediaTypes.Json));
                })
                .AddHttpMessageHandler<DiscogsPersonalAccessTokenAuthenticationHandler>()
                .AddHttpMessageHandler<DiscogsUserAgentHandler>()
                .AddHttpMessageHandler<RateLimitingRequestHandler>()
                .AddStandardResilienceHandler();

            builder.Services.AddOptions<WorkerOptions>().AutoBind();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
        catch (Exception exception)
        {
            bootstrapLogger.Fatal(exception, "Application terminated unexpectedly.");
            Environment.Exit(1);
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }
}