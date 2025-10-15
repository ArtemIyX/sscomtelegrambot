using Microsoft.Extensions.Logging;
using SS.Data;

namespace SS.Parser;

public interface ISsWebFetcherService
{
    public Task<ApartmentContainer> FetchApartments(ApartmentFilter filter,
        CancellationToken cancellationToken = default);
}

public class SsWebFetcherService : ISsWebFetcherService
{
    private readonly ILogger<SsWebFetcherService> _logger;
    private readonly IApartmentParserService _apartmentParserService;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _searchUrl = @"https://www.ss.lv/en/real-estate/flats/riga/all/hand_over/";

    public SsWebFetcherService(ILogger<SsWebFetcherService> logger,
        IApartmentParserService apartmentParserService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _apartmentParserService = apartmentParserService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApartmentContainer> FetchApartments(ApartmentFilter filter,
        CancellationToken cancellationToken = default)
    {
        ApartmentContainer container = new ApartmentContainer();
        int index = 1;
        int firstPargeHash = 0;
        while (true)
        {
            _logger.LogInformation("Trying to fetch apartments, page {page}", index);
            if (cancellationToken.IsCancellationRequested)
                break;
            List<ApartmentModel>? list = await FetchSinglePage(index, cancellationToken);
            if (list == null) break;
            if (list.Count == 0)
            {
                _logger.LogInformation("Page {index} has no flats, finishing", index);
                break;
            }

            if (firstPargeHash == 0)
            {
                firstPargeHash = list.GetCombinedHashCode();
            }
            else
            {
                if (firstPargeHash == list.GetCombinedHashCode())
                {
                    _logger.LogInformation("Page {index} has same flats as first page, finishing", index);
                    break;
                }
            }

            var filtered = list.Where(apartment => apartment.MatchesFilter(filter));
            foreach (var apartment in filtered)
            {
                container.Add(apartment.Id, apartment);
            }

            index++;
        }

        return container;
    }

    private string MakeSearchUrl(int index) => $"{_searchUrl}page{index}.html";

    private async Task<List<ApartmentModel>?> FetchSinglePage(int index, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient($"http_client_{index}");
        string url = MakeSearchUrl(index);
        HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "FetchSinglePage request failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}, URL: {Url}",
                response.StatusCode,
                response.ReasonPhrase,
                errorContent,
                url);
            return null;
        }

        string htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
        List<ApartmentModel> list = await _apartmentParserService.ParseApartmentsAsync(htmlContent, cancellationToken);
        return list;
    }
}