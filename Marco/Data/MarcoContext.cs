using Marco.Data.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Marco.Data;

/// <summary>
///     Represents a session with the <c>marco.db</c> database.
/// </summary>
internal sealed class MarcoContext : DbContext
{
    /// <summary>
    ///     Gets the set of macros.
    /// </summary>
    /// <value>The set of macros.</value>
    public DbSet<Macro> Macros { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("Data Source=data/marco.db");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new MacroConfiguration());
    }
}
