using AuthServer.Infrastructure.Data;
using AuthServer.Infrastructure.Domain.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
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

                opt.AllowPasswordFlow(); // Auth server
                opt.AllowAuthorizationCodeFlow(); // SPA
                opt.AllowClientCredentialsFlow(); // Entre APIs
                opt.AllowRefreshTokenFlow();

                opt.UseReferenceAccessTokens(); // Guarda el token de acceso en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.
                opt.UseReferenceRefreshTokens(); // Guarda el token de refresco en BD cuando llenamos de demasiada información de claims. Si no va a ser así, mejor deshabilitarlo.

                // Force client applications to use Proof Key for Code Exchange (PKCE).
                // opt.RequireProofKeyForCodeExchange();

                // Registro de scopes
                opt.RegisterScopes(
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Scopes.Profile);

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
                .WithOrigins("https://localhost:5001", "https://localhost:5101", "https://localhost:5201")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

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

        app.UseWelcomePage();
    }
}
