﻿using DSharpPlus.Entities;
using Marco.Configuration;
using Marco.Data;

namespace Marco.Services;

internal sealed class MacroCooldownService
{
    private readonly ConfigurationService _configurationService;
    private readonly Dictionary<DiscordChannel, Dictionary<Macro, DateTimeOffset>> _lastUses = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCooldownService" /> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    public MacroCooldownService(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    ///     Returns a value indicating whether the specified macro is on cooldown.
    /// </summary>
    /// <param name="channel">The channel whose cooldown buckets to search.</param>
    /// <param name="macro">The macro whose cooldown status to retrieve.</param>
    /// <returns><see langword="true" /> if the macro is currently on cooldown; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="channel" /> or <paramref name="macro" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="channel" /> is not a guild channel.</exception>
    public bool IsOnCooldown(DiscordChannel channel, Macro macro)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(macro);

        if (channel.Guild is not { } guild) throw new ArgumentException("Channel must be in a guild.", nameof(channel));
        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
            guildConfiguration = new GuildConfiguration();

        if (guildConfiguration.Cooldown == 0) return false;

        if (!_lastUses.TryGetValue(channel, out Dictionary<Macro, DateTimeOffset>? lastUses))
            return false;

        if (!lastUses.TryGetValue(macro, out DateTimeOffset lastUse))
            return false;

        return DateTimeOffset.UtcNow < lastUse + TimeSpan.FromMilliseconds(guildConfiguration.Cooldown);
    }

    /// <summary>
    ///     Returns a value indicating whether the specified macro is on cooldown.
    /// </summary>
    /// <param name="channel">The channel whose cooldown buckets to search.</param>
    /// <param name="macro">The macro whose cooldown status to retrieve.</param>
    /// <returns><see langword="true" /> if the macro is currently on cooldown; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="channel" /> or <paramref name="macro" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="channel" /> is not a guild channel.</exception>
    public void UpdateCooldown(DiscordChannel channel, Macro macro)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(macro);

        if (channel.Guild is not { } guild) throw new ArgumentException("Channel must be in a guild.", nameof(channel));

        if (!_configurationService.TryGetGuildConfiguration(guild, out GuildConfiguration? guildConfiguration))
            guildConfiguration = new GuildConfiguration();

        if (guildConfiguration.Cooldown == 0) return;

        if (!_lastUses.TryGetValue(channel, out Dictionary<Macro, DateTimeOffset>? lastUses))
        {
            lastUses = new Dictionary<Macro, DateTimeOffset>();
            _lastUses[channel] = lastUses;
        }

        lastUses[macro] = DateTimeOffset.UtcNow;
    }
}
