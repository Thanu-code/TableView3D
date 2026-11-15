using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableView3D.Data;

namespace TableView3D.Controllers;

public class TablesController : Controller
{
    private readonly ApplicationDbContext _db;

    public TablesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var tables = await _db.RestaurantTables
            .Where(t => t.IsActive)
            .OrderBy(t => t.Number)
            .ToListAsync();

        return View(tables);
    }
}