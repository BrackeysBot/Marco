using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Marco.Data;
using Marco.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marco.AutocompleteProviders;

internal sealed class MacroAutocompleteProvider : IAutocompleteProvider
{
    /// <inheritdoc />
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
    {
        var macroService = context.Services.GetRequiredService<MacroService>();
        IReadOnlyCollection<Macro> macros = macroService.GetMacros(context.Guild);

        var result = new List<DiscordAutoCompleteChoice>();
        string optionValue = context.OptionValue.ToString() ?? string.Empty;
        bool hasOptionValue = !string.IsNullOrWhiteSpace(optionValue);

        foreach (Macro macro in macros)
        {
            if (macro.ChannelId.HasValue && macro.ChannelId.Value != context.Channel.Id)
            {
                continue;
            }

            if (hasOptionValue && !macro.Name.Contains(optionValue) && !macro.Aliases.Contains(optionValue))
            {
                continue;
            }

            result.Add(new DiscordAutoCompleteChoice(macro.Name, macro.Name));
            
            if (result.Count >= 25)
            {
                // Discord only allows 25 choices per autocomplete response
                break;
            }
        }

        result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(result);
    }
}
