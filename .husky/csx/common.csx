#r "System.Text.RegularExpressions"

using System.Text.RegularExpressions;

public static class Constants
{
    public static class RegexPatterns
    {
        // ref: https://regex101.com/r/ZgMPsl/2
        // language=regex
        public static readonly string CommitMessage = @"\A(?:(?<type>[^\(!:]+)(?:\((?<scope>[^\)]+)\))?(?<breaking>!)?: (?<description>.+))(?:(?!(?:\n{2}^(?:BREAKING[ -]CHANGE|[a-zA-Z0-9_\-]+)(?:: | #\b)[^\n:]+(\n*\b)?)+)\n{2}(?<body>(?:(?!\n{2}(?:^(?:BREAKING[ -]CHANGE|[a-zA-Z0-9_\-]+)(?:: | #\b)[^\n:]+(\n*\b)?)+).|\n+?)+?))?(?:\n{2}(?<footers>(?:^(?:BREAKING[ -]CHANGE|[a-zA-Z0-9_\-]+)(?:: | #\b)[^\n:]+(\n*\b)?)+))?(?:\n*)\Z";

        // ref: https://regex101.com/r/QU2nvM/1
        // language=regex
        public static readonly string BranchName = @"^(?:(?<type>main|staging|dev(?:elop)?)|(?<type>release)\/(?<version>[0-9]+(?:\.[0-9]+){2}(?:-[a-zA-Z0-9]+)?)|(?<type>wip)\/(?<desc>[a-zA-Z0-9]+(?:[-][a-zA-Z0-9]+)*)|(?<type>feature|(?:bug|hot)fix)\/(?<issue>[0-9]+|no-ref){1}\/(?<desc>[a-zA-Z0-9]+(?:[-][a-zA-Z0-9]+)*))$";
    }

    public static class UnicodeEmojis
    {
        public static readonly string Success = "\u2714"; // check_mark = ✔
        public static readonly string Failure = "\u274c"; // cross_mark = ❌
        public static readonly string Information = "\u2139"; // information = ℹ
        public static readonly string Warning = "\u26a0"; // warning = ⚠
    }
}

public static class Utilities
{
    public static string NormalizeNewlineFeeds(string message)
    {
        return Regex.Replace(message, @"\r\n|\r|\n", "\n");
    }
}