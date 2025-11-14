using System.Text;
using SS.Data;
using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data.Extensions;

public static class ApartmentExtensions
{
    public static ApartmentEntity ToEntity(this ApartmentModel model)
    {
        return new ApartmentEntity()
        {
            Id = model.Id,
            Price = model.PricePerMonth,
            Area = model.ParseArea(),
            Floor = model.ParseFloor(),
            MaxFloor = model.ParseMaxFloor(),
            Url = model.Link,
            Region = model.Region,
            Rooms = model.ParseRooms(),
            Series = model.Series
        };
    }

    // Escape special MarkdownV2 characters
    public static string Escape(this string input) => input?
        .Replace("\\", "\\\\")
        .Replace("_", "\\_")
        .Replace("*", "\\*")
        .Replace("[", "\\[")
        .Replace("]", "\\]")
        .Replace("(", "\\(")
        .Replace(")", "\\)")
        .Replace("~", "\\~")
        .Replace("`", "\\`")
        .Replace(">", "\\>")
        .Replace("#", "\\#")
        .Replace("+", "\\+")
        .Replace("-", "\\-")
        .Replace("=", "\\=")
        .Replace("|", "\\|")
        .Replace("{", "\\{")
        .Replace("}", "\\}")
        .Replace(".", "\\.")
        .Replace("!", "\\!") ?? string.Empty;

    public static string ToTelegramString(this ApartmentEntity entity)
    {
        string link = entity.Url;
        //var region = Escape(entity.Region);
        string series = string.IsNullOrWhiteSpace(entity.Series) ? "none" : Escape(entity.Series);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"💶 *Price:* {entity.Price:N0} €");
        sb.AppendLine($"📐 *Area:* {entity.Area} m²");
        sb.AppendLine($"💰 *Price/m:* {entity.Price / entity.Area:N0} €");
        sb.AppendLine();
        sb.AppendLine($"🛏 *Rooms:* {entity.Rooms}");
        sb.AppendLine($"🏢 *Floor:*  {entity.Floor} / {entity.MaxFloor}");
        sb.AppendLine($"🏗 *Series:* {series}");
        sb.AppendLine();
        sb.AppendLine($"✅ [{entity.Id}]({link})");

        return sb.ToString();
    }
}