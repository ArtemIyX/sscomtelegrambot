namespace SS.Data;

public class ApartmentModel
{
    public string Region { get; set; } = string.Empty;
    public string Rooms { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public decimal? PricePerMonth { get; set; }
    public string Link { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"[{Region}] {PricePerMonth}eur {Rooms}r {Area}m2 {Floor} {Series} {Link}";
    }
}