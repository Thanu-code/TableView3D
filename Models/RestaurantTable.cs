namespace TableView3D.Models;

public class RestaurantTable
{
    public int Id { get; set; }
    public int Number { get; set; }          // the number shown on the floor plan
    public int Seats { get; set; }
    public string Location { get; set; } = "Indoor";   // Window, Indoor or Patio
    public bool IsActive { get; set; } = true;          // admin can disable a table

    public List<Reservation> Reservations { get; set; } = new();
}