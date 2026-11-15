using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TableView3D.Models;

namespace TableView3D.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);   // keep this line, Identity needs it

        builder.Entity<RestaurantTable>().HasIndex(t => t.Number).IsUnique();
        builder.Entity<Reservation>().HasIndex(r => r.ReferenceCode).IsUnique();
    }
}