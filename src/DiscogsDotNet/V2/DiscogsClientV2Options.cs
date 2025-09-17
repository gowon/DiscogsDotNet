namespace DiscogsDotNet.V2;

using System.ComponentModel.DataAnnotations;

public class DiscogsClientV2Options
{
    public string BasePath { get; set; } = "https://api.discogs.com";

    [Required]
    public string PersonalAccessToken { get; set; }

    [Required]
    public string UserAgent { get; set; }

    [Required]
    public string UserAgentVersion { get; set; }

    [Required]
    public string? UserAgentUri { get; set; }
}