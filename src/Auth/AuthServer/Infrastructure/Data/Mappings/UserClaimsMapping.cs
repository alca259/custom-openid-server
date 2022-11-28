using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data.Mappings;

internal class UserClaimsMapping : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.HasKey(pk => pk.Id);
        builder.ToTable("User_Claim", "Identity");

        builder.Property(p => p.Id).HasColumnName("Id").IsRequired();
        builder.Property(p => p.UserId).HasColumnName("User_Id").IsRequired();
        builder.Property(p => p.ClaimType).HasColumnName("Claim_Type");
        builder.Property(p => p.ClaimValue).HasColumnName("Claim_Value");

        builder.HasOne(ur => ur.FkUser).WithMany(u => u.FkUserClaims).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
