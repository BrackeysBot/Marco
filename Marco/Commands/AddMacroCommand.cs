using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Marco.Data;
using Marco.Interactivity;
using Marco.Services;

namespace Marco.Commands;

internal sealed class AddMacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public AddMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [SlashCommand("addmacro", "Adds a new macro.", false)]
    [SlashRequireGuild]
    public async Task AddMacro(
        InteractionContext context,
        [Option("name", "The name of the macro.")] string name,
        [Option("channel", "The channel to which the macro will be limited.")] DiscordChannel? channel = null
    )
    {
        var embed = new DiscordEmbedBuilder();

        DiscordGuild guild = context.Guild;

        if (channel is not null && channel.Guild != guild)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription("The channel is not in this guild.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription("No name was specified.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        name = Regex.Replace(name.ToLowerInvariant(), "\\s", string.Empty, RegexOptions.Compiled);

        var modal = new DiscordModalBuilder(context.Client);
        modal.WithTitle($"Add macro '{name}'");
        DiscordModalTextInput responseInput = modal.AddInput("Response", inputStyle: TextInputStyle.Paragraph);
        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        if (response == DiscordModalResponse.Success)
        {
            Macro macro = await _macroService.CreateMacroAsync(guild, channel, name, responseInput.Value!).ConfigureAwait(false);
            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro added");
            embed.WithDescription($"The macro `{macro.Name}` has been added.");
            embed.AddField("Response", macro.Response);
        }
        else
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Macro not added");
            embed.WithDescription($"The macro '{name}' was not added because the response timed out.");
        }

        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)).ConfigureAwait(false);
    }
}
