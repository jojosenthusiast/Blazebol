using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_5_ConvocatoriasReportesNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "genero",
                schema: "public",
                table: "personas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fechainicioreal",
                schema: "public",
                table: "partidos",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "minutoactual",
                schema: "public",
                table: "partidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razonreprogramacion",
                schema: "public",
                table: "partidos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipoequipo",
                schema: "public",
                table: "equipos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Libre");

            migrationBuilder.CreateTable(
                name: "convocatorias",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partidoid = table.Column<int>(type: "integer", nullable: false),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    entrenadorid = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fechapublicacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convocatorias", x => x.id);
                    table.ForeignKey(
                        name: "fk_convocatorias_entrenadores_entrenadorid",
                        column: x => x.entrenadorid,
                        principalSchema: "public",
                        principalTable: "entrenadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_convocatorias_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_convocatorias_partidos_partidoid",
                        column: x => x.partidoid,
                        principalSchema: "public",
                        principalTable: "partidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historialpartido",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partidoid = table.Column<int>(type: "integer", nullable: false),
                    cambiadoporusuarioid = table.Column<Guid>(type: "uuid", nullable: true),
                    tipocambio = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fechaanterior = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    fechanueva = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    sedeanterior = table.Column<string>(type: "text", nullable: true),
                    sedenueva = table.Column<string>(type: "text", nullable: true),
                    estadoanterior = table.Column<string>(type: "text", nullable: true),
                    estadonuevo = table.Column<string>(type: "text", nullable: true),
                    razon = table.Column<string>(type: "text", nullable: true),
                    fechacambio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historialpartido", x => x.id);
                    table.ForeignKey(
                        name: "fk_historialpartido_partidos_partidoid",
                        column: x => x.partidoid,
                        principalSchema: "public",
                        principalTable: "partidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_historialpartido_usuarios_cambiadoporid",
                        column: x => x.cambiadoporusuarioid,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notificacionesinternas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    referenciaid = table.Column<int>(type: "integer", nullable: true),
                    urlaccion = table.Column<string>(type: "text", nullable: true),
                    leida = table.Column<bool>(type: "boolean", nullable: false),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechalectura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacionesinternas", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificacionesinternas_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reportespartido",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partidoid = table.Column<int>(type: "integer", nullable: false),
                    arbitroid = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    minutosjugados = table.Column<int>(type: "integer", nullable: true),
                    huboprorroga = table.Column<bool>(type: "boolean", nullable: false),
                    hubopenales = table.Column<bool>(type: "boolean", nullable: false),
                    goleslocalpenales = table.Column<int>(type: "integer", nullable: true),
                    golesvisitantepenales = table.Column<int>(type: "integer", nullable: true),
                    fechaenvio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    motivoreapertura = table.Column<string>(type: "text", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reportespartido", x => x.id);
                    table.ForeignKey(
                        name: "fk_reportespartido_arbitros_arbitroid",
                        column: x => x.arbitroid,
                        principalSchema: "public",
                        principalTable: "arbitros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reportespartido_partidos_partidoid",
                        column: x => x.partidoid,
                        principalSchema: "public",
                        principalTable: "partidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "convocatoriasjugadores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    convocatoriaid = table.Column<int>(type: "integer", nullable: false),
                    equipojugadortorneoid = table.Column<int>(type: "integer", nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    posicionasignada = table.Column<string>(type: "text", nullable: true),
                    estadoconfirmacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fechaconfirmacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    notaconfirmacion = table.Column<string>(type: "text", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convocatoriasjugadores", x => x.id);
                    table.ForeignKey(
                        name: "fk_convocatoriasjugadores_convocatorias_convocatoriaid",
                        column: x => x.convocatoriaid,
                        principalSchema: "public",
                        principalTable: "convocatorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_convocatoriasjugadores_equiposjugadorestorneos_equipojugado~",
                        column: x => x.equipojugadortorneoid,
                        principalSchema: "public",
                        principalTable: "equiposjugadorestorneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incidenciaspartido",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    partidoid = table.Column<int>(type: "integer", nullable: false),
                    reportepartidoid = table.Column<int>(type: "integer", nullable: true),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    ejtjugadorid = table.Column<int>(type: "integer", nullable: true),
                    ejtjugadorsalidaid = table.Column<int>(type: "integer", nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    minuto = table.Column<int>(type: "integer", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    esautogol = table.Column<bool>(type: "boolean", nullable: false),
                    espenal = table.Column<bool>(type: "boolean", nullable: false),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_incidenciaspartido", x => x.id);
                    table.ForeignKey(
                        name: "fk_incidenciaspartido_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_incidenciaspartido_equiposjugadorestorneos_ejtjugadorid",
                        column: x => x.ejtjugadorid,
                        principalSchema: "public",
                        principalTable: "equiposjugadorestorneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_incidenciaspartido_equiposjugadorestorneos_ejtjugadorsalida~",
                        column: x => x.ejtjugadorsalidaid,
                        principalSchema: "public",
                        principalTable: "equiposjugadorestorneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_incidenciaspartido_partidos_partidoid",
                        column: x => x.partidoid,
                        principalSchema: "public",
                        principalTable: "partidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_incidenciaspartido_reportespartido_reportepartidoid",
                        column: x => x.reportepartidoid,
                        principalSchema: "public",
                        principalTable: "reportespartido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_convocatorias_entrenadorid",
                schema: "public",
                table: "convocatorias",
                column: "entrenadorid");

            migrationBuilder.CreateIndex(
                name: "ix_convocatorias_equipoid",
                schema: "public",
                table: "convocatorias",
                column: "equipoid");

            migrationBuilder.CreateIndex(
                name: "IX_convocatorias_partidoid_equipoid",
                schema: "public",
                table: "convocatorias",
                columns: new[] { "partidoid", "equipoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_convocatoriasjugadores_convocatoriaid_equipojugadortorneoid",
                schema: "public",
                table: "convocatoriasjugadores",
                columns: new[] { "convocatoriaid", "equipojugadortorneoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_convocatoriasjugadores_equipojugadortorneoid",
                schema: "public",
                table: "convocatoriasjugadores",
                column: "equipojugadortorneoid");

            migrationBuilder.CreateIndex(
                name: "IX_historialpartido_cambiadoporusuarioid",
                schema: "public",
                table: "historialpartido",
                column: "cambiadoporusuarioid");

            migrationBuilder.CreateIndex(
                name: "ix_historialpartido_partidoid",
                schema: "public",
                table: "historialpartido",
                column: "partidoid");

            migrationBuilder.CreateIndex(
                name: "ix_incidenciaspartido_ejtjugadorid",
                schema: "public",
                table: "incidenciaspartido",
                column: "ejtjugadorid");

            migrationBuilder.CreateIndex(
                name: "ix_incidenciaspartido_ejtjugadorsalidaid",
                schema: "public",
                table: "incidenciaspartido",
                column: "ejtjugadorsalidaid");

            migrationBuilder.CreateIndex(
                name: "ix_incidenciaspartido_equipoid",
                schema: "public",
                table: "incidenciaspartido",
                column: "equipoid");

            migrationBuilder.CreateIndex(
                name: "IX_incidenciaspartido_partidoid_tipo",
                schema: "public",
                table: "incidenciaspartido",
                columns: new[] { "partidoid", "tipo" });

            migrationBuilder.CreateIndex(
                name: "ix_incidenciaspartido_reportepartidoid",
                schema: "public",
                table: "incidenciaspartido",
                column: "reportepartidoid");

            migrationBuilder.CreateIndex(
                name: "IX_notificacionesinternas_usuarioid_leida",
                schema: "public",
                table: "notificacionesinternas",
                columns: new[] { "usuarioid", "leida" });

            migrationBuilder.CreateIndex(
                name: "ix_reportespartido_arbitroid",
                schema: "public",
                table: "reportespartido",
                column: "arbitroid");

            migrationBuilder.CreateIndex(
                name: "ix_reportespartido_partidoid",
                schema: "public",
                table: "reportespartido",
                column: "partidoid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convocatoriasjugadores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "historialpartido",
                schema: "public");

            migrationBuilder.DropTable(
                name: "incidenciaspartido",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notificacionesinternas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "convocatorias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reportespartido",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "genero",
                schema: "public",
                table: "personas");

            migrationBuilder.DropColumn(
                name: "fechainicioreal",
                schema: "public",
                table: "partidos");

            migrationBuilder.DropColumn(
                name: "minutoactual",
                schema: "public",
                table: "partidos");

            migrationBuilder.DropColumn(
                name: "razonreprogramacion",
                schema: "public",
                table: "partidos");

            migrationBuilder.DropColumn(
                name: "tipoequipo",
                schema: "public",
                table: "equipos");
        }
    }
}
