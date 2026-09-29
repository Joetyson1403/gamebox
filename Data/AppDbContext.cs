using Microsoft.EntityFrameworkCore;
using gamebox.Models;

namespace gamebox.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<CustomList> CustomLists => Set<CustomList>();
    public DbSet<MemberGameStatus> MemberGameStatuses => Set<MemberGameStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Différencier un Admin d'un Membre dans la même table User
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Member>("Member")
            .HasValue<Administrator>("Administrator");

        // Création de la table de liaison pour les jeux favoris
        modelBuilder.Entity<Member>()
            .HasMany(m => m.FavoriteGames)
            .WithMany(g => g.FavoritedBy)
            .UsingEntity(j => j.ToTable("MemberFavoriteGames"));

        // Création de la table de liaison pour les statuts de jeu
        modelBuilder.Entity<MemberGameStatus>()
            .HasKey(mgs => new { mgs.MemberId, mgs.GameId });

        modelBuilder.Entity<MemberGameStatus>()
            .HasOne(mgs => mgs.Member)
            .WithMany(m => m.GameStatuses)
            .HasForeignKey(mgs => mgs.MemberId);

        modelBuilder.Entity<MemberGameStatus>()
            .HasOne(mgs => mgs.Game)
            .WithMany(g => g.Statuses)
            .HasForeignKey(mgs => mgs.GameId);
    }
}