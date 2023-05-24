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

        if (_macroService.TryGetMacro(guild, name, out _))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription($"A macro with the name or alias `{name}` already exists.");
            await context.CreateResponseAsync(embed, true).ConfigureAwait(false);
            return;
        }

        var modal = new DiscordModalBuilder(context.Client);
        modal.WithTitle($"Add macro '{name}'");
        DiscordModalTextInput aliasesInput =
            modal.AddInput("Aliases (space-separated)", placeholder: "e.g. null nullreference nullref", isRequired: false);
        DiscordModalTextInput responseInput = modal.AddInput("Response", inputStyle: TextInputStyle.Paragraph);
        DiscordModalResponse response =
            await modal.Build().RespondToAsync(context.Interaction, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        if (response != DiscordModalResponse.Success)
            return;

        var aliases = new List<string>(aliasesInput.Value?.Split() ?? ArraySegment<string>.Empty);
        var invalidAliases = new List<string>();

        if (aliases.Count > 0)
        {
            for (int index = aliases.Count - 1; index >= 0; index--)
            {
                string current = aliases[index];
                if (_macroService.TryGetMacro(guild, current, out _))
                {
                    aliases.RemoveAt(index);
                    invalidAliases.Add(current);
                }
            }
        }

        Macro macro = await _macroService
            .CreateMacroAsync(guild, null, name, responseInput.Value!, aliases.ToArray()).ConfigureAwait(false);
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Macro added");
        embed.WithDescription($"The macro `{macro.Name}` has been added.");
        if (invalidAliases.Count > 0)
        {
            embed.Description += "\n\n⚠️ The following aliases were not added because they already exist: " +
                                 string.Join(", ", invalidAliases.Select(a => $"`{a}`"));
        }

        embed.AddField("Type", "Global", true);
        embed.AddFieldIf(macro.Aliases.Count > 0, "Alias".ToQuantity(macro.Aliases.Count), string.Join(' ', macro.Aliases), true);

        string value = macro.Response;
        if (value is {Length: > 1024})
            value = $"{value[..1021]}...";
        embed.AddField("Response", value);

        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)).ConfigureAwait(false);
    }
}
