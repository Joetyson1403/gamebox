using Microsoft.EntityFrameworkCore;
// creer les modèles (User, Game) plus tard dans un dossier Models
// using gamebox.Models; 

namespace gamebox.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // lister les tables plus tard, par exemple :
    // public DbSet<Game> Games => Set<Game>();
}