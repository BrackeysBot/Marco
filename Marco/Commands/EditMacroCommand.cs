using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Interactivity;
using Marco.Services;

namespace Marco.Commands;

internal sealed class EditMacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public EditMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [SlashCommand("editmacro", "Edits an existing macro.", false)]
    [SlashRequireGuild]
    public async Task EditMacroAsync(
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

        if (!_macroService.TryGetMacro(context.Guild, name, out Macro? macro))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot delete macro");
            embed.WithDescription($"No macro with the name `{name}` was found.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        var modal = new DiscordModalBuilder(context.Client);
        modal.WithTitle($"Add macro '{name}'");

        DiscordChannel? channel = null;
        DiscordModalTextInput channelInput =
            modal.AddInput("Channel ID", initialValue: macro.ChannelId?.ToString(), isRequired: false);
        DiscordModalTextInput responseInput =
            modal.AddInput("Response", initialValue: macro.Response, inputStyle: TextInputStyle.Paragraph);

        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        if (response != DiscordModalResponse.Success)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Macro not added");
            embed.WithDescription($"The macro '{name}' was not added because the response timed out.");
            await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)).ConfigureAwait(false);
            return;
        }

        try
        {
            await _macroService.EditMacroAsync(context.Guild, name, m =>
            {
                m.Response = responseInput.Value!;

                if (string.IsNullOrWhiteSpace(channelInput.Value) || !ulong.TryParse(channelInput.Value, out ulong channelId))
                    m.ChannelId = null;
                else if (context.Guild.Channels.TryGetValue(channelId, out channel))
                    m.ChannelId = channel?.Id;
            }).ConfigureAwait(false);

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro edited");
            embed.WithDescription($"The macro `{name}` has been edited.");
            embed.AddField("Name", macro.Name, true);
            embed.AddField("Type", channel?.Mention ?? "Global", true);
            embed.AddField("Response", responseInput.Value);
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().Name);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription(exception.Message);
        }

        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)).ConfigureAwait(false);
    }
}
