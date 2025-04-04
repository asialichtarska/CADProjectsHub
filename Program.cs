using CADProjectsHub.Data;
using CADProjectsHub.Helpers;
using CADProjectsHub.Models.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//Wczytanie konfiguracji CryptoSettings z appsettings.json
builder.Services.Configure<CryptoSettings>(
    builder.Configuration.GetSection("CryptoSettings")
);

// Rejestracja bazy danych dla użytkowników (IdentityDB)
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Rejestracja bazy danych dla projektów CAD (CADProjectsDB)
builder.Services.AddDbContext<CADProjectsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectsConnection")));

// Konfiguracja Identity dla użytkowników
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

// Konfiguracja zabezpieczeń haseł
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
});

// Konfiguracja systemu logowania i autoryzacji
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.Configure<CryptoSettings>(builder.Configuration.GetSection("CryptoSettings"));

// Przesyłanie plików
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // Maksymalny rozmiar pliku
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Inicjalizacja bazy danych z danymi testowymi
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var projectsContext = services.GetRequiredService<CADProjectsContext>();
        var identityContext = services.GetRequiredService<IdentityContext>();

        projectsContext.Database.Migrate();
        identityContext.Database.Migrate();

        DbInitializer.Initialize(projectsContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error.");
    }
}

// Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
