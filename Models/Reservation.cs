namespace TableView3D.Models;

public class Reservation
{
    public int Id { get; set; }
    public string ReferenceCode { get; set; } = "";

    public int RestaurantTableId { get; set; }          // links to the table booked
    public RestaurantTable? RestaurantTable { get; set; }

    public string? UserId { get; set; }                 // we connect this to login in Phase 3
    public string GuestName { get; set; } = "";
    public string GuestPhone { get; set; } = "";

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int PartySize { get; set; }
    public string? SpecialRequests { get; set; }
    public string Status { get; set; } = "Confirmed";   // Confirmed, Cancelled, Completed
}