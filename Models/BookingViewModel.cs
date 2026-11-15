using System.ComponentModel.DataAnnotations;

namespace TableView3D.Models;

public class BookingViewModel
{
    // Carried from the search (not typed by the user)
    public int TableId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public int PartySize { get; set; }

    // Only for display on the page
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public string Location { get; set; } = "";

    // Typed by the user
    [Required, StringLength(100)]
    public string GuestName { get; set; } = "";

    [Required, Phone, StringLength(20)]
    public string GuestPhone { get; set; } = "";

    [StringLength(300)]
    public string? SpecialRequests { get; set; }
}