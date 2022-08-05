namespace Marco.Data;

internal sealed class Macro : IEquatable<Macro>
{
    /// <summary>
    ///     Gets or sets the ID of the channel to which this macro is restricted.
    /// </summary>
    /// <value>The channel ID, or <see langword="null" /> if this is a global macro.</value>
    public ulong? ChannelId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the guild in which the macro was created.
    /// </summary>
    /// <value>The guild ID.</value>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the macro.
    /// </summary>
    /// <value>The macro name.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the response of this macro.
    /// </summary>
    /// <value>The response.</value>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    ///     Determines whether two <see cref="Macro" /> instances are equal.
    /// </summary>
    /// <param name="left">The first macro.</param>
    /// <param name="right">The second macro.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Macro? left, Macro? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Determines whether two <see cref="Macro" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first macro.</param>
    /// <param name="right">The second macro.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Macro? left, Macro? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether this <see cref="Macro" /> is equal to another <see cref="Macro" />.
    /// </summary>
    /// <param name="other">The other macro.</param>
    /// <returns>
    ///     <see langword="true" /> if this macro is equal to <paramref name="other" />; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(Macro? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GuildId == other.GuildId && Name == other.Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Macro other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(GuildId, Name);
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
