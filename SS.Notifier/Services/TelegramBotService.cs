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

        /*string text = entity.ToTelegramString();

        await telegramBotClient.SendMessage(new ChatId((chatId)), text, ParseMode.Markdown,
            cancellationToken: cancellationToken);*/
        
        if (!photos.Any())
        {
            // Fallback: just send text if no photos
            string text = entity.ToTelegramString();
            await telegramBotClient.SendMessage(
                new ChatId(chatId), 
                text, 
                parseMode: ParseMode.Markdown, 
                cancellationToken: cancellationToken);
            return;
        }
        
        string caption = entity.ToTelegramString();
        var media = new List<IAlbumInputMedia>();

        for (int i = 0; i < photos.Count; i++)
        {
            var photoPath = photos[i];
            InputMediaPhoto mediaPhoto;

            // Check if it's a URL or local file
            if (Uri.TryCreate(photoPath, UriKind.Absolute, out var uri) && 
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                // Remote URL
                mediaPhoto = new InputMediaPhoto(new InputFileUrl(photoPath));
            }
            else
            {
                // Local file path
                if (!File.Exists(photoPath))
                    throw new FileNotFoundException($"Photo not found: {photoPath}");

                var fileStream = File.OpenRead(photoPath);
                mediaPhoto = new InputMediaPhoto(InputFile.FromStream(fileStream, Path.GetFileName(photoPath)));
            }

            // Only first photo gets the caption
            if (i == 0)
                mediaPhoto.Caption = caption;
        
            mediaPhoto.ParseMode = ParseMode.Markdown;
            media.Add(mediaPhoto);
        }

        await telegramBotClient.SendMediaGroup(
            chatId: new ChatId(chatId),
            media: media,
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