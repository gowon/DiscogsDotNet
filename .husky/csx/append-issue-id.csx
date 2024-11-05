#r "System.Linq"
#r "System.Text.RegularExpressions"
#load "common.csx"

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Constants;
using static Utilities;

private static void Log(string message)
{
    Console.WriteLine($"[append-issue-id] {message}");
}

var branchName = Args[0];
var branchMatch = Regex.Match(branchName, RegexPatterns.BranchName, RegexOptions.IgnoreCase);
var issue = branchMatch.Groups["issue"];
var commitMessage = File.ReadAllText(Args[1]);
commitMessage = NormalizeNewlineFeeds(commitMessage);
var commitMatch = Regex.Match(commitMessage, RegexPatterns.CommitMessage, RegexOptions.Multiline);
var footers = commitMatch.Groups["footers"];

// skip if no issue ID detected
if (!issue.Success || issue.Value.Equals("no-ref"))
{
    Log($"{UnicodeEmojis.Information} No issue ID detected in branch name.");
    return 0;
}

// does the commit message have a footer
if (footers.Success)
{
    // does the footer already contain a reference to the issue
    if (footers.Value.Contains(issue.Value))
    {
        Log($"{UnicodeEmojis.Information} Issue #{issue.Value} detected in commit message.");
        return 0;
    }

    File.AppendAllText(Args[1], Environment.NewLine + $"Refs: #{issue.Value}");
}
else
{
    File.AppendAllText(Args[1], Environment.NewLine + Environment.NewLine + $"Refs: #{issue.Value}");
}

Log($"{UnicodeEmojis.Success} Appended reference for issue #{issue.Value} to commit message!");
return 0;