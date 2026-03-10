using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class FixtureAutomatico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS so this is safe to run even if columns already exist
            migrationBuilder.Sql(@"
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS descripcioncompleta text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS direccionsede text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS frase1 text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS frase2 text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS frase3 text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS imagenesgaleria text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS imagenurl text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS logourl text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS patrocinadores text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS ubicacion text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS urlgrupowhatsapp text;
                ALTER TABLE public.torneos ADD COLUMN IF NOT EXISTS urlreglamento text;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "descripcioncompleta",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "direccionsede",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "frase1",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "frase2",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "frase3",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "imagenesgaleria",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "imagenurl",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "logourl",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "patrocinadores",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "ubicacion",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "urlgrupowhatsapp",
                schema: "public",
                table: "torneos");

            migrationBuilder.DropColumn(
                name: "urlreglamento",
                schema: "public",
                table: "torneos");
        }
    }
}
