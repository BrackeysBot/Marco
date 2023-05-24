using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Services;

namespace Marco.Commands;

internal sealed class MacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public MacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [SlashCommand("macro", "Executes a macro.")]
    [SlashRequireGuild]
    public async Task MacroAsync(InteractionContext context,
        [Option("macro", "The name of the macro.", true)] [Autocomplete(typeof(MacroAutocompleteProvider))] string macroName)
    {
        if (!_macroService.TryGetMacro(context.Guild, macroName, out Macro? macro))
        {
            await context.CreateResponseAsync($"The macro `{macroName}` doesn't exist.", true).ConfigureAwait(false);
            return;
        }

        if (macro.ChannelId.HasValue && macro.ChannelId.Value != context.Channel.Id)
        {
            await context.CreateResponseAsync($"The macro `{macroName}` cannot be executed here.", true).ConfigureAwait(false);
            return;
        }

        await context.CreateResponseAsync(macro.Response).ConfigureAwait(false);
    }
}
