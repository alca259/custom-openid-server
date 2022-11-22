using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data.Mappings
{
    internal class UserRolesMapping : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(pk => new { pk.UserId, pk.RoleId });
            builder.ToTable("User_Role", "Identity");

            builder.Property(p => p.UserId).HasColumnName("User_Id").IsRequired().HasMaxLength(128);
            builder.Property(p => p.RoleId).HasColumnName("Role_Id").IsRequired().HasMaxLength(128);

            builder.HasOne(ur => ur.FkUser).WithMany(u => u.FkUserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ur => ur.FkRole).WithMany(r => r.FkUserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
