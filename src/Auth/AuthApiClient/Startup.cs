using AuthApiClient.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using OpenIddict.Client;
using OpenIddict.Validation.AspNetCore;

namespace AuthApiClient;

public sealed class Startup
{
    private const string AUTH_URI = "https://localhost:5001";
    public const string API_CLIENT_ID = "default-api-client";
    public const string API_CLIENT_SECRET = "11111111-54AE-4044-8E05-07044FD96943";
    public const string SWAGGER_CLIENT_ID = "swagger-client";
    public const string SWAGGER_CLIENT_SECRET = "22222222-1FEB-4D9B-B0EB-169E73F0987B";

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

        // Configuración de OpenIDDict server
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect(options =>
        {
            options.Authority = AUTH_URI;
            options.ClientId = SWAGGER_CLIENT_ID;
            options.ClientSecret = SWAGGER_CLIENT_SECRET;

            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ResponseType = OpenIdConnectResponseType.Token;

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = false;
        });

        services.AddScoped<IAuthorizationHandler, RequireScopeHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Default", cfg =>
            {
                cfg.Requirements.Add(new RequireScope("API"));
            });
        });

        services.AddOpenIddict()
            //.AddValidation(options =>
            //{
            //    // Note: the validation handler uses OpenID Connect discovery
            //    // to retrieve the address of the introspection endpoint.
            //    options.SetIssuer(AUTH_URI);

            //    // Configure the validation handler to use introspection and register the client
            //    // credentials used when communicating with the remote introspection endpoint.
            //    options.UseIntrospection()
            //            .SetClientId(API_CLIENT_ID)
            //            .SetClientSecret(API_CLIENT_SECRET);

            //    // disable access token encyption for this
            //    options.UseAspNetCore();

            //    // Register the System.Net.Http integration.
            //    options.UseSystemNetHttp();

            //    // Register the ASP.NET Core host.
            //    options.UseAspNetCore();
            //})
            .AddClient(options =>
            {
                options.AddEphemeralEncryptionKey()
                       .AddEphemeralSigningKey();

                options.UseSystemNetHttp();

                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = new Uri(AUTH_URI, UriKind.Absolute),
                    ClientId = API_CLIENT_ID,
                    ClientSecret = API_CLIENT_SECRET,
                    Scopes =
                    {
                        "API"
                    }
                });
            });

        var apiName = "Foo API";
        var apiAuthority = AUTH_URI;
        var apiAuthUrl = "";
        var apiTokenUrl = "";
        var basePath = AUTH_URI;
        var scopeName = "API";//IdentityServerConstants.LocalApi.ScopeName;

        services.AddSwaggerGen(options =>
        {
            options.DescribeAllParametersInCamelCase();
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = apiName,
                Version = "v1",
                Description = apiName + " HTTP",
                Contact = new OpenApiContact
                {
                    Name = "Foo Company",
                    Email = string.Empty,
                    Url = new Uri("https://foo.com/"),
                }
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl =
                            string.IsNullOrWhiteSpace(apiAuthUrl) ?
                            new Uri($"{apiAuthority}/connect/authorize") :
                            new Uri($"{apiAuthUrl}"),

                        TokenUrl =
                            string.IsNullOrWhiteSpace(apiTokenUrl) ?
                            new Uri($"{apiAuthority}/connect/token") :
                            new Uri($"{apiTokenUrl}"),

                        Scopes = new Dictionary<string, string>
                            {
                                { scopeName, basePath }
                            }
                    }
                },
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Name = "Authorization"

            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { scopeName }
                }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Foo API V1");

            c.OAuthClientId(SWAGGER_CLIENT_ID);
            c.OAuthAppName($"API Swagger UI");
            c.OAuthClientSecret(API_CLIENT_SECRET);

            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

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
