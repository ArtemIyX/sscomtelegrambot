using Microsoft.EntityFrameworkCore;
using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data.Repository;

public interface IApartmentRepository : IRepository<ApartmentEntity, string>
{
    Task<IEnumerable<ApartmentEntity>> GetByRegionAsync(string region);
    Task<IEnumerable<ApartmentEntity>> GetByRoomsAsync(int rooms);

    Task<IEnumerable<ApartmentEntity>> SearchAsync(string region,
        int? minRooms = null,
        int? maxRooms = null,
        decimal? minArea = null,
        decimal? maxArea = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);
}

// ApartmentRepository.cs - Implementation
public class ApartmentRepository(NotifierDbContext context)
    : Repository<ApartmentEntity, string>(context), IApartmentRepository
{
    public async Task<IEnumerable<ApartmentEntity>> GetByRegionAsync(string region)
    {
        return await DbSet
            .Where(a => a.Region == region)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApartmentEntity>> GetByRoomsAsync(int rooms)
    {
        return await DbSet
            .Where(a => a.Rooms == rooms)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApartmentEntity>> SearchAsync(string region,
        int? minRooms = null,
        int? maxRooms = null,
        decimal? minArea = null,
        decimal? maxArea = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        IQueryable<ApartmentEntity> query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(region))
            query = query.Where(a => a.Region == region);

        if (minRooms.HasValue)
            query = query.Where(a => a.Rooms >= minRooms.Value);

        if (maxRooms.HasValue)
            query = query.Where(a => a.Rooms <= maxRooms.Value);

        if (minArea.HasValue)
            query = query.Where(a => a.Area >= minArea.Value);

        if (maxArea.HasValue)
            query = query.Where(a => a.Rooms <= maxArea.Value);

        if (minPrice.HasValue)
            query = query.Where(a => a.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(a => a.Price <= maxPrice.Value);

        return await query.ToListAsync();
    }
}