using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
        await CreateSwaggerClient(scope, cancellationToken);
        await CreateAngularClient(scope, cancellationToken);
        await RegisterScopes(scope, cancellationToken);
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
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials
            }
        }, cancellationToken);
    }

    private static async Task CreateSwaggerClient(IServiceScope scope, CancellationToken cancellationToken)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existDefaultAppClient = await manager.FindByClientIdAsync("swagger-client", cancellationToken);
        if (existDefaultAppClient != null) return;

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "swagger-client",
            ClientSecret = "22222222-1FEB-4D9B-B0EB-169E73F0987B",
            DisplayName = "Auth Server Swagger Client",
            ConsentType = ConsentTypes.Implicit,
            RedirectUris =
            {
                new Uri("https://localhost:5201/swagger/oauth2-redirect.html"),
                new Uri("https://localhost:5201/signin-oidc"),
                new Uri("https://localhost:5201/signout-oidc")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Introspection,
                Permissions.GrantTypes.Implicit,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Token,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                $"{Permissions.Prefixes.Scope}API"
            }
        }, cancellationToken);
    }

    private static async Task CreateAngularClient(IServiceScope scope, CancellationToken cancellationToken)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existDefaultAppClient = await manager.FindByClientIdAsync("angular-client", cancellationToken);
        if (existDefaultAppClient != null) return;

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "angular-client",
            ClientSecret = "D1D312D8-1FEB-4D9B-B0EB-169E73F0987B",
            DisplayName = "Auth Server Angular Client",
            ConsentType = ConsentTypes.Explicit,
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:6001/signout-oidc"),
                new Uri("http://localhost:6001")
            },
            RedirectUris =
            {
                new Uri("http://localhost:6001/signin-oidc"),
                new Uri("http://localhost:6001")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                $"{Permissions.Prefixes.Scope}API"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        }, cancellationToken);
    }

    private static async Task RegisterScopes(IServiceScope scope, CancellationToken cancellationToken)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await manager.FindByNameAsync("API", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                DisplayName = "Default Client API scope",
                DisplayNames =
                {
                    [CultureInfo.GetCultureInfo("es-ES")] = "Acceso a la API de demo"
                },
                Name = "API",
                Resources =
                {
                    "default-client"
                }
            }, cancellationToken);
        }
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
