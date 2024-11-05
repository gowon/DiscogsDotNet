#r "System.Text.RegularExpressions"
#load "common.csx"

using System.Text.RegularExpressions;
using static Constants;
using static Utilities;

private static void Log(string message)
{
    Console.WriteLine($"[lint-commit-msg] {message}");
}

var commitMessage = File.ReadAllText(Args[0]);
commitMessage = NormalizeNewlineFeeds(commitMessage);

if (Regex.IsMatch(commitMessage, RegexPatterns.CommitMessage, RegexOptions.Multiline))
{
    Log($"{UnicodeEmojis.Success} commit message format is valid!");
    return 0;
}

Log($"{UnicodeEmojis.Failure} commit message format is invalid!");
Log("e.g: 'feat(scope): subject' or 'fix: subject'");
Log("more info: https://www.conventionalcommits.org/en/v1.0.0/");

return 1;