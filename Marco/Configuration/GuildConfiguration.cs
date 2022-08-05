namespace Marco.Configuration;

/// <summary>
///     Represents a guild configuration.
/// </summary>
internal sealed class GuildConfiguration
{
    /// <summary>
    ///     Gets or sets the cooldown, in milliseconds, between duplicate macro usage.
    /// </summary>
    /// <value>The cooldown in milliseconds.</value>
    public long Cooldown { get; set; } = 5000;
    
    /// <summary>
    ///     Gets or sets the prefix.
    /// </summary>
    /// <value>The prefix.</value>
    public string Prefix { get; set; } = "[]";

    /// <summary>
    ///     Gets or sets the reaction configuration.
    /// </summary>
    /// <value>The reaction configuration.</value>
    public ReactionConfiguration ReactionConfiguration { get; set; } = new();
}
