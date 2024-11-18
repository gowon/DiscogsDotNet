namespace DiscogsDotNet.V2;

public class DiscogsClientV2Options
{
    public string BasePath { get; set; } = "https://api.discogs.com";
    public string? PersonalAccessToken { get; set; }
    public string UserAgent { get; set; }
    public string UserAgentVersion { get; set; } = "1.0";
    public string? UserAgentUri { get; set; }
}
