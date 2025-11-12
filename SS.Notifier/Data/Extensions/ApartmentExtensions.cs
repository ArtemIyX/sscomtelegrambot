using SS.Data;
using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data.Extensions;

public static class ApartmentExtensions
{
    public static ApartmentEntity ToEntity(this ApartmentModel model)
    {
        return new ApartmentEntity()
        {
            Id = model.Id,
            Price = model.PricePerMonth,
            Area = model.ParseArea(),
            Floor = model.ParseFloor(),
            MaxFloor = model.ParseMaxFloor(),
            Link = model.Link,
            Region = model.Region,
            Rooms = model.ParseRooms(),
            Series = model.Series
        };
    }
}