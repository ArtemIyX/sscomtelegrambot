using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SS.Notifier.Services;

public class DebugChatHostedService(
    ILogger<DebugChatHostedService> logger,
    ITelegramBotClient telegramBotClient) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting debug chat service");
        // Set up polling receiver
        var receiverOptions = new ReceiverOptions
        {
            //AllowedUpdates = [UpdateType.Message] // For simplicity, only handle messages
        };

        telegramBotClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("DebugChatHostedService is shutting down");
        return Task.CompletedTask;
    }

    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Update:ID:{updateId}, ChatID:{chatId}, ThreadID:{thread} Message: {message}",
            update.Id, update.Message?.Chat.Id, update.Message?.MessageThreadId, update.Message?.Text);
        return Task.CompletedTask;
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

        logger.LogError(exception, errorMessage);
        return Task.CompletedTask;
    }
}