namespace SampleClientService;

using DiscogsDotNet.V2;
using Extensions.Options.AutoBinder;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Layouts.ClefJsonLayout;

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

            builder.Services.AddOptions<DiscogsClientV2Options>().AutoBind();
            builder.Services.AddSingleton<DiscogsAuthenticationHandler>();

            builder.Services.AddHttpClient<IDiscogsClientV2, DiscogsClientV2>((provider, client) =>
            {
                var options = provider.GetRequiredService<DiscogsClientV2Options>();
                client.BaseAddress = new Uri(options.BasePath);
            })
                .AddHttpMessageHandler<DiscogsAuthenticationHandler>()
                .AddStandardResilienceHandler();

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