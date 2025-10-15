using System.Text;
using SS.Data;
using SS.Telegram.Interfaces;
using SS.Telegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SS.Telegram.Commands;

public class FilterCommandHandler(SSComService ssComService) : IBotCommandHandler
{
    public string Command => "/filter";

    private string ToTelegramString(ApartmentModel apartmentModel)
    {
        return
            $@"🏠 [{apartmentModel.Region}]({apartmentModel.Link}) 💰 {apartmentModel.PricePerMonth:0,0}€ | {apartmentModel.Area} m² | {apartmentModel.PricePerSquare():0.#}€/m² | {apartmentModel.Rooms}r | {apartmentModel.Floor}f | {apartmentModel.Series}";
    }


    private List<string> SplitIntoChunks(List<string> list)
    {
        const int maxMessageLength = 4000; // Safe buffer for Telegram's 4096 limit
        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        int separatorLength = "\n".Length; // Length of separator between entries

        foreach (var entry in list)
        {
            // Calculate length if we add this entry (include separator if not first in chunk)
            int addedLength = entry.Length + (currentChunk.Length > 0 ? separatorLength : 0);

            if (currentChunk.Length + addedLength > maxMessageLength)
            {
                // Current chunk is full; save it and start a new one
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }

                // If single entry is too long (rare, but handle it)
                if (entry.Length > maxMessageLength)
                {
                    // Truncate or handle error; for now, add as-is or split entry if needed
                    chunks.Add(entry.Substring(0, maxMessageLength)); // Or throw/log
                    continue;
                }
            }

            // Add separator if not first
            if (currentChunk.Length > 0)
            {
                currentChunk.Append("\n");
            }

            currentChunk.Append(entry);
        }


        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        return chunks;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Text is null)
            {
                throw new Exception("Empty message text received");
            }

            var split = message.Text.Split(' ').ToList();
            if (split.Count < 2)
            {
                split.Add("100-400;1,2;30-60;plyavnieki,purvciems,darzciems");
                //throw new Exception("Not enough arguments received\n Try /filter 100-450;1,2;10-60;plyavnieki,purvciems,darzciems");
            }

            if (split.Count > 2)
            {
                throw new Exception("To many arguments received\n Try /filter 100-450;1,2;10-60;plyavnieki,purvciems,darzciems");
            }

            if (!ssComService.IsApartmentContainerInitialized)
            {
                throw new Exception("Call /refresh first");
            }

            string argument = split[1];
            ApartmentFilter filter = ApartmentFilter.FromString(argument);

            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Filter: {argument}\n{filter.ToString()}",
                cancellationToken: cancellationToken);

            var flatsByRegion = ssComService.Filter(filter);

            if (flatsByRegion.Count == 0)
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"No flats found, try another filter",
                    cancellationToken: cancellationToken);
                return;
            }

            foreach (var region in flatsByRegion)
            {
                List<ApartmentModel> flats = region.Value.ToList();
                if (flats.Count == 0)
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"No flats in '{region.Key}",
                        cancellationToken: cancellationToken);
                    continue;
                }

                List<string> str = flats.Select(ToTelegramString).ToList();
                var chunks = SplitIntoChunks(str);
                foreach (var chunk in chunks)
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: chunk,
                        cancellationToken: cancellationToken,
                        parseMode: ParseMode.Markdown);
                }
            }
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