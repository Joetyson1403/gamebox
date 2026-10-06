using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using gamebox.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration de la base de données SQLite via Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Enregistrement des services MVC (Contrôleurs et Vues Razor)
builder.Services.AddControllersWithViews();

// 3. Configuration de l'authentification par cookies de session
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";                // Redirection si l'utilisateur n'est pas connecté
        options.AccessDeniedPath = "/Auth/Login";         // Redirection si l'accès est refusé
        options.ExpireTimeSpan = TimeSpan.FromDays(14);   // Durée de validité du cookie (14 jours)
        options.SlidingExpiration = true;                 // Renouvellement glissant tant que l'utilisateur est actif
    });

// 4. Injection de dépendance du service HTTP pour l'API RAWG
builder.Services.AddHttpClient<gamebox.Services.IGameApiService, gamebox.Services.GameApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);           // Timeout de 10 secondes pour éviter les blocages réseau
});

var app = builder.Build();

// 5. Configuration du pipeline de requêtes HTTP (Middlewares)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 6. Activation de l'authentification et de l'autorisation
app.UseAuthentication();
app.UseAuthorization();

// 7. Fichiers statiques et routage par défaut vers HomeController -> Index
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Démarrage du serveur web
app.Run();

