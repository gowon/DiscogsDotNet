#r "System.Linq"
#r "System.Text.RegularExpressions"
#load "common.csx"

using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using static Constants;

private static void Log(string message)
{
    Console.WriteLine($"[lint-branch-name] {message}");
}

var branchName = Args[0];
var match = Regex.Match(branchName, RegexPatterns.BranchName, RegexOptions.IgnoreCase);

if (!match.Success)
{
    Log($"{UnicodeEmojis.Failure} branch name is invalid!");
    Log("Please review the docs for more information about the branch naming conventions.");
    return 1;
}

switch (match.Groups["type"].Value)
{
    case "feature":
    case "bugfix":
    case "hotfix":
    case "wip":
        if (match.Groups["desc"].Value.Length < 4)
        {
            Log($"{UnicodeEmojis.Failure} branch name is invalid!");
            Log("The 'description' segment of the branch name must be at least 4 characters long.");
            Log("Please review the docs for more information about the branch naming conventions.");
            return 1;
        }
        break;
}

Log($"{UnicodeEmojis.Success} branch name is valid!");
return 0;