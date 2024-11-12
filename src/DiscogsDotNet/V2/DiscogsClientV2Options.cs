namespace DiscogsDotNet.V2;

public class DiscogsClientV2Options
{
    public string BasePath { get; set; } = "https://api.discogs.com";

    public string? PersonalAccessToken { get; set; }
}
