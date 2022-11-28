using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data.Mappings;

internal class UserTokensMapping : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.HasKey(pk => new { pk.UserId, pk.LoginProvider, pk.Name });
        builder.ToTable("User_Token", "Identity");

        builder.Property(p => p.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(p => p.LoginProvider).HasColumnName("LoginProvider");
        builder.Property(p => p.Name).HasColumnName("Name");
        builder.Property(p => p.Value).HasColumnName("Value");

        builder.HasOne(ur => ur.FkUser).WithMany(u => u.FkUserTokens).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
