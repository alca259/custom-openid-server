namespace AuthServer;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHost(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHost(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(opt =>
        {
            opt.UseStartup<Startup>();
        });
}