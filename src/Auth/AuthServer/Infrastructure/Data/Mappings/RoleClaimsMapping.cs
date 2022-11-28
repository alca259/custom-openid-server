using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data.Mappings;

internal class RoleClaimsMapping : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.HasKey(pk => pk.Id);
        builder.ToTable("Role_Claim", "Identity");

        builder.Property(p => p.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(p => p.ClaimType).HasColumnName("ClaimType");
        builder.Property(p => p.ClaimValue).HasColumnName("ClaimValue");
        builder.Property(p => p.RoleId).HasColumnName("RoleId").IsRequired();

        builder.HasIndex(i => i.RoleId).HasDatabaseName("IX_Role_Claims").IsUnique(false);
    }
}
