using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Marco.Data;
using Marco.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marco.AutocompleteProviders;

internal sealed class MacroAutocompleteProvider : IAutocompleteProvider
{
    /// <inheritdoc />
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        var macroService = ctx.Services.GetRequiredService<MacroService>();
        IReadOnlyCollection<Macro> macros = macroService.GetMacros(ctx.Guild);
        return Task.FromResult(macros.Select(macro => new DiscordAutoCompleteChoice(macro.Name, macro.Name)));
    }
}
