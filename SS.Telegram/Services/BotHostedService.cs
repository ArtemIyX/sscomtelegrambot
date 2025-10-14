using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SS.Telegram.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SS.Telegram.Services;

public class TelegramBotHostedService : IHostedService
{
    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IEnumerable<IBotCommandHandler> _commandHandlers;

    private readonly Dictionary<string, IBotCommandHandler> _commands = new();

    public TelegramBotHostedService(
        ILogger<TelegramBotHostedService> logger,
        ITelegramBotClient botClient,
        IEnumerable<IBotCommandHandler> commandHandlers)
    {
        _logger = logger;
        _botClient = botClient;
        _commandHandlers = commandHandlers;

        // Register all command handlers
        foreach (var handler in _commandHandlers)
        {
            _commands[handler.Command] = handler;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Telegram bot...");

        var me = await _botClient.GetMe(cancellationToken);
        _logger.LogInformation($"Bot {me.Username} is online.");

        // Set up polling receiver
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message] // For simplicity, only handle messages
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Telegram bot...");
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not { } message || message.Type != MessageType.Text)
            return;

        var text = message.Text?.Trim();
        if (string.IsNullOrEmpty(text) || !text.StartsWith('/'))
            return; // Only handle commands starting with /

        _logger.LogInformation("[{cha}]: {message}", message.Chat.Id, text);

        // Extract command (ignore parameters for simplicity)
        var command = text.Split(' ')[0];

        if (_commands.TryGetValue(command, out var handler))
        {
            try
            {
                await handler.HandleAsync(botClient, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling command {command}");
            }
        }
        else
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Unknown command.",
                cancellationToken: cancellationToken);
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}