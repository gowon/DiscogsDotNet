namespace SampleClientService;

using System.IO.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using DiscogsDotNet.V2;
using Extensions.Options.AutoBinder;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Layouts.ClefJsonLayout;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
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

            builder.Services.AddOptions<DiscogsClientV2Options>().AutoBind();
            builder.Services.AddSingleton<DiscogsPersonalAccessTokenAuthenticationHandler>();
            builder.Services.AddSingleton<DiscogsUserAgentHandler>();

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
                .AddResilienceHandler("discogs", (pipeline, context) =>
                {
                    pipeline.ConfigureTelemetry(context.ServiceProvider.GetRequiredService<ILoggerFactory>());

                    // todo: the rate limiter and retry are not working as expected

                    // ref: https://www.discogs.com/developers#page:home,header:home-rate-limiting
                    pipeline.AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        SegmentsPerWindow = 4,
                        Window = TimeSpan.FromMinutes(1)
                    }));


                    pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                    {
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                            .Handle<HttpRequestException>()
                            .Handle<RateLimiterRejectedException>()
                            .HandleResult(message => message.StatusCode == HttpStatusCode.TooManyRequests),
                        DelayGenerator = delayArgs =>
                        {
                            if (delayArgs.Outcome.Exception is RateLimiterRejectedException
                                {
                                    RetryAfter: not null
                                } rejectedException)
                            {
                                return new ValueTask<TimeSpan?>(rejectedException.RetryAfter.Value);
                            }

                            return new ValueTask<TimeSpan?>(delayArgs.Outcome.Result?.Headers.RetryAfter?.Delta ??
                                                            TimeSpan.FromSeconds(4));
                        },
                        MaxRetryAttempts = 3
                    });
                });

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