using Microsoft.Extensions.Logging;
using SS.Data;
using SS.Parser;

namespace SS.Telegram.Services;

public class SSComService : IAsyncDisposable
{
    private readonly ILogger<SSComService> _logger;
    private readonly IWebFetcherService _webFetcherService;
    private ApartmentContainer? _apartmentContainer = null;
    private CancellationTokenSource? _cts = null;
    public bool IsFetching { get; private set; } = false;
    public bool IsApartmentContainerInitialized => _apartmentContainer is not null;
    public int NumFlats => _apartmentContainer is null ? 0 : _apartmentContainer.Map.Count;

    public SSComService(ILogger<SSComService> logger,
        IWebFetcherService webFetcherService)
    {
        _logger = logger;
        _webFetcherService = webFetcherService;
    }

    public IDictionary<string, IOrderedEnumerable<ApartmentModel>> Filter(ApartmentFilter filter)
    {
        if (_apartmentContainer is null)
        {
            throw new NullReferenceException("You should refresh the apartment container before any filters");
        }

        return _apartmentContainer.Filter(filter);
    }

    public async Task RefreshAsync()
    {
        if (IsFetching)
        {
            throw new Exception("Service is already fetching flats!");
        }

        IsFetching = true;
        try
        {
            _logger.LogInformation("Starting web fetcher");
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            ApartmentContainer res = await _webFetcherService.FetchApartments(cancellationToken: token);
            _logger.LogInformation("Web fetcher finished, found {num} flats in RIGA", res.Map.Count);
            _apartmentContainer = res;
        }
        catch (Exception ex)
        {
            _cts = null;
            IsFetching = false;
            throw new Exception($"Fetching failed: {ex.Message}");
        }
        finally
        {
            _cts = null;
            IsFetching = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is not null)
        {
            if (!_cts.IsCancellationRequested)
            {
                await _cts.CancelAsync();
            }

            _cts.Dispose();
            _cts = null;
            GC.SuppressFinalize(this);
        }
    }
}