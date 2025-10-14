using SS.Telegram.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SS.Telegram.Commands;

public class StartCommandHandler : IBotCommandHandler
{
    public string Command => "/start";

    public Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        return botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Hello! This is a simple Telegram bot framework.",
            cancellationToken: cancellationToken);
    }
}