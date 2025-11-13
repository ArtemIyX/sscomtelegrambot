using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Notifier.Data.Entity;
using SS.Notifier.Data.Extensions;
using SS.Notifier.Data.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SS.Notifier.Services;

public interface ITelegramBotService
{
    public Task SendApartment(ApartmentEntity entity, List<string> photos,
        CancellationToken cancellationToken = default);
}

public class TelegramBotService(ILogger<TelegramBotService> logger,
    ITelegramBotClient telegramBotClient,
    IOptions<AppSettings> appSettings) : ITelegramBotService
{
    public async Task SendApartment(ApartmentEntity entity, List<string> photos, CancellationToken cancellationToken)
    {
        string region = entity.Region;
        if (!appSettings.Value.Chats.TryGetValue(region, out var chatId))
        {
            throw new ArgumentException($"{region} not found");
        }

        string text = entity.ToTelegramString();

        await telegramBotClient.SendMessage(new ChatId((chatId)), text, ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Update received: {update}", update.Message.Chat.Id);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}