using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;
using System.Net.Http.Headers;

var services = new ServiceCollection();
services.AddOpenIddict()
    .AddClient(options =>
    {
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();

        //options.DisableTokenStorage();

        options.UseSystemNetHttp();

        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:5001/", UriKind.Absolute),

            ClientId = "default-client",
            ClientSecret = "DB95C15D-54AE-4044-8E05-07044FD96943"
        });
    });

await using var provider = services.BuildServiceProvider();

Console.WriteLine("Press \"Enter\" when server start");
Console.ReadLine();

var token = await GetTokenAsync(provider);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(provider, token);
Console.WriteLine("API response: {0}", resource);
Console.ReadLine();

static async Task<string> GetTokenAsync(IServiceProvider provider)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var (response, _) = await service.AuthenticateWithClientCredentialsAsync(new Uri("https://localhost:5001/", UriKind.Absolute));
    return response.AccessToken;
}

static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
{
    using var client = provider.GetRequiredService<HttpClient>();
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/weather/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}