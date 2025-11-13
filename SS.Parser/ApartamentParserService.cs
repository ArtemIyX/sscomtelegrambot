using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace SS.Parser;

using SS.Data;

public interface IApartmentParserService
{
    public Task<List<ApartmentModel>> ParseApartmentsAsync(string htmlContent,
        CancellationToken cancellationToken = default);

    public Task<List<string>> ParseApartmentPhotoAsync(string htmlContent, CancellationToken cancellationToken = default);
}

public class ApartmentParserService : IApartmentParserService
{
    public Task<List<string>> ParseApartmentPhotoAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        // Run parsing off the main thread to avoid blocking
        return Task.Run(() =>
        {
            var photoUrls = new List<string>();
        
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
        
            // Find all divs with class "pic_dv_thumbnail"
            var thumbnailDivs = htmlDoc.DocumentNode.SelectNodes("//div[@class='pic_dv_thumbnail']");
        
            if (thumbnailDivs != null)
            {
                foreach (var div in thumbnailDivs)
                {
                    // Find the <a> tag within each div and get its href attribute
                    var anchorNode = div.SelectSingleNode(".//a[@href]");
                
                    if (anchorNode != null)
                    {
                        var href = anchorNode.GetAttributeValue("href", string.Empty);
                    
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            photoUrls.Add(href);
                        }
                    }
                }
            }
        
            return photoUrls;

        }, cancellationToken);
    }
    public Task<List<ApartmentModel>> ParseApartmentsAsync(string htmlContent,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var apartments = new List<ApartmentModel>();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            cancellationToken.ThrowIfCancellationRequested();

            // Select all tr elements, filtering out header and banner rows
            var rows = doc.DocumentNode.SelectNodes(
                "//tr[starts-with(@id, 'tr_') and not(@id='head_line') and not(starts-with(@id, 'tr_bnr'))]");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var tds = row.SelectNodes("td");
                    if (tds == null || tds.Count < 9) continue;

                    // Extract link from the description td (3rd td, index 2)
                    var linkA = tds[2].SelectSingleNode(".//a[@class='am']");
                    string link = linkA?.GetAttributeValue("href", "") ?? "";
                    if (!string.IsNullOrEmpty(link))
                    {
                        link = "https://www.ss.lv" + link;
                    }

                    // Extract region from URL
                    string region = "";
                    try
                    {
                        var match = Regex.Match(link, @"/flats/riga/([^/]+)/");
                        if (match.Success)
                        {
                            region = match.Groups[1].Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting region from URL: {ex.Message}");
                    }

                    // Extract rooms (5th td, index 4, labeled "R.")
                    string rooms = tds[4].InnerText.Trim();

                    // Extract area (6th td, index 5, labeled "m²")
                    string areaText = tds[5].InnerText.Trim();
                    string area = areaText;
                    decimal? areaValue = null;
                    try
                    {
                        var areaMatch = Regex.Match(areaText, @"(\d+\.?\d*)");
                        if (areaMatch.Success && decimal.TryParse(areaMatch.Groups[1].Value, out decimal a))
                        {
                            areaValue = a;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing area: {ex.Message}");
                    }

                    // Extract floor (7th td, index 6)
                    string floor = tds[6].InnerText.Trim();

                    // Extract series (8th td, index 7)
                    string series = tds[7].InnerText.Trim();

                    // Extract price (9th td, index 8)
                    string priceText = tds[8].InnerText.Trim();
                    decimal price = 0.0m;
                    if (priceText.Contains("€/mon."))
                    {
                        try
                        {
                            var pricePart = priceText.Split('€')[0].Trim();
                            var priceStr = pricePart.Replace(" ", "").Replace(",", "");
                            if (decimal.TryParse(priceStr, out decimal p))
                            {
                                price = p;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing price: {ex.Message}");
                        }
                    }
                    else
                    {
                        continue; // Skip if not a monthly price
                    }

                    apartments.Add(new ApartmentModel(region, rooms, area, floor, series, price, link));
                }
            }

            return apartments;
        }, cancellationToken);
    }
}