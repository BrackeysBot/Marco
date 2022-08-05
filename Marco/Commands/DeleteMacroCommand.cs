using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Marco.AutocompleteProviders;
using Marco.Services;

namespace Marco.Commands;

internal sealed class DeleteMacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public DeleteMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [SlashCommand("deletemacro", "Deletes an existing macro.", false)]
    [SlashRequireGuild]
    public async Task DeleteMacroAsync(
        InteractionContext context,
        [Option("name", "The name of the macro."), Autocomplete(typeof(MacroAutocompleteProvider))] string name
    )
    {
        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(name))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot delete macro");
            embed.WithDescription("No name was specified.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        try
        {
            await _macroService.DeleteMacroAsync(context.Guild, name).ConfigureAwait(false);

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro deleted");
            embed.WithDescription($"The macro `{name}` has been deleted.");
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().Name);
            embed.WithTitle("Cannot delete macro");
            embed.WithDescription(exception.Message);
        }

        await context.CreateResponseAsync(embed).ConfigureAwait(false);
    }
}
