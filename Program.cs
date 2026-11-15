using Microsoft.AspNetCore.Identity;
using TableView3D.Models;
using Microsoft.EntityFrameworkCore;
using TableView3D.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.RestaurantTables.Any())
    {
        db.RestaurantTables.AddRange(
            new RestaurantTable { Number = 1, Seats = 2, Location = "Window" },
            new RestaurantTable { Number = 2, Seats = 2, Location = "Window" },
            new RestaurantTable { Number = 3, Seats = 4, Location = "Indoor" },
            new RestaurantTable { Number = 4, Seats = 4, Location = "Indoor" },
            new RestaurantTable { Number = 5, Seats = 6, Location = "Indoor" },
            new RestaurantTable { Number = 6, Seats = 4, Location = "Patio" },
            new RestaurantTable { Number = 7, Seats = 8, Location = "Patio" }
        );
        db.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var role in new[] { "Admin", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = app.Configuration["Seed:AdminEmail"];
    var adminPassword = app.Configuration["Seed:AdminPassword"];

    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword)
        && await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var created = await userManager.CreateAsync(admin, adminPassword);
        if (created.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
