using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gamebox.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberGameStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberPlayedGames");

            migrationBuilder.DropTable(
                name: "MemberToPlayGames");

            migrationBuilder.CreateTable(
                name: "MemberGameStatuses",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberGameStatuses", x => new { x.MemberId, x.GameId });
                    table.ForeignKey(
                        name: "FK_MemberGameStatuses_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberGameStatuses_Users_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberGameStatuses_GameId",
                table: "MemberGameStatuses",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberGameStatuses");

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

            migrationBuilder.CreateIndex(
                name: "IX_MemberPlayedGames_PlayedGamesId",
                table: "MemberPlayedGames",
                column: "PlayedGamesId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberToPlayGames_ToPlayId",
                table: "MemberToPlayGames",
                column: "ToPlayId");
        }
    }
}
