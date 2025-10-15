// See https://aka.ms/new-console-template for more information

using SS.Parser;

Console.WriteLine("Hello, World!");
using var httpClient = new HttpClient();
var response =
    await httpClient.GetAsync(@"https://www.ss.com/en/real-estate/flats/riga/all/hand_over/filter/photo/page2.html");

var htmlText = await response.Content.ReadAsStringAsync();
ApartmentParserService service = new ApartmentParserService();
var list = service.ParseApartments(htmlText);
list.ForEach(Console.WriteLine);