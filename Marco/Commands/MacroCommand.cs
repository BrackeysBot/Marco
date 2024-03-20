using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Services;

namespace Marco.Commands;

internal sealed class MacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;
    private readonly MacroCooldownService _cooldownService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    /// <param name="cooldownService">The cooldown service.</param>
    public MacroCommand(MacroService macroService, MacroCooldownService cooldownService)
    {
        _macroService = macroService;
        _cooldownService = cooldownService;
    }

    [SlashCommand("macro", "Executes a macro.")]
    [SlashRequireGuild]
    public async Task MacroAsync(InteractionContext context,
        [Option("macro", "The name of the macro.", true)] [Autocomplete(typeof(MacroAutocompleteProvider))]
        string macroName)
    {
        if (!_macroService.TryGetMacro(context.Guild, macroName, out Macro? macro))
        {
            await context.CreateResponseAsync($"The macro `{macroName}` doesn't exist.", true).ConfigureAwait(false);
            return;
        }

        DiscordChannel channel = context.Channel;

        if (macro.ChannelId.HasValue && macro.ChannelId.Value != channel.Id)
        {
            await context.CreateResponseAsync($"The macro `{macroName}` cannot be executed here.", true).ConfigureAwait(false);
            return;
        }
        if (_cooldownService.IsOnCooldown(channel, macro))
        {
            await context.CreateResponseAsync($"The macro `{macroName}` is on cooldown because it was very recently executed.", true).ConfigureAwait(false);
            return;
        }

        var builder = new DiscordInteractionResponseBuilder();
        string response = macro.Response;

        if (response.StartsWith("@silent "))
        {
            response = response[8..];
            builder.SuppressNotifications();
        }

        builder.WithContent(response);
        await context.CreateResponseAsync(builder).ConfigureAwait(false);
        _cooldownService.UpdateCooldown(channel, macro);
    }
}
