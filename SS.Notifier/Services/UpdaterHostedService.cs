using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Data;
using SS.Notifier.Data.Entity;
using SS.Notifier.Data.Models;
using SS.Notifier.Data.Settings;
using SS.Parser;

namespace SS.Notifier.Services;

public class UpdaterHostedService(
    ILogger<UpdaterHostedService> logger,
    ITelegramBotService telegramBotService,
    IApartmentRegistryService apartmentRegistryService,
    IWebFetcherService webFetcherService,
    IOptions<AppSettings> appSettings) : IHostedService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting updater...");

        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Processing update...");
                await Update(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update");
            }

            // Wait exactly one hour for the next tick
            await Task.Delay(_interval, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("Updater is shutting down");
        return Task.CompletedTask;
    }

    protected async Task Update(CancellationToken cancellationToken = default)
    {
        try
        {
            AppSettings.ApartmentFilter appFilter = appSettings.Value.Filter;

            logger.LogInformation("Fetching apartments from ss.com, filter:\n{filter}",
                JsonSerializer.Serialize(appFilter, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }));

            ApartmentContainer apartmentContainer = await webFetcherService.FetchApartmentsAsync(
                filter: new SS.Data.ApartmentFilter()
                {
                    MinPrice = appFilter.MinPrice,
                    MaxPrice = appFilter.MaxPrice,
                    MinSquare = appFilter.MinArea,
                    MaxSquare = appFilter.MaxArea,
                    Rooms = appFilter.Rooms,
                    Regions = appSettings.Value.Telegram.Regions,
                }, cancellationToken: cancellationToken);

            logger.LogInformation("Found {num} apartments in RIGA", apartmentContainer.Map.Count);

            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Updating db...");
            ApartmentResult updateList =
                await apartmentRegistryService.UpdateAsync(apartmentContainer.GetAll(), cancellationToken);

            logger.LogInformation("{n} apartments are new", updateList.Container.Count);

            await ProcessApartments(updateList, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Update failed");
        }
    }

    protected async Task ProcessApartments(ApartmentResult updateList, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing {n} apartments...", updateList.Container.Count);

        foreach (KeyValuePair<string, List<ApartmentEntity>> kvp in updateList.Container)
        {
            logger.LogInformation("Processing '{region}' region for {n} apartments", kvp.Key,
                updateList.Container.Count);
            cancellationToken.ThrowIfCancellationRequested();
            string region = kvp.Key;
            List<ApartmentEntity> apartments = kvp.Value;
            for (int i = 0; i < apartments.Count; i++)
            {
                logger.LogInformation("Processing '{region}': {c}/{n}", kvp.Key, i, apartments.Count);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await ProcessNewApartment(region, apartments[i], cancellationToken);
                }
                catch (Exception ex)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };
                    logger.LogError(ex, "Failed to update apartment with id {apartmentId}:\n{apartmentStr}",
                        apartments[i].Id,
                        JsonSerializer.Serialize(apartments[i], options));
                }
            }
        }
    }

    protected async Task ProcessNewApartment(string region, ApartmentEntity entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<string> photos = new List<string>();
        try
        {
            logger.LogInformation("Fetching photos for apartment '{id}' in '{region}: {url}", entity.Id, region,
                entity.Url);
            photos = await webFetcherService.FetchPhotosAsync(entity.Url, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, $"Failed to fetch photos for '{entity.Id}' ({entity.Url})");
        }

        logger.LogInformation("'{id}': {n} photos have been fetched", entity.Id, photos.Count);

        await telegramBotService.SendApartment(entity, photos, cancellationToken);
    }
}