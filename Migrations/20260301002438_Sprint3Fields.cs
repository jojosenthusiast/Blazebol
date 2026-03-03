using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add esinvitacion column (was missing from DB despite being in snapshot)
            migrationBuilder.Sql(@"
                ALTER TABLE public.solicitudesjugadorequipo
                ADD COLUMN IF NOT EXISTS esinvitacion boolean NOT NULL DEFAULT false;
            ");

            // Add abreviacion column to equipos
            migrationBuilder.Sql(@"
                ALTER TABLE public.equipos
                ADD COLUMN IF NOT EXISTS abreviacion character varying(3);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.solicitudesjugadorequipo
                DROP COLUMN IF EXISTS esinvitacion;
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE public.equipos
                DROP COLUMN IF EXISTS abreviacion;
            ");
        }
    }
}
