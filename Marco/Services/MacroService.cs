using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Marco.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using X10D.Collections;

namespace Marco.Services;

/// <summary>
///     Represents a service which manages macros.
/// </summary>
internal sealed class MacroService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordClient _discordClient;
    private readonly Dictionary<DiscordGuild, Dictionary<string, Macro>> _macros = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroService" /> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory.</param>
    /// <param name="discordClient">The Discord client.</param>
    public MacroService(IServiceScopeFactory scopeFactory, DiscordClient discordClient)
    {
        _scopeFactory = scopeFactory;
        _discordClient = discordClient;
    }

    /// <summary>
    ///     Creates a new macro.
    /// </summary>
    /// <param name="guild">The guild in which the macro will be created.</param>
    /// <param name="channel">
    ///     The channel to which the macro will be restricted, or <see langword="null" /> to create a global macro.
    /// </param>
    /// <param name="name">The name of the macro.</param>
    /// <param name="response">The bot response string.</param>
    /// <param name="aliases">The aliases</param>
    /// <returns>The newly-created macro.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" />, <paramref name="name" />, or <paramref name="response" />, is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="name" />, or <paramref name="response" />, is empty or consists only of whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">A macro with the specified name already exists.</exception>
    public async Task<Macro> CreateMacroAsync(
        DiscordGuild guild,
        DiscordChannel? channel,
        string name,
        string response,
        params string[]? aliases
    )
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(response);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty or whitespace", nameof(name));

        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty or whitespace", nameof(response));

        if (TryGetMacro(guild, name, out _))
            throw new InvalidOperationException("Macro already exists");

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<MarcoContext>();

        var macro = new Macro
        {
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            Aliases = new List<string>((aliases ?? ArraySegment<string>.Empty).WhereNot(string.IsNullOrWhiteSpace)),
            GuildId = guild.Id,
            ChannelId = channel?.Id,
            Name = Regex.Replace(name.ToLowerInvariant(), "\\s", string.Empty, RegexOptions.Compiled),
            Response = response
        };

        EntityEntry<Macro> entry = await context.Macros.AddAsync(macro).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        macro = entry.Entity;

        if (!_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros))
        {
            macros = new Dictionary<string, Macro>();
            _macros.Add(guild, macros);
        }

        macros[macro.Name] = macro;
        return macro;
    }

    /// <summary>
    ///     Deletes a guild macro by its name.
    /// </summary>
    /// <param name="guild">The guild whose macros to modify.</param>
    /// <param name="name">The name of the macro to remove.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" /> or <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public async Task DeleteMacroAsync(DiscordGuild guild, string name)
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(name);

        if (!TryGetMacro(guild, name, out Macro? macro))
            return;

        if (_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros))
            macros.Remove(macro.Name);

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<MarcoContext>();
        context.Remove(macro);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Edits a guild macro.
    /// </summary>
    /// <param name="guild">The guild whose macros to modify.</param>
    /// <param name="name">The name of the macro to modify.</param>
    /// <param name="action">A function which defines the modification model for the macro.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" /> or <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public async Task<Macro> EditMacroAsync(DiscordGuild guild, string name, Action<Macro> action)
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(action);

        if (!TryGetMacro(guild, name, out Macro? macro))
            throw new InvalidOperationException("Macro does not exist");

        action(macro);
        macro.Aliases = new List<string>(macro.Aliases?.WhereNot(string.IsNullOrWhiteSpace) ?? ArraySegment<string>.Empty);

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<MarcoContext>();
        context.Update(macro);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return macro;
    }

    /// <summary>
    ///     Gets all macros defined for a guild.
    /// </summary>
    /// <param name="guild">The guild whose macros to retrieve.</param>
    /// <returns>A read-only view of the macros defined for the specified guild.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="guild" /> is <see langword="null" />.</exception>
    public IReadOnlyCollection<Macro> GetMacros(DiscordGuild guild)
    {
        ArgumentNullException.ThrowIfNull(guild);
        return _macros.TryGetValue(guild, out Dictionary<string, Macro>? macros) ? macros.Values : ArraySegment<Macro>.Empty;
    }

    /// <summary>
    ///     Gets all macros defined for a channel.
    /// </summary>
    /// <param name="channel">The channel whose macros to retrieve.</param>
    /// <returns>A read-only view of the macros defined for the specified channel.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="channel" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="channel" /> is not a guild channel.</exception>
    public IReadOnlyCollection<Macro> GetMacros(DiscordChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        if (channel.Guild is not { } guild) throw new ArgumentException("Channel must be in a guild", nameof(channel));
        return _macros.TryGetValue(guild, out Dictionary<string, Macro>? macros)
            ? macros.Values.Where(m => m.ChannelId == channel.Id).ToArray()
            : ArraySegment<Macro>.Empty;
    }

    /// <summary>
    ///     Attempts to find a channel macro by its name, and returns a value indicating the success of the operation. 
    /// </summary>
    /// <param name="channel">The channel whose macros to search.</param>
    /// <param name="name">The name of the macro to find.</param>
    /// <param name="macro">
    ///     When this method returns, contains the macro whose name is equal to <paramref name="name" /> and whose
    ///     <see cref="Macro.ChannelId" /> is the ID of <paramref name="channel" />, or <see langword="null" /> if no such match
    ///     was found.
    /// </param>
    /// <returns><see langword="true" /> if the channel macro exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="channel" /> or <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public bool TryGetChannelMacro(DiscordChannel channel, string name, [NotNullWhen(true)] out Macro? macro)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(name);

        if (channel.Guild is not { } guild)
        {
            macro = null;
            return false;
        }

        if (_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros) && macros.TryGetValue(name, out macro))
        {
            if (macro.ChannelId != channel.Id)
            {
                macro = null;
                return false;
            }

            return true;
        }

        macro = null;
        return false;
    }

    /// <summary>
    ///     Attempts to find a global macro by its name, and returns a value indicating the success of the operation. 
    /// </summary>
    /// <param name="guild">The guild whose macros to search.</param>
    /// <param name="name">The name of the macro to find.</param>
    /// <param name="macro">
    ///     When this method returns, contains the macro whose name is equal to <paramref name="name" /> and whose
    ///     <see cref="Macro.ChannelId" /> is <see langword="null" />, or <see langword="null" /> if no such match was found.
    /// </param>
    /// <returns><see langword="true" /> if the global macro exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" /> or <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public bool TryGetGlobalMacro(DiscordGuild guild, string name, [NotNullWhen(true)] out Macro? macro)
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(name);

        if (_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros) && macros.TryGetValue(name, out macro))
        {
            if (macro.ChannelId is not null)
            {
                macro = null;
                return false;
            }

            return true;
        }

        macro = null;
        return false;
    }

    /// <summary>
    ///     Attempts to find a macro by its name, and returns a value indicating the success of the operation. 
    /// </summary>
    /// <param name="guild">The guild whose macros to search.</param>
    /// <param name="name">The name of the macro to find.</param>
    /// <param name="macro">
    ///     When this method returns, contains the macro whose name is equal to <paramref name="name" />, or
    ///     <see langword="null" /> if no such match was found.
    /// </param>
    /// <returns><see langword="true" /> if the macro exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="guild" /> or <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    public bool TryGetMacro(DiscordGuild guild, string name, [NotNullWhen(true)] out Macro? macro)
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(name);

        if (_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros) && macros.TryGetValue(name, out macro))
            return true;

        macro = null;
        return false;
    }

    /// <summary>
    ///     Updates all guild macros from the database.
    /// </summary>
    /// <param name="guild">The guild whose macros to retrieve.</param>
    public async Task UpdateFromDatabaseAsync(DiscordGuild guild)
    {
        ArgumentNullException.ThrowIfNull(guild);

        if (!_macros.TryGetValue(guild, out Dictionary<string, Macro>? macros))
        {
            macros = new Dictionary<string, Macro>();
            _macros.Add(guild, macros);
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<MarcoContext>();

        foreach (Macro macro in context.Macros.Where(m => m.GuildId == guild.Id))
        {
            macros[macro.Name] = macro;
            List<string> aliases = macro.Aliases;
            for (var index = 0; index < aliases.Count; index++)
                macros[aliases[index]] = macro;
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<MarcoContext>();
        await context.Database.EnsureCreatedAsync(stoppingToken).ConfigureAwait(false);

        _discordClient.GuildAvailable += OnGuildAvailable;
    }

    private Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        return UpdateFromDatabaseAsync(e.Guild);
    }
}
