namespace Boiler.QuartzWorker.RestApi.Client;

// ref: https://www.discogs.com/developers/#page:home,header:home-versioning-and-media-types
public static class MediaTypes
{
    public const string Discogs = "application/vnd.discogs.v2.discogs+json";
    public const string PlainText = "application/vnd.discogs.v2.plaintext+json";
    public const string Html = "application/vnd.discogs.v2.html+json";

}