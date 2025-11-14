using Microsoft.Extensions.Logging;
using SS.Data;
using SS.Parser;

namespace SS.Telegram.Services;

public class SSComService : IAsyncDisposable
{
    private readonly ILogger<SSComService> _logger;
    private readonly IWebFetcherService _webFetcherService;
    private ApartmentContainer? _lastApartmentContainer = null;
    private ApartmentContainer? _currentApartmentContainer = null;
    private CancellationTokenSource? _cts = null;
    public bool IsFetching { get; private set; } = false;
    public bool IsApartmentContainerInitialized => _currentApartmentContainer is not null;
    public int NumFlats => _currentApartmentContainer is null ? 0 : _currentApartmentContainer.Map.Count;

    public bool IsNew(string id)
    {
        if (_lastApartmentContainer is null)
            return true;

        return !_lastApartmentContainer.Contains(id);
    }

    public SSComService(ILogger<SSComService> logger,
        IWebFetcherService webFetcherService)
    {
        _logger = logger;
        _webFetcherService = webFetcherService;
    }

    public IDictionary<string, IOrderedEnumerable<ApartmentModel>> Filter(ApartmentFilter filter)
    {
        if (_currentApartmentContainer is null)
        {
            throw new NullReferenceException("You should refresh the apartment container before any filters");
        }

        return _currentApartmentContainer.Filter(filter);
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
            ApartmentContainer res = await _webFetcherService.FetchApartmentsAsync(cancellationToken: token);
            _logger.LogInformation("Web fetcher finished, found {num} flats in RIGA", res.Map.Count);
            _lastApartmentContainer = _currentApartmentContainer;
            _currentApartmentContainer = res;
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