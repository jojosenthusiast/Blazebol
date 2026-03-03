using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlTorneosFutbol.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "categorias",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    edadminima = table.Column<int>(type: "integer", nullable: true),
                    edadmaxima = table.Column<int>(type: "integer", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    logo = table.Column<string>(type: "text", nullable: true),
                    colorprimario = table.Column<string>(type: "text", nullable: true),
                    colorsecundario = table.Column<string>(type: "text", nullable: true),
                    sede = table.Column<string>(type: "text", nullable: true),
                    fechafundacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ligas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "text", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ligas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombrenormalizado = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "temporadas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    fechainicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechafin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_temporadas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    emailconfirmado = table.Column<bool>(type: "boolean", nullable: false),
                    passwordhash = table.Column<string>(type: "text", nullable: false),
                    nombreusuario = table.Column<string>(type: "text", nullable: true),
                    telefononumero = table.Column<string>(type: "text", nullable: true),
                    telefonoconfirmado = table.Column<bool>(type: "boolean", nullable: false),
                    autenticaciondosfactores = table.Column<bool>(type: "boolean", nullable: false),
                    fechabloqueofin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    bloqueohabilitado = table.Column<bool>(type: "boolean", nullable: false),
                    contadoraccesosfallidos = table.Column<int>(type: "integer", nullable: false),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "torneos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ligaid = table.Column<int>(type: "integer", nullable: false),
                    temporadaid = table.Column<int>(type: "integer", nullable: false),
                    categoriaid = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    formato = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fechainicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechafin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    numeroequiposmaximo = table.Column<int>(type: "integer", nullable: true),
                    numeroequiposminimo = table.Column<int>(type: "integer", nullable: false),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_torneos", x => x.id);
                    table.ForeignKey(
                        name: "fk_torneos_categorias_categoriaid",
                        column: x => x.categoriaid,
                        principalSchema: "public",
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_torneos_ligas_ligaid",
                        column: x => x.ligaid,
                        principalSchema: "public",
                        principalTable: "ligas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_torneos_temporadas_temporadaid",
                        column: x => x.temporadaid,
                        principalSchema: "public",
                        principalTable: "temporadas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditoriaaccesos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    direccionip = table.Column<string>(type: "text", nullable: true),
                    useragent = table.Column<string>(type: "text", nullable: true),
                    fechaintento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    motivofallo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoriaaccesos", x => x.id);
                    table.ForeignKey(
                        name: "fk_auditoriaaccesos_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidopaterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidomaterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fechanacimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    identificacioncifrada = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    fotoperfil = table.Column<string>(type: "text", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechaactualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personas", x => x.id);
                    table.ForeignKey(
                        name: "fk_personas_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuariosroles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    rolid = table.Column<int>(type: "integer", nullable: false),
                    fechaasignacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuariosroles", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuariosroles_roles_rolid",
                        column: x => x.rolid,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuariosroles_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "configuracionestorneo",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    puntosporvictoria = table.Column<int>(type: "integer", nullable: false),
                    puntosporempate = table.Column<int>(type: "integer", nullable: false),
                    puntosporderrota = table.Column<int>(type: "integer", nullable: false),
                    criteriodesempate1 = table.Column<string>(type: "text", nullable: false),
                    criteriodesempate2 = table.Column<string>(type: "text", nullable: true),
                    criteriodesempate3 = table.Column<string>(type: "text", nullable: true),
                    criteriodesempate4 = table.Column<string>(type: "text", nullable: true),
                    tarjetasamarillasparasuspension = table.Column<int>(type: "integer", nullable: false),
                    partidossuspensiontarjetaroja = table.Column<int>(type: "integer", nullable: false),
                    maximojugadoresconvocados = table.Column<int>(type: "integer", nullable: true),
                    minimojugadorestitulares = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configuracionestorneo", x => x.id);
                    table.ForeignKey(
                        name: "fk_configuracionestorneo_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupos", x => x.id);
                    table.ForeignKey(
                        name: "fk_grupos_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reglaspersonalizadastorneo",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    esautomatica = table.Column<bool>(type: "boolean", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    condicionjson = table.Column<string>(type: "text", nullable: true),
                    fechacreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reglaspersonalizadastorneo", x => x.id);
                    table.ForeignKey(
                        name: "fk_reglaspersonalizadastorneo_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "arbitros",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<Guid>(type: "uuid", nullable: false),
                    numerolicencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    categoriasarbitra = table.Column<string>(type: "text", nullable: true),
                    estadoaprobacion = table.Column<string>(type: "text", nullable: false),
                    fechaaprobacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_arbitros", x => x.id);
                    table.ForeignKey(
                        name: "fk_arbitros_personas_personaid",
                        column: x => x.personaid,
                        principalSchema: "public",
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entrenadores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<Guid>(type: "uuid", nullable: false),
                    numerolicencia = table.Column<string>(type: "text", nullable: true),
                    fechaexpiracionlicencia = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    especialidad = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrenadores", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrenadores_personas_personaid",
                        column: x => x.personaid,
                        principalSchema: "public",
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jugadores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<Guid>(type: "uuid", nullable: false),
                    posicionpreferida = table.Column<string>(type: "text", nullable: true),
                    numerocamiseta = table.Column<int>(type: "integer", nullable: true),
                    peso = table.Column<decimal>(type: "numeric", nullable: true),
                    altura = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jugadores", x => x.id);
                    table.ForeignKey(
                        name: "fk_jugadores_personas_personaid",
                        column: x => x.personaid,
                        principalSchema: "public",
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipostorneos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    grupoid = table.Column<int>(type: "integer", nullable: true),
                    fechainscripcion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipostorneos", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipostorneos_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipostorneos_grupos_grupoid",
                        column: x => x.grupoid,
                        principalSchema: "public",
                        principalTable: "grupos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_equipostorneos_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equiposentrenadores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrenadorid = table.Column<int>(type: "integer", nullable: false),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    rol = table.Column<string>(type: "text", nullable: false),
                    fechainicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechafin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    espropietario = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equiposentrenadores", x => x.id);
                    table.ForeignKey(
                        name: "fk_equiposentrenadores_entrenadores_entrenadorid",
                        column: x => x.entrenadorid,
                        principalSchema: "public",
                        principalTable: "entrenadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equiposentrenadores_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equiposjugadorestorneos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jugadorid = table.Column<int>(type: "integer", nullable: false),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    torneoid = table.Column<int>(type: "integer", nullable: false),
                    numerocamiseta = table.Column<int>(type: "integer", nullable: false),
                    fechainscripcion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fechabaja = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equiposjugadorestorneos", x => x.id);
                    table.ForeignKey(
                        name: "fk_equiposjugadorestorneos_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equiposjugadorestorneos_jugadores_jugadorid",
                        column: x => x.jugadorid,
                        principalSchema: "public",
                        principalTable: "jugadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equiposjugadorestorneos_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitudesjugadorequipo",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jugadorid = table.Column<int>(type: "integer", nullable: false),
                    equipoid = table.Column<int>(type: "integer", nullable: false),
                    torneoid = table.Column<int>(type: "integer", nullable: true),
                    mensajejugador = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fechasolicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fecharespuesta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    respuestaentrenador = table.Column<string>(type: "text", nullable: true),
                    entrenadorresponsableid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitudesjugadorequipo", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitudesjugadorequipo_entrenadores_entrenadorresponsable~",
                        column: x => x.entrenadorresponsableid,
                        principalSchema: "public",
                        principalTable: "entrenadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_solicitudesjugadorequipo_equipos_equipoid",
                        column: x => x.equipoid,
                        principalSchema: "public",
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_solicitudesjugadorequipo_jugadores_jugadorid",
                        column: x => x.jugadorid,
                        principalSchema: "public",
                        principalTable: "jugadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_solicitudesjugadorequipo_torneos_torneoid",
                        column: x => x.torneoid,
                        principalSchema: "public",
                        principalTable: "torneos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_arbitros_numerolicencia",
                schema: "public",
                table: "arbitros",
                column: "numerolicencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_arbitros_personaid",
                schema: "public",
                table: "arbitros",
                column: "personaid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auditoriaaccesos_usuarioid",
                schema: "public",
                table: "auditoriaaccesos",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                schema: "public",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_configuracionestorneo_torneoid",
                schema: "public",
                table: "configuracionestorneo",
                column: "torneoid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entrenadores_personaid",
                schema: "public",
                table: "entrenadores",
                column: "personaid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipos_nombre",
                schema: "public",
                table: "equipos",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equiposentrenadores_entrenadorid",
                schema: "public",
                table: "equiposentrenadores",
                column: "entrenadorid");

            migrationBuilder.CreateIndex(
                name: "ix_equiposentrenadores_equipoid",
                schema: "public",
                table: "equiposentrenadores",
                column: "equipoid");

            migrationBuilder.CreateIndex(
                name: "IX_equiposjugadorestorneos_equipoid_torneoid_numerocamiseta",
                schema: "public",
                table: "equiposjugadorestorneos",
                columns: new[] { "equipoid", "torneoid", "numerocamiseta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equiposjugadorestorneos_jugadorid_torneoid",
                schema: "public",
                table: "equiposjugadorestorneos",
                columns: new[] { "jugadorid", "torneoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equiposjugadorestorneos_torneoid",
                schema: "public",
                table: "equiposjugadorestorneos",
                column: "torneoid");

            migrationBuilder.CreateIndex(
                name: "IX_equipostorneos_equipoid_torneoid",
                schema: "public",
                table: "equipostorneos",
                columns: new[] { "equipoid", "torneoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equipostorneos_grupoid",
                schema: "public",
                table: "equipostorneos",
                column: "grupoid");

            migrationBuilder.CreateIndex(
                name: "ix_equipostorneos_torneoid",
                schema: "public",
                table: "equipostorneos",
                column: "torneoid");

            migrationBuilder.CreateIndex(
                name: "ix_grupos_torneoid",
                schema: "public",
                table: "grupos",
                column: "torneoid");

            migrationBuilder.CreateIndex(
                name: "ix_jugadores_personaid",
                schema: "public",
                table: "jugadores",
                column: "personaid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ligas_nombre",
                schema: "public",
                table: "ligas",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_personas_usuarioid",
                schema: "public",
                table: "personas",
                column: "usuarioid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reglaspersonalizadastorneo_torneoid_orden",
                schema: "public",
                table: "reglaspersonalizadastorneo",
                columns: new[] { "torneoid", "orden" });

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre",
                schema: "public",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitudesjugadorequipo_entrenadorresponsableid",
                schema: "public",
                table: "solicitudesjugadorequipo",
                column: "entrenadorresponsableid");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudesjugadorequipo_equipoid",
                schema: "public",
                table: "solicitudesjugadorequipo",
                column: "equipoid");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudesjugadorequipo_jugadorid",
                schema: "public",
                table: "solicitudesjugadorequipo",
                column: "jugadorid");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudesjugadorequipo_torneoid",
                schema: "public",
                table: "solicitudesjugadorequipo",
                column: "torneoid");

            migrationBuilder.CreateIndex(
                name: "IX_temporadas_nombre_anio",
                schema: "public",
                table: "temporadas",
                columns: new[] { "nombre", "anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_torneos_categoriaid",
                schema: "public",
                table: "torneos",
                column: "categoriaid");

            migrationBuilder.CreateIndex(
                name: "ix_torneos_ligaid",
                schema: "public",
                table: "torneos",
                column: "ligaid");

            migrationBuilder.CreateIndex(
                name: "ix_torneos_temporadaid",
                schema: "public",
                table: "torneos",
                column: "temporadaid");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                schema: "public",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuariosroles_rolid",
                schema: "public",
                table: "usuariosroles",
                column: "rolid");

            migrationBuilder.CreateIndex(
                name: "IX_usuariosroles_usuarioid_rolid",
                schema: "public",
                table: "usuariosroles",
                columns: new[] { "usuarioid", "rolid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arbitros",
                schema: "public");

            migrationBuilder.DropTable(
                name: "auditoriaaccesos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "configuracionestorneo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equiposentrenadores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equiposjugadorestorneos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipostorneos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reglaspersonalizadastorneo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "solicitudesjugadorequipo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuariosroles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "grupos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "entrenadores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "jugadores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "torneos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "personas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categorias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ligas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "temporadas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "public");
        }
    }
}
