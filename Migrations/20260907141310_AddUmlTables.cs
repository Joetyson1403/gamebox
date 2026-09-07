using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gamebox.Migrations
{
    /// <inheritdoc />
    public partial class AddUmlTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Studio = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseYear = table.Column<int>(type: "INTEGER", nullable: false),
                    CoverUrl = table.Column<string>(type: "TEXT", nullable: false),
                    AverageRating = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    UserType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    AccessLevel = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomLists_Users_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberFavoriteGames",
                columns: table => new
                {
                    FavoriteGamesId = table.Column<int>(type: "INTEGER", nullable: false),
                    FavoritedById = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberFavoriteGames", x => new { x.FavoriteGamesId, x.FavoritedById });
                    table.ForeignKey(
                        name: "FK_MemberFavoriteGames_Games_FavoriteGamesId",
                        column: x => x.FavoriteGamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberFavoriteGames_Users_FavoritedById",
                        column: x => x.FavoritedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberPlayedGames",
                columns: table => new
                {
                    PlayedById = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayedGamesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPlayedGames", x => new { x.PlayedById, x.PlayedGamesId });
                    table.ForeignKey(
                        name: "FK_MemberPlayedGames_Games_PlayedGamesId",
                        column: x => x.PlayedGamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberPlayedGames_Users_PlayedById",
                        column: x => x.PlayedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberToPlayGames",
                columns: table => new
                {
                    InToPlayOfId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToPlayId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberToPlayGames", x => new { x.InToPlayOfId, x.ToPlayId });
                    table.ForeignKey(
                        name: "FK_MemberToPlayGames_Games_ToPlayId",
                        column: x => x.ToPlayId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberToPlayGames_Users_InToPlayOfId",
                        column: x => x.InToPlayOfId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomListGame",
                columns: table => new
                {
                    CustomListsId = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesInListId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomListGame", x => new { x.CustomListsId, x.GamesInListId });
                    table.ForeignKey(
                        name: "FK_CustomListGame_CustomLists_CustomListsId",
                        column: x => x.CustomListsId,
                        principalTable: "CustomLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomListGame_Games_GamesInListId",
                        column: x => x.GamesInListId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomListGame_GamesInListId",
                table: "CustomListGame",
                column: "GamesInListId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomLists_MemberId",
                table: "CustomLists",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFavoriteGames_FavoritedById",
                table: "MemberFavoriteGames",
                column: "FavoritedById");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPlayedGames_PlayedGamesId",
                table: "MemberPlayedGames",
                column: "PlayedGamesId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberToPlayGames_ToPlayId",
                table: "MemberToPlayGames",
                column: "ToPlayId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GameId",
                table: "Reviews",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MemberId",
                table: "Reviews",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomListGame");

            migrationBuilder.DropTable(
                name: "MemberFavoriteGames");

            migrationBuilder.DropTable(
                name: "MemberPlayedGames");

            migrationBuilder.DropTable(
                name: "MemberToPlayGames");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "CustomLists");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
