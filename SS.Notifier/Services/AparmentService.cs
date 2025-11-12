using Microsoft.Extensions.Logging;
using SS.Data;
using SS.Notifier.Data.Entity;
using SS.Notifier.Data.Extensions;
using SS.Notifier.Data.Models;
using SS.Notifier.Data.Repository;

namespace SS.Notifier.Services;

public interface IApartmentService
{
    public Task<ApartmentResult> UpdateAsync(List<ApartmentModel> apartmentModels,
        CancellationToken cancellationToken = default);
}

public class ApartmentService(
    ILogger<ApartmentService> logger,
    IRepository<ApartmentEntity, string> apartmentRepository)
    : IApartmentService
{
    public async Task<ApartmentResult> UpdateAsync(List<ApartmentModel> apartmentModels,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            // Convert models to entities, skipping those that throw exceptions
            List<ApartmentEntity> incomingEntities = new List<ApartmentEntity>();
            foreach (var model in apartmentModels)
            {
                try
                {
                    var entity = model.ToEntity();
                    incomingEntities.Add(entity);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to convert apartment model with ID: {Id}. Skipping.", model.Id);
                }
            }

            HashSet<string> incomingIds = incomingEntities.Select(e => e.Id).ToHashSet();

            // Get all existing entities from database
            List<ApartmentEntity> existingEntities =
                (await apartmentRepository.GetAllAsync(cancellationToken)).ToList();
            HashSet<string> existingIds = existingEntities.Select(e => e.Id).ToHashSet();

            List<ApartmentEntity> result = new List<ApartmentEntity>();

            // Process each incoming entity
            foreach (ApartmentEntity entity in incomingEntities)
            {
                if (existingIds.Contains(entity.Id))
                {
                    // Update existing entity
                    apartmentRepository.Update(entity);
                    logger.LogInformation("Updated apartment with ID: {Id}", entity.Id);
                }
                else
                {
                    // Add new entity
                    ApartmentEntity tempResult = await apartmentRepository.AddAsync(entity, cancellationToken);
                    result.Add(tempResult);
                    logger.LogInformation("Added new apartment with ID: {Id}", entity.Id);
                }
            }

            // Remove entities that are not in the incoming list (outdated)
            foreach (var existingEntity in existingEntities)
            {
                if (incomingIds.Contains(existingEntity.Id)) continue;

                await apartmentRepository.DeleteAsync(existingEntity.Id, cancellationToken);
                logger.LogInformation("Deleted outdated apartment with ID: {Id}", existingEntity.Id);
            }

            // Save all changes
            await apartmentRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new ApartmentResult(result);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}