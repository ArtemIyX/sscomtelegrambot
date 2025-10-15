namespace SS.Data;

public class ApartmentFilter
{
    public decimal MinPrice { get; set; } = 0;
    
    public decimal? MaxPrice { get; set; } = null;
    
    public List<int>? Rooms { get; set; } = null;
    
    public decimal MinSquare { get; set; } = 0;
    
    public decimal? MaxSquare { get; set; } = null;
    
    public List<string>? Regions { get; set; } = null;
}