using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class AddPartidoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partidos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    grupoid = table.Column<int>(type: "integer", nullable: true),
                    equipolocalid = table.Column<int>(type: "integer", nullable: false),
                    equipovisitanteid = table.Column<int>(type: "integer", nullable: false),
                    arbitroid = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sede = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    goleslocal = table.Column<int>(type: "integer", nullable: true),
                    golesvisitante = table.Column<int>(type: "integer", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_partidos", x => x.id);
                    table.ForeignKey(
                        name: "fk_partidos_arbitros_arbitroid",
                        column: x => x.arbitroid,
                        principalSchema: "public",
                        principalTable: "arbitros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_partidos_equipos_equipolocalid",
                        column: x => x.equipolocalid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_partidos_equipos_equipovisitanteid",
                        column: x => x.equipovisitanteid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_partidos_grupos_grupoid",
                        column: x => x.grupoid,
                        principalSchema: "public",
                        principalTable: "grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_partidos_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_partidos_arbitroid",
                schema: "public",
                table: "partidos",
                column: "arbitroid");

            migrationBuilder.CreateIndex(
                name: "ix_partidos_equipolocalid",
                schema: "public",
                table: "partidos",
                column: "equipolocalid");

            migrationBuilder.CreateIndex(
                name: "ix_partidos_equipovisitanteid",
                schema: "public",
                table: "partidos",
                column: "equipovisitanteid");

            migrationBuilder.CreateIndex(
                name: "ix_partidos_grupoid",
                schema: "public",
                table: "partidos",
                column: "grupoid");

            migrationBuilder.CreateIndex(
                name: "IX_partidos_torneoid_fecha",
                schema: "public",
                table: "partidos",
                columns: new[] { "torneoid", "fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partidos",
                schema: "public");
        }
    }
}
