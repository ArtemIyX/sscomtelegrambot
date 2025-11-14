using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SS.Data;
using SS.Notifier.Data.Entity;
using SS.Notifier.Data.Extensions;
using SS.Notifier.Data.Models;
using SS.Notifier.Data.Repository;
using Telegram.Bot.Types;

namespace SS.Notifier.Services;

public interface IApartmentRegistryService
{
    public Task<ApartmentResult> UpdateAsync(List<ApartmentModel> apartmentModels,
        CancellationToken cancellationToken = default);
}

public class ApartmentRegistryRegistryService(
    ILogger<ApartmentRegistryRegistryService> logger,
    IRepository<ApartmentEntity, string> apartmentRepository)
    : IApartmentRegistryService
{
    public async Task<ApartmentResult> UpdateAsync(List<ApartmentModel> apartmentModels,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await apartmentRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            List<ApartmentEntity> result = new List<ApartmentEntity>();
        
            // Process each incoming entity
            foreach (ApartmentModel model in apartmentModels)
            {
                ApartmentEntity? entityToUpdate = await apartmentRepository.GetByIdAsync(model.Id, cancellationToken);
                if (entityToUpdate != null)
                {
                    // Update existing entity
                    ApartmentEntity tempEntity = model.ToEntity();
                    tempEntity.CopyTo(entityToUpdate);
                    apartmentRepository.Update(entityToUpdate);
                    logger.LogInformation("Updated apartment with ID: {Id}", entityToUpdate.Id);
                }
                else
                {
                    // Add new entity
                    ApartmentEntity tempEntity = model.ToEntity();
                    ApartmentEntity tempResult = await apartmentRepository.AddAsync(tempEntity, cancellationToken);
                    result.Add(tempResult);
                    logger.LogInformation("Added new apartment with ID: {Id}", tempResult.Id);
                }
            }
            HashSet<string> incomingIds = apartmentModels.Select(e => e.Id).ToHashSet();
            var entitiesToDelete = await apartmentRepository.AsQueryable()
                .Where(entity => !incomingIds.Contains(entity.Id))
                .ToListAsync(cancellationToken);
            
            if (entitiesToDelete.Any())
            {
                apartmentRepository.DeleteRange(entitiesToDelete);
                logger.LogInformation("Deleted {Count} outdated apartments.", entitiesToDelete.Count);
            }
            /*// Remove entities that are not in the incoming list (outdated)
            foreach (var existingEntity in existingEntities)
            {
                // Skip existing
                if (incomingIds.Contains(existingEntity.Id)) continue;

                await apartmentRepository.DeleteAsync(existingEntity.Id, cancellationToken);
                logger.LogInformation("Deleted outdated apartment with ID: {Id}", existingEntity.Id);
            }*/

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