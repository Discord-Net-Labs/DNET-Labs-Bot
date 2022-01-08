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
    public class LabsLabelAutocompleter : BaseLabelAutocompleter
    {
        public LabsLabelAutocompleter() : base("Discord-Net-Labs/Discord.Net-Labs") { }
    }

    public class MainLabelAutocompleter : BaseLabelAutocompleter
    {
        public MainLabelAutocompleter() : base("discord-net/Discord.Net") { }
    }

    public abstract class BaseLabelAutocompleter : AutocompleteHandler
    {
        private string _repo;

        private DateTimeOffset _lastFetched = DateTime.MinValue;

        private List<string> _labels;

        public BaseLabelAutocompleter(string repo)
        {
            _repo = repo;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var query = (string)autocompleteInteraction.Data.Current.Value;

            var labels = await GetLabels(query, services);

            return AutocompletionResult.FromSuccess(labels.Take(20).Select(x => new AutocompleteResult(x, x)));
        }

        public async ValueTask<IOrderedEnumerable<string>> GetLabels(string query, IServiceProvider services)
        {
            if((DateTimeOffset.UtcNow - _lastFetched).TotalMinutes > 10)
            {
                var rest = services.GetRequiredService<GithubRest>();

                var labels = await rest.GetLabelsAsync(_repo);

                _labels = labels.Select(x => x.Name).ToList();

                _lastFetched = DateTime.UtcNow;
            }

            if (query == null)
                return _labels.OrderBy(x => x);

            return _labels.OrderBy(x => Levenshtein.Compute(x, query));
        }
    }
}
