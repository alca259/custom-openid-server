namespace AuthServer;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHost(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHost(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, opt) =>
        {
            var env = context.HostingEnvironment;

            opt.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        })
        .ConfigureWebHostDefaults(opt =>
        {
            opt.UseStartup<Startup>();
        });
}