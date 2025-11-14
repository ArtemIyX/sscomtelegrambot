// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SS.Data;
using SS.Parser;

namespace consoletest;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        Task t = host.RunAsync();
        ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
        IWebFetcherService fetcher = host.Services.GetRequiredService<IWebFetcherService>();
        IApartmentParserService parser = host.Services.GetRequiredService<IApartmentParserService>();
        var container = await fetcher.FetchApartmentsAsync(new ApartmentFilter()
        {
            MinPrice = 100,
            MaxPrice = 350,
            MinSquare = 35,
            Rooms = [2]
        });

        foreach (var apartment in container.Map)
        {
            logger.LogInformation(apartment.Value.ToString());
        }
        
        var httpClient = new  HttpClient();
        var html =await httpClient.GetAsync("https://www.ss.lv/msg/en/real-estate/flats/riga/mezhciems/bepghe.html#photo-1");
        
        List<string> photoUrl =
           await parser.ParseApartmentPhotoAsync(
               await html.Content.ReadAsStringAsync());
        logger.LogInformation("URLs: ");
        foreach (var url in photoUrl)
        {
            logger.LogInformation(url);
        }
        
        await t;
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddUserSecrets<Program>(); // Loads user secrets
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient("SsClient", client =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseCookies = true, // Enables cookie persistence
                    //AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Br
                });
                services.AddTransient<IApartmentParserService, ApartmentParserService>();
                services.AddTransient<IWebFetcherService, WebFetcherService>();
            });
}