using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SS.Telegram.Commands;
using SS.Telegram.Interfaces;
using SS.Telegram.Services;
using Telegram.Bot;

namespace SS.Telegram;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddUserSecrets<Program>();  // Loads user secrets
            })
            .ConfigureServices((context, services) =>
            {
                var botToken = context.Configuration["Telegram:BotToken"];
                if (string.IsNullOrEmpty(botToken))
                {
                    throw new InvalidOperationException("Telegram:BotToken is not configured. Set it via environment variables or other config sources.");
                }

                // Register Telegram bot client
                services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

                // Register hosted service
                services.AddHostedService<TelegramBotHostedService>();

                // Register command handlers (add more as needed)
                services.AddTransient<IBotCommandHandler, StartCommandHandler>();

            });
}