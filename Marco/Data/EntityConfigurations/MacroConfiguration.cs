using Marco.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marco.Data.EntityConfigurations;

internal sealed class MacroConfiguration : IEntityTypeConfiguration<Macro>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Macro> builder)
    {
        builder.ToTable(nameof(Macro));
        builder.HasKey(e => new {e.GuildId, e.Name});

        builder.Property(e => e.GuildId).IsRequired();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.ChannelId);
        builder.Property(e => e.Response).IsRequired();
        builder.Property(e => e.Aliases).IsRequired().HasConversion<StringListToBytesConverter>();
    }
}
