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

public class TelegramBotService(
    ILogger<TelegramBotService> logger,
    ITelegramBotClient telegramBotClient,
    IOptions<AppSettings> appSettings) : ITelegramBotService
{
    public async Task SendApartment(ApartmentEntity entity, List<string> photos, CancellationToken cancellationToken)
    {
        string region = entity.Region;
        long chatId = appSettings.Value.Telegram.Chat;
        if (!appSettings.Value.Telegram.Threads.TryGetValue(region, out int threadId))
        {
            throw new ArgumentException($"{region} not found");
        }

        string caption = entity.ToTelegramString();
        if (!photos.Any())
        {
            // Fallback: just send text if no photos
            logger.LogInformation("No photos found for '{id}' in '{region}'", entity.Id,
                entity.Region);
            logger.LogInformation("Sending telegram: '{id}' in '{region}' to chat '{chatId}'", entity.Id,
                entity.Region, threadId);
            await telegramBotClient.SendMessage(
                new ChatId(chatId),
                caption,
                messageThreadId: threadId,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            logger.LogInformation("[{chatId}] -> {text}", threadId, caption);
            return;
        }

        if (photos.Count > 8)
            photos = photos.Take(8).ToList(); // Keep first 8

        List<IAlbumInputMedia> media = new List<IAlbumInputMedia>();

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

        logger.LogInformation("Sending telegram: '{id}' in '{region}' to chat '{chatId}' with '{n}' photos", entity.Id,
            entity.Region, threadId, photos.Count);
        await telegramBotClient.SendMediaGroup(
            chatId: new ChatId(chatId),
            media: media,
            messageThreadId: threadId,
            cancellationToken: cancellationToken);
        logger.LogInformation("[{chatId}] -> {text}", threadId, caption);
    }
}