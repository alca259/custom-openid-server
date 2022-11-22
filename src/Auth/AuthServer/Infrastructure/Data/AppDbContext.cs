using AuthServer.Infrastructure.Data.Mappings;
using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

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
        modelBuilder.ApplyConfiguration(new UsersMapping());
        modelBuilder.ApplyConfiguration(new RolesMapping());
        modelBuilder.ApplyConfiguration(new UserRolesMapping());
    }
}
