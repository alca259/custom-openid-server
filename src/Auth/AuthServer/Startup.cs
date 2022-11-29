using AuthServer.Infrastructure.Data;
using AuthServer.Infrastructure.Domain.Identity;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Claims;

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
        services.AddMvcCore()
            .AddRazorViewEngine();

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            // Aquí se puede especificar clases personalizadas para OpenIDDict
            opt.UseOpenIddict();
        });

        // Configuración de OpenIDDict server
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        services.AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            })
            .AddServer(opt =>
            {
                // Endpoints que daremos de alta
                opt.SetAuthorizationEndpointUris("/connect/authorize");
                opt.SetTokenEndpointUris("/connect/token");
                opt.SetLogoutEndpointUris("/connect/logout");
                opt.SetUserinfoEndpointUris("/connect/userinfo");

                // Registro de scopes que el servidor de OpenID conoce para todos los clientes.
                // Deben registrarse antes de configurar flujos (GitHub Issue #835)
                opt.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    "API");

                opt.AllowPasswordFlow(); // Auth server
                opt.AllowAuthorizationCodeFlow(); // SPA
                opt.AllowClientCredentialsFlow(); // Entre APIs
                opt.AllowRefreshTokenFlow(); // Para poder renovar el token en OfflineAccess
                opt.AllowImplicitFlow(); // Para swagger

                opt.UseReferenceAccessTokens(); // Guarda el token de acceso en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.
                opt.UseReferenceRefreshTokens(); // Guarda el token de refresco en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.

                // Lifetime de los tokens
                opt.SetAccessTokenLifetime(TimeSpan.FromMinutes(5));
                opt.SetRefreshTokenLifetime(TimeSpan.FromDays(1));

                // Para development
                if (Environment.IsDevelopment())
                {
                    opt.AddDevelopmentEncryptionCertificate();
                    opt.AddDevelopmentSigningCertificate();
                }

                opt.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    //.DisableTransportSecurityRequirement() // Durante el desarrollo se puede deshabilitar Https
                    .EnableStatusCodePagesIntegration();

                // Force client applications to use Proof Key for Code Exchange (PKCE).
                // Ojo, aplica a todos los clientes. Si es para un cliente concreto, aplicar directamente sobre él.
                // opt.RequireProofKeyForCodeExchange();

                // Note: if you don't want to specify a client_id when sending
                // a token or revocation request, uncomment the following line:
                //
                // opt.AcceptAnonymousClients();

                // Note: if you want to process authorization and token requests
                // that specify non-registered scopes, uncomment the following line:
                //
                // opt.DisableScopeValidation();

                // Note: if you don't want to use permissions, you can disable
                // permission enforcement by uncommenting the following lines:
                //
                // opt.IgnoreEndpointPermissions()
                //        .IgnoreGrantTypePermissions()
                //        .IgnoreResponseTypePermissions()
                //        .IgnoreScopePermissions();

                // Note: when issuing access tokens used by third-party APIs
                // you don't own, you can disable access token encryption:
                //
                // opt.DisableAccessTokenEncryption();
            })
            .AddValidation(opt =>
            {
                opt.UseLocalServer();
                opt.UseAspNetCore();
            });

        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequiredLength = 0;
            opt.Password.RequiredUniqueChars = 0;
            opt.Password.RequireDigit = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireUppercase = false;

            opt.SignIn.RequireConfirmedAccount = false;
            opt.SignIn.RequireConfirmedEmail = false;
            opt.SignIn.RequireConfirmedPhoneNumber = false;

            // Mapeamos Identity para que use los nombres de los claims de OpenIDDict (aunque creo que son exactamente los mismos)
            opt.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Username;
            opt.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            opt.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            opt.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
        });

        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<Role>>();

        services.AddCors(opt =>
        {
            opt.AddPolicy("AllowLocal", cfg => cfg
                .AllowCredentials()
                .WithOrigins("https://localhost:5001", "https://localhost:5101", "https://localhost:5201", "http://localhost:6001")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        services.AddScoped<ILoginService<User>, LoginService>();

        services.AddHostedService<SeedInitTask>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("AllowLocal");
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseRequestLocalization(opt =>
        {
            opt.AddSupportedCultures("es-ES");
            opt.AddSupportedUICultures("es-ES");
            opt.SetDefaultCulture("es-ES");
        });
        app.UseAuthorization();

        app.UseEndpoints(opt =>
        {
            opt.MapControllers();
            opt.MapDefaultControllerRoute();
        });
    }
}
