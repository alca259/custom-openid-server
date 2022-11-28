using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data.Mappings;

internal class RolesMapping : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(pk => pk.Id);
        builder.ToTable("Role", "Identity");

        builder.Property(p => p.Id).HasColumnName("Id").IsRequired().HasMaxLength(128);
        builder.Property(p => p.Name).HasColumnName("Name").IsRequired().HasMaxLength(256);
        builder.Property(p => p.NormalizedName).HasColumnName("NormalizedName").HasMaxLength(256);
        builder.Property(p => p.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");

        builder.HasIndex(ix => ix.Name).HasDatabaseName("IX_Roles_Name").IsUnique(true);
    }
}
