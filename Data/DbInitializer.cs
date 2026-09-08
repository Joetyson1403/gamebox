using Microsoft.EntityFrameworkCore;
using gamebox.Models;

namespace gamebox.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Créer la base si elle n'existe pas
        context.Database.EnsureCreated();

        // S'assurer que la table UserGameStatuses existe dans SQLite
        EnsureUserGameStatusTable(context);

        // Si aucun membre n'existe, en créer un par défaut
        var member = context.Members.FirstOrDefault();
        if (member == null)
        {
            member = new Member
            {
                Username = "Alex",
                Email = "alex@gamebox.fr",
                Password = "password123"
            };
            context.Members.Add(member);
            context.SaveChanges();
        }

        // Si aucun jeu n'existe, en créer quelques-uns pour tester
        if (!context.Games.Any())
        {
            var games = new List<Game>
            {
                new Game
                {
                    Title = "Baldur's Gate 3",
                    Studio = "Larian Studios",
                    ReleaseYear = 2023,
                    AverageRating = 4.9,
                    CoverUrl = "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=500&auto=format&fit=crop&q=80",
                    Description = "Un jeu de rôle de nouvelle génération se déroulant dans l'univers de Donjons & Dragons. Rassemblez votre groupe et retournez dans les Royaumes Oubliés."
                },
                new Game
                {
                    Title = "Elden Ring",
                    Studio = "FromSoftware",
                    ReleaseYear = 2022,
                    AverageRating = 4.8,
                    CoverUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=500&auto=format&fit=crop&q=80",
                    Description = "Levez-vous, Sans-éclat, et puisse la grâce guider vos pas. Brandissez la puissance du Cercle d'Elden et devenez le Seigneur d'Elden dans l'Entre-terre."
                },
                new Game
                {
                    Title = "Cyberpunk 2077",
                    Studio = "CD Projekt Red",
                    ReleaseYear = 2020,
                    AverageRating = 4.5,
                    CoverUrl = "https://images.unsplash.com/photo-1542751110-97427bbecf20?w=500&auto=format&fit=crop&q=80",
                    Description = "Un RPG d'action et d'aventure en monde ouvert qui se déroule à Night City, une mégalopole obsédée par le pouvoir, la séduction et les modifications corporelles."
                },
                new Game
                {
                    Title = "The Witcher 3: Wild Hunt",
                    Studio = "CD Projekt Red",
                    ReleaseYear = 2015,
                    AverageRating = 4.9,
                    CoverUrl = "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=500&auto=format&fit=crop&q=80",
                    Description = "Incarnez Geralt de Riv, un chasseur de monstres mercenaire, et partez à la recherche de l'enfant de la prophétie dans un monde ouvert fantastique ravagé par la guerre."
                }
            };

            context.Games.AddRange(games);
            context.SaveChanges();
        }
    }

    private static void EnsureUserGameStatusTable(AppDbContext context)
    {
        var conn = context.Database.GetDbConnection();
        conn.Open();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserGameStatuses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MemberId INTEGER NOT NULL,
                    GameId INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    CONSTRAINT FK_UserGameStatuses_Users_MemberId FOREIGN KEY (MemberId) REFERENCES Users (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_UserGameStatuses_Games_GameId FOREIGN KEY (GameId) REFERENCES Games (Id) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS IX_UserGameStatuses_MemberId_GameId ON UserGameStatuses (MemberId, GameId);
            ";
            cmd.ExecuteNonQuery();
        }
        finally
        {
            conn.Close();
        }
    }
}
