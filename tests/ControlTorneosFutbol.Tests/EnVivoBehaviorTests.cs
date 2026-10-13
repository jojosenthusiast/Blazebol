using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Pages.Arbitro.Partidos;
using ControlTorneosFutbol.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ControlTorneosFutbol.Tests;

public sealed class EnVivoBehaviorTests
{
    private const int MatchId = 41;
    private const int RefereeId = 7;
    private static readonly Guid UserId = Guid.Parse("aef6a24a-ccfc-4ce6-9ba4-70cf581b91c2");

    [Fact]
    public async Task Estado_asignado_puede_obtener_el_estado_en_vivo()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService();
        var page = CreatePage(db, service, UserId, "Arbitro");

        var result = await page.OnGetEstadoAsync(MatchId);

        Assert.IsType<JsonResult>(result);
        Assert.Equal(MatchId, service.StateMatchId);
        Assert.Equal(StatusCodes.Status200OK, ((JsonResult)result).StatusCode ?? StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Estado_referee_no_asignado_es_denegado_sin_consultar_servicio()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var otherUserId = Guid.Parse("aef6a24a-ccfc-4ce6-9ba4-70cf581b91c3");
        var otherPersona = new Persona { Id = Guid.NewGuid(), UsuarioId = otherUserId, Nombre = "Otra", ApellidoPaterno = "Arbitra" };
        otherPersona.Arbitro = new Arbitro { Id = 8, PersonaId = otherPersona.Id, NumeroLicencia = "REF-8" };
        await db.Personas.AddAsync(otherPersona);
        await db.SaveChangesAsync();
        var service = new RecordingLiveService();
        var page = CreatePage(db, service, otherUserId, "Arbitro");

        var result = await page.OnGetEstadoAsync(MatchId);

        AssertDenied(result);
        Assert.Null(service.StateMatchId);
    }

    [Fact]
    public async Task Gol_referee_no_asignado_es_denegado_y_no_registra_la_incidencia()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService();
        var page = CreatePage(db, service, Guid.NewGuid(), "Arbitro");

        var result = await page.OnPostGolAsync(MatchId, 10, null, 12, false, false);

        AssertDenied(result);
        Assert.Equal(0, service.IncidentCalls);
    }

    [Fact]
    public async Task Gol_referee_asignado_puede_registrar_la_incidencia()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService();
        var page = CreatePage(db, service, UserId, "Arbitro");

        var result = await page.OnPostGolAsync(MatchId, 10, null, 12, false, false);

        var jsonResult = Assert.IsType<JsonResult>(result);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(jsonResult.Value));
        Assert.True(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(1, service.IncidentCalls);
        Assert.Equal(MatchId, service.LastIncidentMatchId);
    }

    [Fact]
    public async Task Incidencia_generica_rechaza_tipo_gol_sin_invocar_el_servicio()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService();
        var page = CreatePage(db, service, UserId, "Arbitro");

        var result = Assert.IsType<JsonResult>(await page.OnPostIncidenciaAsync(MatchId, 10, "Gol", null, 12, null));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));

        Assert.True(result.StatusCode == StatusCodes.Status400BadRequest
            || json.RootElement.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.False);
        Assert.Equal(0, service.IncidentCalls);
    }

    [Fact]
    public async Task Registrar_gol_si_servicio_falla_responde_ok_false()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService { IncidentResult = false };
        var page = CreatePage(db, service, UserId, "Arbitro");

        var result = Assert.IsType<JsonResult>(await page.OnPostGolAsync(MatchId, 10, null, 12, false, false));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));

        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task Finalizar_si_el_servicio_falla_no_devuelve_url_de_reporte()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var service = new RecordingLiveService { FinalizationResult = false };
        var page = CreatePage(db, service, UserId, "Arbitro");

        var result = Assert.IsType<JsonResult>(await page.OnPostFinalizarAsync(MatchId));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result.Value));

        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.True(json.RootElement.GetProperty("redirectUrl").ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task Registrar_incidencia_rechaza_equipo_existente_que_no_juega_el_partido()
    {
        await using var db = CreateDb();
        await SeedMatchTeamsAndTournament(db);
        var service = new PartidoEnVivoService(db);

        var accepted = await service.RegistrarIncidenciaAsync(MatchId, TiposIncidencia.Gol, 30, null, 5, null, false, false);

        Assert.False(accepted);
        Assert.Equal(0, await db.IncidenciasPartido.CountAsync());
    }

    [Fact]
    public async Task Registrar_incidencia_rechaza_jugador_ejt_ausente_de_la_convocatoria()
    {
        await using var db = CreateDb();
        await SeedMatchTeamsAndTournament(db, includePlayerAndConvocation: true);
        var service = new PartidoEnVivoService(db);

        var accepted = await service.RegistrarIncidenciaAsync(MatchId, TiposIncidencia.Gol, 10, 501, 5, null, false, false);

        Assert.False(accepted);
        Assert.Equal(0, await db.IncidenciasPartido.CountAsync());
    }

    [Fact]
    public async Task Gol_en_partido_y_convocatoria_validos_se_acepta()
    {
        await using var db = CreateDb();
        await SeedMatchTeamsAndTournament(db, includePlayerAndConvocation: true);
        var service = new PartidoEnVivoService(db);

        var accepted = await service.RegistrarIncidenciaAsync(MatchId, TiposIncidencia.Gol, 10, 500, 5, null, false, false);

        Assert.True(accepted);
        Assert.Equal(1, await db.IncidenciasPartido.CountAsync());
    }

    [Fact]
    public async Task Autogol_valida_jugador_del_equipo_que_lo_comete_y_suma_al_oponente()
    {
        await using var db = CreateDb();
        await SeedMatchTeamsAndTournament(db, includePlayerAndConvocation: true);
        var service = new PartidoEnVivoService(db);

        var accepted = await service.RegistrarIncidenciaAsync(MatchId, TiposIncidencia.Gol, 10, 500, 12, null, true, false);

        Assert.True(accepted);
        Assert.Equal(1, await db.IncidenciasPartido.CountAsync());
        var match = await db.Partidos.SingleAsync(p => p.Id == MatchId);
        Assert.Equal(0, match.GolesLocal);
        Assert.Equal(1, match.GolesVisitante);
    }

    [Fact]
    public async Task Partido_reprogramado_asignado_puede_iniciarse()
    {
        await using var db = CreateDb();
        await SeedRefereeAndMatch(db);
        var match = await db.Partidos.SingleAsync(p => p.Id == MatchId);
        match.Estado = EstadosPartido.Reprogramado;
        await db.SaveChangesAsync();
        var service = new PartidoEnVivoService(db);

        var started = await service.IniciarPartidoAsync(MatchId, RefereeId, new NoopNotificationService());

        Assert.True(started);
        Assert.Equal(EstadosPartido.EnCurso, (await db.Partidos.SingleAsync(p => p.Id == MatchId)).Estado);
    }

    [Fact]
    public void Team_names_are_not_interpolated_into_inline_javascript_handlers()
    {
        var sourceRoot = FindSourceRoot();
        var viewSource = File.ReadAllText(Path.Combine(sourceRoot, "Pages/Arbitro/Partidos/EnVivo.cshtml"));

        Assert.DoesNotContain("onclick=\"abrirModalGol(", viewSource, StringComparison.Ordinal);
        Assert.Contains("data-goal-team-id=", viewSource, StringComparison.Ordinal);
        Assert.Contains("data-goal-team-name=", viewSource, StringComparison.Ordinal);
        Assert.Contains("addEventListener('click'", viewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Feed_en_vivo_escapa_las_etiquetas_dinamicas_antes_de_insertarlas_como_html()
    {
        var sourceRoot = FindSourceRoot();
        var viewSource = File.ReadAllText(Path.Combine(sourceRoot, "Pages/Arbitro/Partidos/EnVivo.cshtml"));
        var feedSection = ExtractSection(viewSource, "function actualizarFeed(incidencias)", "function incIconFor");
        Assert.Contains("function escapeHtml(", viewSource, StringComparison.Ordinal);
        var escapeSection = ExtractSection(viewSource, "function escapeHtml(", "function incIconFor(");
        Assert.Contains("escapeHtml(labelFor(inc))", feedSection, StringComparison.Ordinal);
        Assert.Contains("escapeHtml(inc.nombreEquipo || '')", feedSection, StringComparison.Ordinal);
        Assert.DoesNotContain("${labelFor(inc)}", feedSection, StringComparison.Ordinal);
        Assert.DoesNotContain("${inc.nombreEquipo || ''}", feedSection, StringComparison.Ordinal);
        Assert.Contains("textContent", escapeSection, StringComparison.Ordinal);
        Assert.Contains("innerHTML", escapeSection, StringComparison.Ordinal);
    }

    [Fact]
    public void Pagina_viva_no_omite_antiforgery_y_publica_token_para_post()
    {
        var sourceRoot = FindSourceRoot();
        var pageModelSource = File.ReadAllText(Path.Combine(sourceRoot, "Pages/Arbitro/Partidos/EnVivo.cshtml.cs"));
        var viewSource = File.ReadAllText(Path.Combine(sourceRoot, "Pages/Arbitro/Partidos/EnVivo.cshtml"));

        Assert.DoesNotContain("[IgnoreAntiforgeryToken]", pageModelSource, StringComparison.Ordinal);
        Assert.Contains("@Html.AntiForgeryToken()", viewSource, StringComparison.Ordinal);
        Assert.Contains("__RequestVerificationToken", viewSource, StringComparison.Ordinal);

        var startPost = ExtractSection(viewSource, "async function iniciarPartido()", "</script>");
        var sharedPost = ExtractSection(viewSource, "async function fetchPost", "function actualizarFeed");
        Assert.Contains("?handler=Iniciar", startPost, StringComparison.Ordinal);
        Assert.Contains("X-CSRF-TOKEN", startPost, StringComparison.Ordinal);
        Assert.Contains("X-CSRF-TOKEN", sharedPost, StringComparison.Ordinal);
    }

    private static string ExtractSection(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find section start: {startMarker}");
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(end >= 0, $"Could not find section end: {endMarker}");
        return source[start..end];
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static EnVivoModel CreatePage(ApplicationDbContext db, RecordingLiveService service, Guid userId, string role)
    {
        var page = new EnVivoModel(db, service, new NoopNotificationService())
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };
        page.HttpContext.Session = new TestSession();
        page.HttpContext.Session.SetUserSession(userId, role, "Referee", "referee@example.test");
        return page;
    }

    private static async Task SeedRefereeAndMatch(ApplicationDbContext db)
    {
        await SeedMatchTeamsAndTournament(db);
        var persona = new Persona { Id = Guid.NewGuid(), UsuarioId = UserId, Nombre = "Árbitro", ApellidoPaterno = "Asignado" };
        persona.Arbitro = new Arbitro { Id = RefereeId, PersonaId = persona.Id, NumeroLicencia = "REF-7" };
        db.Personas.Add(persona);
        var match = await db.Partidos.SingleAsync(p => p.Id == MatchId);
        match.ArbitroId = RefereeId;
        match.Arbitro = persona.Arbitro;
        await db.SaveChangesAsync();
    }

    private static async Task SeedMatchTeamsAndTournament(ApplicationDbContext db, bool includePlayerAndConvocation = false)
    {
        db.Equipos.AddRange(
            new Equipo { Id = 10, Nombre = "Local" },
            new Equipo { Id = 20, Nombre = "Visitante" },
            new Equipo { Id = 30, Nombre = "Tercero" });
        db.Torneos.Add(new Torneo { Id = 1, Nombre = "Torneo", Formato = FormatosTorneo.TodosContraTodos, Estado = EstadosTorneo.Activo });
        db.Partidos.Add(new Partido
        {
            Id = MatchId, TorneoId = 1, EquipoLocalId = 10, EquipoVisitanteId = 20,
            Estado = EstadosPartido.EnCurso, ArbitroId = RefereeId
        });
        if (includePlayerAndConvocation)
        {
            var playerOnePersona = new Persona { Id = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), Nombre = "Jugador", ApellidoPaterno = "Convocado" };
            var playerTwoPersona = new Persona { Id = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), Nombre = "Jugador", ApellidoPaterno = "NoConvocado" };
            db.Personas.AddRange(playerOnePersona, playerTwoPersona);
            db.Jugadores.AddRange(
                new Jugador { Id = 50, PersonaId = playerOnePersona.Id },
                new Jugador { Id = 51, PersonaId = playerTwoPersona.Id });
            var coachPersona = new Persona { Id = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), Nombre = "Entrenador", ApellidoPaterno = "Local" };
            db.Personas.Add(coachPersona);
            db.Entrenadores.Add(new Entrenador { Id = 70, PersonaId = coachPersona.Id });
            db.EquiposJugadoresTorneos.AddRange(
                new EquipoJugadorTorneo { Id = 500, JugadorId = 50, EquipoId = 10, TorneoId = 1, NumeroCamiseta = 9 },
                new EquipoJugadorTorneo { Id = 501, JugadorId = 51, EquipoId = 10, TorneoId = 1, NumeroCamiseta = 10 });
            db.Convocatorias.Add(new Convocatoria { Id = 60, PartidoId = MatchId, EquipoId = 10, EntrenadorId = 70, Estado = EstadosConvocatoria.Publicada });
            db.ConvocatoriasJugadores.Add(new ConvocatoriaJugador { Id = 80, ConvocatoriaId = 60, EquipoJugadorTorneoId = 500 });
        }
        await db.SaveChangesAsync();
    }

    private static void AssertDenied(IActionResult result)
    {
        if (result is JsonResult json)
        {
            Assert.True(json.StatusCode == StatusCodes.Status403Forbidden || IsExplicitDenial(json.Value));
            return;
        }
        Assert.IsType<ForbidResult>(result);
    }

    private static bool IsExplicitDenial(object? value)
    {
        if (value is null) return false;
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value));
        var root = json.RootElement;
        return root.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.False
            || root.TryGetProperty("error", out _)
            || root.TryGetProperty("denied", out var denied) && denied.ValueKind == JsonValueKind.True;
    }

    private static string FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ControlTorneosFutbol.csproj"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the application source root from the test output directory.");
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _values.Keys;
        public void Clear() => _values.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _values.TryGetValue(key, out value);
    }

    private sealed class RecordingLiveService : IPartidoEnVivoService
    {
        public int? StateMatchId { get; private set; }
        public int IncidentCalls { get; private set; }
        public int? LastIncidentMatchId { get; private set; }
        public bool IncidentResult { get; set; } = true;
        public bool FinalizationResult { get; set; } = true;
        public Task<bool> IniciarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif) => Task.FromResult(true);
        public Task<bool> RegistrarIncidenciaAsync(int partidoId, string tipo, int equipoId, int? ejtJugadorId, int? minuto, string? desc, bool esAutogol, bool esPenal, int? ejtJugadorSalidaId = null)
        {
            IncidentCalls++;
            LastIncidentMatchId = partidoId;
            return Task.FromResult(IncidentResult);
        }
        public Task RecalcularGolesAsync(int partidoId) => Task.CompletedTask;
        public Task<bool> FinalizarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif) => Task.FromResult(FinalizationResult);
        public Task<List<IncidenciaPartido>> GetIncidenciasAsync(int partidoId) => Task.FromResult(new List<IncidenciaPartido>());
        public Task<PartidoEnVivoVM> GetEstadoEnVivoAsync(int partidoId)
        {
            StateMatchId = partidoId;
            return Task.FromResult(new PartidoEnVivoVM { PartidoId = partidoId });
        }
    }

    private sealed class NoopNotificationService : INotificacionService
    {
        public Task CrearAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Sistema", int? referenciaId = null, string? urlAccion = null) => Task.CompletedTask;
        public Task<int> ContarNoLeidasAsync(Guid usuarioId) => Task.FromResult(0);
        public Task<List<NotificacionInterna>> GetRecientesAsync(Guid usuarioId, int top = 10) => Task.FromResult(new List<NotificacionInterna>());
        public Task MarcarLeidaAsync(int notificacionId, Guid usuarioId) => Task.CompletedTask;
        public Task MarcarTodasLeidasAsync(Guid usuarioId) => Task.CompletedTask;
        public Task NotificarPartidoAsync(int partidoId, string titulo, string mensaje, string tipo = "Partido") => Task.CompletedTask;
    }
}
