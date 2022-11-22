using AuthServer.Infrastructure.Data;
using AuthServer.Infrastructure.Domain.Identity;
using AuthServer.Infrastructure.Domain.Identity.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace AuthServer;

public sealed class Startup
{
    public IConfiguration Configuration { get; }
    public IHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            // Aquí se puede especificar clases personalizadas para OpenIDDict
            opt.UseOpenIddict();
        });

        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequiredLength = 0;
            opt.Password.RequiredUniqueChars = 0;
            opt.Password.RequireDigit = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireUppercase = false;

            // Mapeamos Identity para que use los nombres de los claims de OpenIDDict (aunque creo que son exactamente los mismos)
            opt.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Username;
            opt.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            opt.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            opt.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
        });

        // Configuración de OpenIDDict server
        services.AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore(); // Aquí se puede configurar que use otro contexto
            })
            .AddServer(opt =>
            {
                // Endpoints que daremos de alta
                opt.SetTokenEndpointUris("/connect/token");
                opt.SetUserinfoEndpointUris("/connect/userinfo");
                //opt.SetLogoutEndpointUris(""); // TODO: Ver documentación
                //opt.SetVerificationEndpointUris(""); // TODO: Ver documentación
                //opt.SetAuthorizationEndpointUris(""); // TODO: Ver documentación
                //opt.SetConfigurationEndpointUris(""); // TODO: Ver documentación
                //opt.SetIntrospectionEndpointUris(""); // TODO: Ver documentación
                //opt.SetRevocationEndpointUris(""); // TODO: Ver documentación

                opt.AllowPasswordFlow();
                opt.AllowRefreshTokenFlow();
                // TODO: Aquí hay más interesantes, habrá que ver la documentación para ver como funcionan

                //opt.UseReferenceAccessTokens(); // Guarda el token de acceso en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.
                //opt.UseReferenceRefreshTokens(); // Guarda el token de refresco en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.

                // Registro de scopes
                opt.RegisterScopes(OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Scopes.Profile);

                // Lifetime de los tokens
                opt.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                opt.SetRefreshTokenLifetime(TimeSpan.FromDays(1));

                // Para development
                if (Environment.IsDevelopment())
                {
                    opt.AddDevelopmentEncryptionCertificate();
                    opt.AddDevelopmentSigningCertificate();
                }

                opt.UseAspNetCore()
                    .EnableTokenEndpointPassthrough(); // No me he enterado de que hace exactamente, ver documentación o ir probando
            })
            .AddValidation(opt =>
            {
                opt.UseLocalServer();
                opt.UseAspNetCore();
            });

        services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = OpenIddictConstants.Schemes.Bearer;
            opt.DefaultChallengeScheme = OpenIddictConstants.Schemes.Bearer;
        });

        services.AddIdentity<User, Role>()
            .AddSignInManager()
            .AddUserStore<AppUserStore>()
            .AddRoleStore<AppRoleStore>()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<Role>>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(opt =>
        {
            opt.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}");
        });

        #region Guarrada - Esto a un seed o a otro lado
        CreateDefaultClient(app).GetAwaiter().GetResult();
        CreateSeedUser(app).GetAwaiter().GetResult();
        #endregion
    }

    private async Task CreateDefaultClient(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ctx.Database.EnsureCreatedAsync();

        var openIdManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var existDefaultAppClient = await openIdManager.FindByClientIdAsync("default-client");
        if (existDefaultAppClient != null) return;

        await openIdManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "default-client",
            ClientSecret = "DB95C15D-54AE-4044-8E05-07044FD96943",
            DisplayName = "Auth Server Default Client",
            Permissions = // Forma de asignar mágica (corramos un tupido velo e imaginamos que esto no está aquí)
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.Password
            }
        });
    }

    private async Task CreateSeedUser(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

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

        var existUser = userManager.FindByNameAsync(user.UserName);
        if (existUser != null) return;

        var pass = userManager.PasswordHasher.HashPassword(user, "demo");
        user.PasswordHash = pass;
        await userManager.CreateAsync(user);
    }
}
