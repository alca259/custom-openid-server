using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace AuthServer.Infrastructure.Data;

public sealed class SeedInitTask : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedInitTask(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ctx.Database.EnsureCreatedAsync(cancellationToken);

        await CreateDefaultClient(scope, cancellationToken);
        await CreateDefaultUser(scope);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task CreateDefaultClient(IServiceScope scope, CancellationToken cancellationToken)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existDefaultAppClient = await manager.FindByClientIdAsync("default-client", cancellationToken);
        if (existDefaultAppClient != null) return;

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "default-client",
            ClientSecret = "DB95C15D-54AE-4044-8E05-07044FD96943",
            DisplayName = "Auth Server Default Client",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
            }
        }, cancellationToken);
    }

    private static async Task CreateDefaultUser(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = "administrator",
            FkUserRoles = new List<UserRole>
            {
                new UserRole
                {
                    FkRole = new Role
                    {
                        Name = "admin",
                        NormalizedName = "ADMIN"
                    }
                }
            }
        };

        var existUser = await userManager.FindByNameAsync(user.UserName);
        if (existUser != null) return;

        var pass = userManager.PasswordHasher.HashPassword(user, "demo");
        user.PasswordHash = pass;
        await userManager.CreateAsync(user);
    }
}
