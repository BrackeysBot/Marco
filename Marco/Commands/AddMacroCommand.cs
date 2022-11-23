using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Humanizer;
using Marco.Data;
using Marco.Interactivity;
using Marco.Services;
using X10D.DSharpPlus;

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
        [Option("name", "The name of the macro.")] string name
    )
    {
        var embed = new DiscordEmbedBuilder();

        DiscordGuild guild = context.Guild;

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
        DiscordModalTextInput aliasesInput =
            modal.AddInput("Aliases (space-separated)", placeholder: "e.g. null nullreference nullref", isRequired: false);
        DiscordModalTextInput responseInput = modal.AddInput("Response", inputStyle: TextInputStyle.Paragraph);
        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        if (response != DiscordModalResponse.Success)
            return;

        Macro macro = await _macroService
            .CreateMacroAsync(guild, null, name, responseInput.Value!, aliasesInput.Value?.Split()).ConfigureAwait(false);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Macro added");
        embed.WithDescription($"The macro `{macro.Name}` has been added.");
        embed.AddField("Type", "Global", true);
        embed.AddFieldIf(macro.Aliases.Count > 0, "Alias".ToQuantity(macro.Aliases.Count), string.Join(' ', macro.Aliases), true);
        embed.AddField("Response", macro.Response);
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)).ConfigureAwait(false);
    }
}
