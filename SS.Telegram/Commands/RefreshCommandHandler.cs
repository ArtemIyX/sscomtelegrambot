using SS.Data;
using SS.Telegram.Interfaces;
using SS.Telegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SS.Telegram.Commands;

public class RefreshCommandHandler(SSComService ssComService) : IBotCommandHandler
{
    public string Command => "/refresh";
    
    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Text is null)
            {
                throw new Exception("Empty message text received");
            }

            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Refreshing...",
                cancellationToken: cancellationToken);
            await ssComService.RefreshAsync();
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Fetched {ssComService.NumFlats} flats in Riga!",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Error: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }
}