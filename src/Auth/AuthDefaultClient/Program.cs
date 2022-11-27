using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;
using System.Net.Http.Headers;

namespace AuthDefaultClient;

internal class Program
{
    private const string AUTH_URI = "https://localhost:5001";
    public const string API_CLIENT_ID = "default-client";
    public const string API_CLIENT_SECRET = "DB95C15D-54AE-4044-8E05-07044FD96943";

    private static async Task Main(string[] args)
    {
        await using var provider = SetupServices().BuildServiceProvider();

        Console.WriteLine("Press \"Enter\" when auth server start");
        Console.ReadLine();

        var token = await GetTokenAsync(provider);
        Console.WriteLine("Access token: {0}", token);
        Console.WriteLine();

        var resource = await GetResourceAsync(provider, token);
        Console.WriteLine("API response: {0}", resource);
        Console.ReadLine();
    }

    private static IServiceCollection SetupServices()
    {
        var services = new ServiceCollection();
        services.AddOpenIddict()
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

        return services;
    }

    private static async Task<string> GetTokenAsync(IServiceProvider provider)
    {
        var service = provider.GetRequiredService<OpenIddictClientService>();

        var (response, _) = await service.AuthenticateWithClientCredentialsAsync(new Uri(AUTH_URI, UriKind.Absolute));
        return response.AccessToken;
    }

    private static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
    {
        using var client = provider.GetRequiredService<HttpClient>();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{AUTH_URI}/weather/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}