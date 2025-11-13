using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SS.Notifier.Data.Entity;


[Table("Apartments")]
public class ApartmentEntity
{
    [Key]
    [MaxLength(512)]
    public string Id { get; set; } = string.Empty;
    public int Rooms { get; set; }
    public decimal Area { get; set; }
    public int Floor { get; set; }
    public int MaxFloor { get; set; }

    [MaxLength(128)]
    public string Series { get; set; } = string.Empty;

    [Required] [MaxLength(64)] public string Region { get; set; } = string.Empty;

    [Required] [MaxLength(500)] public string Link { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}