using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Data.Mappings;

internal class UsersMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(pk => pk.Id);
        builder.ToTable("User", "Identity");

        builder.Property(p => p.Id).HasColumnName("Id").IsRequired().HasMaxLength(128);

        builder.Property(p => p.Email).HasColumnName("Email").HasMaxLength(256);
        builder.Property(p => p.EmailConfirmed).HasColumnName("EmailConfirmed").IsRequired();
        builder.Property(p => p.PasswordHash).HasColumnName("PasswordHash");
        builder.Property(p => p.SecurityStamp).HasColumnName("SecurityStamp");
        builder.Property(p => p.PhoneNumber).HasColumnName("PhoneNumber");
        builder.Property(p => p.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed").IsRequired();
        builder.Property(p => p.TwoFactorEnabled).HasColumnName("TwoFactorEnabled").IsRequired();
        builder.Property(p => p.LockoutEnabled).HasColumnName("LockoutEnabled").IsRequired();
        builder.Property(p => p.AccessFailedCount).HasColumnName("AccessFailedCount").IsRequired();
        builder.Property(p => p.UserName).HasColumnName("UserName").IsRequired().HasMaxLength(256);
        builder.Property(p => p.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
        builder.Property(p => p.LockoutEnd).HasColumnName("LockoutEnd");
        builder.Property(p => p.NormalizedEmail).HasColumnName("NormalizedEmail");
        builder.Property(p => p.NormalizedUserName).HasColumnName("NormalizedUserName");

        builder.HasIndex(ix => ix.UserName).HasDatabaseName("IX_Users_Uq_UserName").IsUnique(true);
    }
}
