using Discord;
using Discord.Interactions;
using DiscordNet.Github;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules.AutoCompleters
{
    public abstract class BaseSearchAutocompleter : AutocompleteHandler
    {
        private string _repo { get; set; }

        public BaseSearchAutocompleter(string repo) : base()
        {
            _repo = repo;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var isOpen = (bool?)autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "open")?.Value ?? true;
            var label = autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "label")?.Value as string;
            string currentQuery = (string)autocompleteInteraction.Data.Current.Value;
            bool isNumber = int.TryParse(currentQuery, out var _);
            bool? pr = Enum.Parse<IssuePrType>(autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "type").Value as string) switch
            {
                IssuePrType.Issue => false,
                IssuePrType.Pr => true,
                IssuePrType.Both => null
            };

            string prIssueFilter = pr.HasValue ? (pr.Value ? "is:pr+" : "is:issue+") : "";

            var ghClient = services.GetRequiredService<GithubRest>();

            var result = await ghClient.SearchRawAsync("issues", $"{(string.IsNullOrEmpty(currentQuery) ? "" : (isNumber ? $"{currentQuery}+" : $"{currentQuery}+in:title+"))}{prIssueFilter}repo:{_repo}+state:{(isOpen ? "open" : "closed")}+{(label != null ? $"label:\"{label}\"+" : "")}").ConfigureAwait(false);

            if (result.Any())
            {
                return AutocompletionResult.FromSuccess(result.Take(20).Select(x => new AutocompleteResult($"#{x.Number} {x.Title}".Length > 100 ? new string($"#{x.Number} {x.Title}".Take(97).ToArray()) + "..." : $"#{x.Number} {x.Title}", $"{x.Number}")));
            }
            else
            {
                return AutocompletionResult.FromSuccess();
            }
        }
    }
}
