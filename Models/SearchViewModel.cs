namespace TableView3D.Models;

public class SearchViewModel
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int PartySize { get; set; } = 2;
    public List<TableAvailability>? Results { get; set; }
}

public class TableAvailability
{
    public RestaurantTable Table { get; set; } = null!;
    public bool IsBooked { get; set; }
    public bool FitsParty { get; set; }
    public bool IsAvailable => !IsBooked && FitsParty;
}