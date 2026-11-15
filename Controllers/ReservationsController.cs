using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableView3D.Data;
using TableView3D.Models;

namespace TableView3D.Controllers;

public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private const int DurationHours = 2;   // every booking lasts 2 hours

    public ReservationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Search(DateOnly? date, TimeOnly? time, int partySize = 2)
    {
        var model = new SearchViewModel
        {
            Date = date ?? DateOnly.FromDateTime(DateTime.Today),
            Time = time ?? new TimeOnly(19, 0),
            PartySize = partySize
        };

        // First visit: just show the empty form
        if (date is null || time is null)
            return View(model);

        var start = time.Value;
        var end = start.AddHours(DurationHours);
        var day = date.Value;

        var tables = await _db.RestaurantTables
            .Where(t => t.IsActive)
            .OrderBy(t => t.Number)
            .ToListAsync();

        // Tables that already have a confirmed booking overlapping this time
        var bookedIds = await _db.Reservations
            .Where(r => r.Date == day
                     && r.Status == "Confirmed"
                     && r.StartTime < end
                     && r.EndTime > start)
            .Select(r => r.RestaurantTableId)
            .ToListAsync();

        model.Results = tables.Select(t => new TableAvailability
        {
            Table = t,
            IsBooked = bookedIds.Contains(t.Id),
            FitsParty = t.Seats >= partySize
        }).ToList();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int tableId, DateOnly date, TimeOnly time, int partySize)
    {
        var table = await _db.RestaurantTables.FindAsync(tableId);
        if (table is null || !table.IsActive) return NotFound();

        return View(new BookingViewModel
        {
            TableId = table.Id,
            TableNumber = table.Number,
            Seats = table.Seats,
            Location = table.Location,
            Date = date,
            Time = time,
            PartySize = partySize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingViewModel model)
    {
        var table = await _db.RestaurantTables.FindAsync(model.TableId);
        if (table is null || !table.IsActive) return NotFound();

        // Refill display values (they aren't sent back by the form)
        model.TableNumber = table.Number;
        model.Seats = table.Seats;
        model.Location = table.Location;

        var start = model.Time;
        var end = start.AddHours(DurationHours);

        // Business rules, checked again on the server
        if (model.Date.ToDateTime(model.Time) < DateTime.Now)
            ModelState.AddModelError("", "You can't book a time in the past.");

        if (model.PartySize < 1 || model.PartySize > table.Seats)
            ModelState.AddModelError("", $"This table seats {table.Seats} guests at most.");

        var clash = await _db.Reservations.AnyAsync(r =>
            r.RestaurantTableId == table.Id
            && r.Date == model.Date
            && r.Status == "Confirmed"
            && r.StartTime < end
            && r.EndTime > start);

        if (clash)
            ModelState.AddModelError("", "Sorry, this table was just booked. Please go back and choose another.");

        if (!ModelState.IsValid) return View(model);

        var reservation = new Reservation
        {
            ReferenceCode = NewReferenceCode(),
            RestaurantTableId = table.Id,
            GuestName = model.GuestName,
            GuestPhone = model.GuestPhone,
            Date = model.Date,
            StartTime = start,
            EndTime = end,
            PartySize = model.PartySize,
            SpecialRequests = model.SpecialRequests,
            Status = "Confirmed"
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { id = reservation.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.RestaurantTable)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null) return NotFound();
        return View(reservation);
    }

    private static string NewReferenceCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return "TV-" + new string(Enumerable.Range(0, 5)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}