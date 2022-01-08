using Bot.Modules.AutoCompleters;
using Discord;
using Discord.Interactions;
using DiscordNet.Github;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    public class SlashModule 
    {
        [Group("labs", "Work with the Labs repo.")]
        public class Labs : InteractionModuleBase<SocketInteractionContext>
        {
            public GithubRest GithubRest { get; set; }

            [SlashCommand("query", "Query pull requests on main dnet")]
            public async Task SearchAsync(
                [Summary("type", "The type of querry you want to preform.")]
                IssuePrType type,

                [Summary("query", "The query for the item to search")]
                [Autocomplete(typeof(LabsSearchAutocompleter))]
                string prNumberRaw = null,

                [Summary("label", "The label attached to the pr/issue")]
                [Autocomplete(typeof(LabsLabelAutocompleter))]
                string label = null,

                [Summary("open", "Whether or not the issue/pr is open.")]
                bool isOpen = true)
            {
                var prNumber = 1;
                if (prNumberRaw != null && !int.TryParse(prNumberRaw, out prNumber))
                {
                    await RespondAsync("Invalid PR/Issue");
                }

                await DeferAsync();

                string openQuery = isOpen ? "state:open" : "state:closed";
                string labelQuery = label != null ? $"label:\"{label}\"" : "";
                string prIssueQuery = type switch
                {
                    IssuePrType.Issue => "is:issue",
                    IssuePrType.Pr => "is:pr",
                    IssuePrType.Both => "",
                    _ => ""
                };

                string FormulateUrl(IssueSearchResultItem item)
                {
                    string postfix = item.PullRequest == null ? "issue" : "pull";

                    return $"https://github.com/Discord-Net-Labs/Discord.Net-Labs/{postfix}/{item.Number}";
                }

                if (prNumberRaw == null)
                {
                    var compiledQuery = string.Join('+', new string[] { openQuery, labelQuery, prIssueQuery }.Where(x => !string.IsNullOrEmpty(x)));
                    var search = $"repo:Discord-Net-Labs/Discord.Net-Labs{(compiledQuery != null ? $"+{compiledQuery}" : "")}";
                    var result = await GithubRest.SearchRawAsync("issues", search);

                    var embed = new EmbedBuilder()
                        .WithTitle("Search result")
                        .WithDescription($"Showing {(result.Count > 25 ? "25" : $"{result.Count}")}/{result.Count} results")
                        .WithColor(Color.Green);

                    foreach (var item in result.Take(25))
                    {
                        embed.AddField(
                            "​",
                            $"**[{item.Title}]({FormulateUrl(item)})**\nNumber: {item.Number}\nType: {(item.PullRequest != null ? "PR" : "Issue")}\nState: {item.State}\nAuthor: {item.User.Login}{(item.Labels?.Any() ?? false ? $"\nLabels:\n```{string.Join("\n", item.Labels.Select(x => x.Name))}```" : "")}",
                            true);
                    }

                    await FollowupAsync(embed: embed.Build());
                }
                else
                {
                    var result = await GithubRest.GetRawPullRequestAsync("Discord-Net-Labs/Discord.Net-Labs", prNumber);

                    if (result == null)
                    {
                        await FollowupAsync("No result found.");
                        return;
                    }

                    var embed = new EmbedBuilder()
                        .WithTitle("Search result")
                        .WithDescription($"Here's the result for #{prNumberRaw}")
                        .WithColor(Color.Green)
                        .AddField("Author", result.User.Login)
                        .AddField("Link", result.HtmlUrl)
                        .AddField("State", result.State);

                    if (result.Labels?.Any() ?? false)
                    {
                        embed.AddField("Labels", $"```{string.Join("\n", result.Labels.Select(x => x.Name))}```");
                    }

                    await FollowupAsync(embed: embed.Build());
                }
            }
        }

        [Group("dnet", "Work with the main dnet repo.")]
        public class Main : InteractionModuleBase<SocketInteractionContext>
        {
            public GithubRest GithubRest { get; set; }

            [SlashCommand("query", "Query pull requests on main dnet")]
            public async Task SearchAsync(
                [Summary("type", "The type of querry you want to preform.")]
                IssuePrType type,
                
                [Summary("query", "The query for the item to search")]
                [Autocomplete(typeof(MainSearchAutocompleter))]
                string prNumberRaw = null,
                
                [Summary("label", "The label attached to the pr/issue")]
                [Autocomplete(typeof(MainLabelAutocompleter))]
                string label = null,

                [Summary("open", "Whether or not the issue/pr is open.")]
                bool isOpen = true)
            {
                var prNumber = 1;
                if(prNumberRaw != null && !int.TryParse(prNumberRaw, out prNumber))
                {
                    await RespondAsync("Invalid PR/Issue");
                }

                await DeferAsync();

                string openQuery = isOpen ? "state:open" : "state:closed";
                string labelQuery = label != null ? $"label:\"{label}\"" : "";
                string prIssueQuery = type switch
                {
                    IssuePrType.Issue => "is:issue",
                    IssuePrType.Pr => "is:pr",
                    IssuePrType.Both => "",
                    _ => ""
                };

                string FormulateUrl(IssueSearchResultItem item)
                {
                    string postfix = item.PullRequest == null ? "issues" : "pull";

                    return $"https://github.com/discord-net/Discord.Net/{postfix}/{item.Number}";
                }

                if (prNumberRaw == null)
                {
                    var compiledQuery = string.Join('+', new string[] { openQuery, labelQuery, prIssueQuery }.Where(x => !string.IsNullOrEmpty(x)));
                    var search = $"repo:discord-net/Discord.Net{(compiledQuery != null ? $"+{compiledQuery}" : "")}";
                    var result = await GithubRest.SearchRawAsync("issues", search);

                    var embed = new EmbedBuilder()
                        .WithTitle("Search result")
                        .WithDescription($"Showing {(result.Count > 25 ? "25" : $"{result.Count}")}/{result.Count} results")
                        .WithColor(Color.Green);

                    foreach (var item in result.Take(25))
                    {
                        embed.AddField(
                            "​",
                            $"**[{item.Title}]({FormulateUrl(item)})**\nNumber: {item.Number}\nType: {(item.PullRequest != null ? "PR" : "Issue")}\nState: {item.State}\nAuthor: {item.User.Login}{(item.Labels?.Any() ?? false ? $"\nLabels:\n```{string.Join("\n", item.Labels.Select(x => x.Name))}```" : "")}",
                            true);
                    }

                    await FollowupAsync(embed: embed.Build());
                }
                else
                {
                    var result = await GithubRest.GetRawPullRequestAsync("discord-net/Discord.Net", prNumber);

                    if(result == null)
                    {
                        await FollowupAsync("No result found.");
                        return;
                    }

                    var embed = new EmbedBuilder()
                        .WithTitle("Search result")
                        .WithDescription($"Here's the result for #{prNumberRaw}")
                        .WithColor(Color.Green)
                        .AddField("Author", result.User.Login)
                        .AddField("Link", result.HtmlUrl)
                        .AddField("State", result.State);

                    if(result.Labels?.Any() ?? false)
                    {
                        embed.AddField("Labels", $"```{string.Join("\n", result.Labels.Select(x => x.Name))}```");
                    }

                    await FollowupAsync(embed: embed.Build());
                }
            }
        }
    }
}
