using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Marco.Configuration;
using Marco.Data;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Marco.Services;

/// <summary>
///     Represents a service which listens for macros.
/// </summary>
internal sealed class MacroListeningService : BackgroundService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly DiscordClient _discordClient;
    private readonly ConfigurationService _configurationService;
    private readonly MacroService _macroService;
    private readonly MacroCooldownService _cooldownService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroListeningService" /> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="macroService">The macro service.</param>
    /// <param name="cooldownService">The macro cooldown service.</param>
    public MacroListeningService(
        DiscordClient discordClient,
        ConfigurationService configurationService,
        MacroService macroService,
        MacroCooldownService cooldownService
    )
    {
        _discordClient = discordClient;
        _configurationService = configurationService;
        _macroService = macroService;
        _cooldownService = cooldownService;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.MessageCreated += OnMessageCreated;
        return Task.CompletedTask;
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Guild is not { } guild) return;
        if (e.Author.IsBot) return;
        if (e.Message.Content is not { } content || string.IsNullOrWhiteSpace(content)) return;

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
            guildConfiguration = new GuildConfiguration();

        if (!content.StartsWith(guildConfiguration.Prefix))
            return;

        int spaceIndex = content.IndexOf(' ');

        string command = spaceIndex < 0
            ? content[guildConfiguration.Prefix.Length..]
            : content[guildConfiguration.Prefix.Length..spaceIndex];

        string? cooldownReaction = guildConfiguration.ReactionConfiguration.CooldownReaction;
        string? successReaction = guildConfiguration.ReactionConfiguration.SuccessReaction;
        string? unknownReaction = guildConfiguration.ReactionConfiguration.UnknownReaction;
        DiscordEmoji? cooldownEmoji = null;
        DiscordEmoji? successEmoji = null;
        DiscordEmoji? unknownEmoji = null;

        if (!string.IsNullOrWhiteSpace(cooldownReaction)) DiscordEmoji.TryFromName(sender, cooldownReaction, out cooldownEmoji);
        if (!string.IsNullOrWhiteSpace(successReaction)) DiscordEmoji.TryFromName(sender, successReaction, out successEmoji);
        if (!string.IsNullOrWhiteSpace(unknownReaction)) DiscordEmoji.TryFromName(sender, unknownReaction, out unknownEmoji);

        DiscordChannel channel = e.Channel;

        if (_macroService.TryGetChannelMacro(channel, command, out Macro? macro))
        {
            if (_cooldownService.IsOnCooldown(channel, command))
            {
                Logger.Info($"{e.Author} used channel macro '{command}' in {channel} but is on cooldown");

                if (cooldownEmoji is not null)
                    await e.Message.CreateReactionAsync(cooldownEmoji).ConfigureAwait(false);
            }
            else
            {
                Logger.Info($"{e.Author} used channel macro '{command}' in {channel}");

                if (successEmoji is not null)
                    await e.Message.CreateReactionAsync(successEmoji).ConfigureAwait(false);

                _cooldownService.UpdateCooldown(channel, command);
                await channel.SendMessageAsync(macro.Response).ConfigureAwait(false);
            }
        }
        else if (_macroService.TryGetGlobalMacro(guild, command, out macro))
        {
            if (_cooldownService.IsOnCooldown(channel, command))
            {
                Logger.Info($"{e.Author} used global macro '{command}' in {channel} but is on cooldown");

                if (cooldownEmoji is not null)
                    await e.Message.CreateReactionAsync(cooldownEmoji).ConfigureAwait(false);
            }
            else
            {
                Logger.Info($"{e.Author} used global macro '{command}' in {channel}");

                if (successEmoji is not null)
                    await e.Message.CreateReactionAsync(successEmoji).ConfigureAwait(false);

                _cooldownService.UpdateCooldown(channel, command);
                await channel.SendMessageAsync(macro.Response).ConfigureAwait(false);
            }
        }
        else
        {
            Logger.Info($"{e.Author} used unknown macro '{command}' in {channel}");

            if (unknownEmoji is not null)
                await e.Message.CreateReactionAsync(unknownEmoji).ConfigureAwait(false);
        }
    }
}
