namespace Marco.Configuration;

/// <summary>
///     Represents a reaction configuration.
/// </summary>
internal sealed class ReactionConfiguration
{
    /// <summary>
    ///     Gets or sets the reaction the bot will give when the same macro is used in quick succession.
    /// </summary>
    /// <value>The cooldown reaction.</value>
    public string? CooldownReaction { get; set; } = ":hourglass_flowing_sand:";

    /// <summary>
    ///     Gets or sets the reaction the bot will give when a known macro is used.
    /// </summary>
    /// <value>The success reaction.</value>
    public string? SuccessReaction { get; set; } = ":white_check_mark:";

    /// <summary>
    ///     Gets or sets the reaction the bot will give when an unknown macro is used.
    /// </summary>
    /// <value>The unknown reaction.</value>
    public string? UnknownReaction { get; set; } = null;
}
