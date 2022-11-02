using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Humanizer;
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
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription("No name was specified.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        if (!_macroService.TryGetMacro(context.Guild, name, out Macro? macro))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription($"No macro with the name `{name}` was found.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        var modal = new DiscordModalBuilder(context.Client);
        modal.WithTitle($"Edit macro '{name}'");

        DiscordChannel? channel = null;
        DiscordModalTextInput aliasesInput =
            modal.AddInput("Aliases (space-separated)", placeholder: "e.g. null nullreference nullref", isRequired: false);
        DiscordModalTextInput responseInput =
            modal.AddInput("Response", initialValue: macro.Response, inputStyle: TextInputStyle.Paragraph);

        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        if (response != DiscordModalResponse.Success)
            return;

        try
        {
            macro = await _macroService.EditMacroAsync(context.Guild, name, m =>
            {
                m.Aliases = new List<string>(aliasesInput.Value?.Split() ?? ArraySegment<string>.Empty);
                m.Response = responseInput.Value!;
            }).ConfigureAwait(false);

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro edited");
            embed.WithDescription($"The macro `{name}` has been edited.");
            embed.AddField("Name", macro.Name, true);
            embed.AddField("Type", channel?.Mention ?? "Global", true);
            embed.AddField("Alias".ToQuantity(macro.Aliases.Count), string.Join(' ', macro.Aliases), true);
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
