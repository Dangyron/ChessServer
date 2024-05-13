using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedSomeTypos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Moves",
                table: "Games",
                newName: "Pgn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pgn",
                table: "Games",
                newName: "Moves");
        }
    }
}
