using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Humanizer;
using Marco.Data;
using Marco.Services;

namespace Marco.Commands;

internal sealed class ListMacrosCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListMacrosCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public ListMacrosCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [SlashCommand("listmacros", "List all available macros.")]
    [SlashRequireGuild]
    public async Task ListMacrosAsync(InteractionContext context)
    {
        IReadOnlyCollection<Macro> macros = _macroService.GetMacros(context.Guild).Distinct().ToArray();
        var embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.CornflowerBlue);
        embed.WithTitle($"{"macro".ToQuantity(macros.Count)} available");

        if (macros.Count > 0)
        {
            foreach (IGrouping<ulong?, Macro>[] groupings in macros.GroupBy(m => m.ChannelId).Chunk(20))
            foreach (IGrouping<ulong?, Macro> grouping in groupings)
            {
                var names = new List<string>();
                string type = grouping.Key.HasValue
                    ? (await context.Client.GetChannelAsync(grouping.Key.Value).ConfigureAwait(false)).Name
                    : "Global";

                foreach (Macro macro in grouping.OrderBy(m => m.Name))
                    names.Add(macro.Name);

                embed.AddField(type, Formatter.BlockCode(string.Join('\n', names)), true);
            }
        }

        await context.CreateResponseAsync(embed).ConfigureAwait(false);
    }
}
