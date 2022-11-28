using AuthServer.Infrastructure.Data.Mappings;
using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<User, Role, long, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Data Source=.\\GENSHIN;Initial Catalog=AuthServer;User Id=sa;Password=sa;Integrated Security=False;MultipleActiveResultSets=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RoleClaimsMapping());
        modelBuilder.ApplyConfiguration(new RolesMapping());
        modelBuilder.ApplyConfiguration(new UsersMapping());
        modelBuilder.ApplyConfiguration(new UserClaimsMapping());
        modelBuilder.ApplyConfiguration(new UserLoginsMapping());
        modelBuilder.ApplyConfiguration(new UserRolesMapping());
        modelBuilder.ApplyConfiguration(new UserTokensMapping());
    }
}
