using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class LandingPageTorneoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "abreviacion",
                schema: "public",
                table: "equipos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "abreviacion",
                schema: "public",
                table: "equipos",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
