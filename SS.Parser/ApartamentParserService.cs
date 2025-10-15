using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace SS.Parser;

using SS.Data;

public class ApartmentParserService
{
    public List<ApartmentModel> ParseApartments(string htmlContent)
    {
        var apartments = new List<ApartmentModel>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Find all apartment listings (div with class d8 or d8p)
        var apartmentNodes =
            doc.DocumentNode.SelectNodes("//div[contains(@class, 'd8') and not(contains(@class, 'd8p'))]");

        if (apartmentNodes.Count == 0)
            return apartments;

        foreach (var node in apartmentNodes)
        {
            try
            {
                var apartment = ParseApartmentNode(node);

                // Only add if price is per month (skip daily rates)
                if (apartment.PricePerMonth.HasValue)
                {
                    apartments.Add(apartment);
                }
            }
            catch (Exception ex)
            {
                // Log error and continue with next apartment
                Console.WriteLine($"Error parsing apartment: {ex.Message}");
            }
        }

        return apartments;
    }

    private ApartmentModel ParseApartmentNode(HtmlNode node)
    {
        var apartment = new ApartmentModel();

        // Extract Region
        var regionNode = node.SelectSingleNode(".//div[@class='d5']//td[@opt='1']");
        apartment.Region = regionNode?.InnerText.Trim();

        // Extract Link
        HtmlNode linkNode = node.SelectSingleNode(".//a[contains(@class, 'am4')]");
        if (linkNode is not null)
        {
            apartment.Link = linkNode.GetAttributeValue("href", "");
        }

        // Extract properties (Rooms, Area, Floor, Series)
        var propertyRows = node.SelectNodes(".//div[@class='d11']//tr");
        if (propertyRows != null)
        {
            foreach (var row in propertyRows)
            {
                var labelNode = row.SelectSingleNode("./td[@class='td1812']");
                var valueNode = row.SelectSingleNode("./td[@opt='1']");

                if (labelNode == null || valueNode == null)
                    continue;

                var label = labelNode.InnerText.Trim().TrimEnd(':');
                var value = valueNode.InnerText.Trim();

                switch (label)
                {
                    case "R":
                    case "R.":
                        apartment.Rooms = value;
                        break;
                    case "m²":
                    case "mÂ²":
                        apartment.Area = value;
                        break;
                    case "Floor":
                        apartment.Floor = value;
                        break;
                    case "Series":
                        apartment.Series = value;
                        break;
                }
            }
        }

        // Extract Price
        var priceNode = node.SelectSingleNode(".//div[@class='d10']");
        if (priceNode != null)
        {
            var priceText = priceNode.InnerText.Trim();
            apartment.PricePerMonth = ParsePrice(priceText);
        }

        return apartment;
    }

    private decimal? ParsePrice(string priceText)
    {
        // Skip daily rates
        if (priceText.Contains("/day"))
            return null;

        // Extract price per month
        if (priceText.Contains("/mon"))
        {
            // Remove "/mon." and currency symbol
            var priceMatch = Regex.Match(priceText, @"([\d,]+)\s*€/mon");

            if (priceMatch.Success)
            {
                // Remove thousand separator (comma) and parse
                var priceString = priceMatch.Groups[1].Value.Replace(",", "");

                if (decimal.TryParse(priceString, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
                {
                    return price;
                }
            }
        }

        return null;
    }
}