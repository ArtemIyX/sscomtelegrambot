using Telegram.Bot;
using Telegram.Bot.Types;

namespace SS.Telegram.Interfaces;

public interface IBotCommandHandler
{
    string Command { get; } // e.g., "/start"
    Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}