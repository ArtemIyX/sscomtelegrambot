using Microsoft.Extensions.Logging;
using SS.Data;

namespace SS.Parser;

public interface IWebFetcherService
{
    public Task<ApartmentContainer> FetchApartments(ApartmentFilter? filter = null,
        CancellationToken cancellationToken = default);
}

public class WebFetcherService : IWebFetcherService
{
    private readonly ILogger<WebFetcherService> _logger;
    private readonly IApartmentParserService _apartmentParserService;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _searchUrl = @"https://www.ss.lv/en/real-estate/flats/riga/all/hand_over/";

    public WebFetcherService(ILogger<WebFetcherService> logger,
        IApartmentParserService apartmentParserService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _apartmentParserService = apartmentParserService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApartmentContainer> FetchApartments(ApartmentFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var container = new ApartmentContainer(); // Assume this is thread-safe or wrap in ConcurrentDictionary

        const int concurrencyLevel = 10;
        var semaphore = new SemaphoreSlim(concurrencyLevel);

        // Step 1: Fetch page 1 sequentially
        _logger.LogInformation("Fetching page 1 sequentially");
        var firstPageList = await FetchSinglePage(1, cancellationToken);
        if (firstPageList == null || firstPageList.Count == 0)
        {
            _logger.LogInformation("Page 1 failed or empty, finishing");
            return container;
        }

        int firstPageHash = firstPageList.GetCombinedHashCode();
        var filteredFirst = (filter is null)
            ? firstPageList
            : firstPageList.Where(apartment => apartment.MatchesFilter(filter));
        foreach (var apartment in filteredFirst)
        {
            container.Add(apartment.Id, apartment);
        }

        // Step 2: Parallel fetch subsequent pages in batches
        int batchStart = 2;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var tasks = new List<Task<(int Page, List<ApartmentModel>? List)>>();
            var batchSize = 0;
            for (int page = batchStart; page < batchStart + concurrencyLevel; page++)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await semaphore.WaitAsync(cancellationToken); // Limit concurrency
                batchSize++;
                var currentPage = page; // Capture for lambda

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var list = await FetchSinglePage(currentPage, cancellationToken);
                        return (currentPage, list);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            if (batchSize == 0) break;

            var results = await Task.WhenAll(tasks);
            bool stop = false;

            // Process batch results sequentially to enforce order and stopping
            foreach (var (page, list) in results.OrderBy(r => r.Page))
            {
                if (list == null)
                {
                    _logger.LogWarning("Page {page} failed, skipping", page);
                    continue; // Or treat as stop?
                }

                if (list.Count == 0)
                {
                    _logger.LogInformation("Page {page} has no flats, finishing", page);
                    stop = true;
                    break;
                }

                if (list.GetCombinedHashCode() == firstPageHash)
                {
                    _logger.LogInformation("Page {page} has same flats as first page, finishing", page);
                    stop = true;
                    break;
                }

                var filtered = filter is null ? list : list.Where(apartment => apartment.MatchesFilter(filter));
                foreach (var apartment in filtered)
                {
                    container.Add(apartment.Id, apartment);
                }
            }

            if (stop) break;

            batchStart += concurrencyLevel;
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