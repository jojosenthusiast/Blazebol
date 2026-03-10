using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class AddFixtureColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fase",
                schema: "public",
                table: "partidos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "jornada",
                schema: "public",
                table: "partidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "camponeutral",
                schema: "public",
                table: "configuracionestorneo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "clasificanporgrupo",
                schema: "public",
                table: "configuracionestorneo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "equilibriolocalvisitante",
                schema: "public",
                table: "configuracionestorneo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "equiposporgrupo",
                schema: "public",
                table: "configuracionestorneo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "idayvuelta",
                schema: "public",
                table: "configuracionestorneo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "numerogrupos",
                schema: "public",
                table: "configuracionestorneo",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fase",
                schema: "public",
                table: "partidos");

            migrationBuilder.DropColumn(
                name: "jornada",
                schema: "public",
                table: "partidos");

            migrationBuilder.DropColumn(
                name: "camponeutral",
                schema: "public",
                table: "configuracionestorneo");

            migrationBuilder.DropColumn(
                name: "clasificanporgrupo",
                schema: "public",
                table: "configuracionestorneo");

            migrationBuilder.DropColumn(
                name: "equilibriolocalvisitante",
                schema: "public",
                table: "configuracionestorneo");

            migrationBuilder.DropColumn(
                name: "equiposporgrupo",
                schema: "public",
                table: "configuracionestorneo");

            migrationBuilder.DropColumn(
                name: "idayvuelta",
                schema: "public",
                table: "configuracionestorneo");

            migrationBuilder.DropColumn(
                name: "numerogrupos",
                schema: "public",
                table: "configuracionestorneo");
        }
    }
}
