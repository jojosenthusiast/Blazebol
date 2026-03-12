# BACKEND_PATTERNS.md — ControlTorneosFutbol

> Arquitectura, patrones de servicio, seguridad, manejo de errores.

---

## 1. Arquitectura General

```
Request → Middleware → Razor Page PageModel → Service → DbContext → PostgreSQL
```

### Capas

| Capa | Responsabilidad | Carpeta |
|------|----------------|---------|
| **Presentación** | Razor Pages (.cshtml + .cshtml.cs) | `Pages/` |
| **Lógica de negocio** | Services (IXxxService + XxxService) | `Services/` |
| **Datos** | EF Core DbContext + Entities | `Data/` |
| **Helpers** | SessionHelper, utilidades | `Helpers/` |
| **ViewModels** | DTOs para formularios y vistas | `Models/ViewModels/` |

---

## 2. Patrón de Servicios

### Interfaz + Implementación
```csharp
// IAuthService.cs
public interface IAuthService
{
    Task<(bool Success, string Message, Guid? UserId)> RegisterEspectadorAsync(RegisterEspectadorViewModel model);
    Task<(bool Success, string Message, Usuario?)> LoginAsync(string email, string password);
    Task<bool> EmailExistsAsync(string email);
}

// AuthService.cs
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public AuthService(ApplicationDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task<(bool Success, string Message, Guid? UserId)> RegisterEspectadorAsync(...)
    {
        try {
            // lógica...
            return (true, "Cuenta creada exitosamente", usuario.Id);
        }
        catch (Exception ex) {
            return (false, $"Error: {ex.Message}", null);
        }
    }
}
```

### Registro en `Program.cs`
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITorneoService, TorneoService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
```

---

## 3. Patrón de Retorno `(bool, string, T?)`

Todos los métodos de servicio que pueden fallar retornan una tupla nombrada:

```csharp
// Éxito
return (true, "Torneo creado exitosamente", torneo.Id);

// Error controlado
return (false, "El torneo ya existe", null);

// Error de excepción
catch (Exception ex)
{
    return (false, $"Error al crear torneo: {ex.Message}", null);
}
```

### Uso en PageModel
```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();

    var (success, message, torneoId) = await _torneoService.CreateTorneoAsync(Input);

    if (!success)
    {
        ErrorMessage = message;
        return Page();
    }

    SuccessMessage = message;
    return RedirectToPage("./Index");
}
```

---

## 4. Patrón PageModel Estándar

```csharp
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;   // o ITorneoService

    // Propiedades de vista
    public List<Torneo> Torneos { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    // Binding de formulario/filtro
    [BindProperty(SupportsGet = true)]
    public string? FiltroEstado { get; set; }

    // Input de POST (ViewModel para formulario)
    [BindProperty]
    public TorneoInputModel Input { get; set; } = new();

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verificar autenticación
        var userId = HttpContext.Session.GetUserId();
        if (userId == null) return RedirectToPage("/Account/Login");

        // Cargar datos
        Torneos = await _context.Torneos
            .Include(t => t.Liga)
            .Where(t => FiltroEstado == null || t.Estado == FiltroEstado)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        // ...
    }
}
```

---

## 5. Manejo de Sesión en PageModel

```csharp
// Obtener datos del usuario logueado
var userId = HttpContext.Session.GetUserId();         // Guid?
var userRole = HttpContext.Session.GetUserRole();     // string?
var userName = HttpContext.Session.GetUserName();     // string?
var userEmail = HttpContext.Session.GetUserEmail();   // string?

// Verificar autenticación
if (!HttpContext.Session.IsAuthenticated())
    return RedirectToPage("/Account/Login");

// Verificar rol específico
if (!HttpContext.Session.IsInRole("Administrador"))
    return Forbid();
```

---

## 6. EF Core — Patrones de Query

### Include navegación
```csharp
// Simple
var torneo = await _context.Torneos
    .Include(t => t.Liga)
    .Include(t => t.Temporada)
    .Include(t => t.Categoria)
    .Include(t => t.ConfiguracionTorneo)
    .FirstOrDefaultAsync(t => t.Id == id);

// Multi-nivel (no soportado nativo en EF8 — usar join manual)
var partidos = await _context.Partidos
    .Include(p => p.EquipoLocal)
    .Include(p => p.EquipoVisitante)
    .Include(p => p.Torneo)
    .Include(p => p.Arbitro)
        .ThenInclude(a => a!.Persona)
    .ToListAsync();
```

### Select proyección (evitar carga completa)
```csharp
var vms = await _context.Partidos
    .Where(p => p.Estado == EstadosPartido.Programado)
    .OrderBy(p => p.Fecha)
    .Take(5)
    .Select(p => new ProximoPartidoDashVM
    {
        Id            = p.Id,
        Fecha         = p.Fecha,
        NombreLocal   = p.EquipoLocal.Nombre,
        AbrevLocal    = p.EquipoLocal.Abreviacion ?? p.EquipoLocal.Nombre.Substring(0, 3).ToUpper(),
        NombreTorneo  = p.Torneo.Nombre,
        TorneoId      = p.TorneoId,
        Estado        = p.Estado,
        ColorTorneo   = GetTorneoColor(p.TorneoId)
    })
    .ToListAsync();
```

### AnyAsync para verificar existencia
```csharp
var exists = await _context.Usuarios
    .AnyAsync(u => u.Email == email);
```

---

## 7. Seguridad

### Contraseñas (BCrypt)
```csharp
// Hash al registrar
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

// Verificar al login
bool valid = BCrypt.Net.BCrypt.Verify(password, storedHash);
```

### Encriptación de datos sensibles (AES-256-CBC)
```csharp
// En EncryptionService:
// - Key: 32 bytes derivados de configuración
// - IV: primeros 16 bytes de la key (simplificación aceptada)
// - Usada para: Persona.Dui (DUI/Pasaporte)

var encrypted = _encryptionService.Encrypt(dui);
var decrypted = _encryptionService.Decrypt(encryptedDui);
```

### Anti-cache headers (evitar ver páginas privadas con Back)
```csharp
// En Program.cs — middleware global
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    await next();
});
```

### Middleware de autenticación por ruta
```csharp
// Program.cs — antes de app.MapRazorPages()
app.Use(async (context, next) =>
{
    var path = context.Request.Path.ToString().ToLower();
    var session = context.Session;

    // Rutas protegidas por rol
    if (path.StartsWith("/admin") && !session.IsInRole("Administrador"))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    if (path.StartsWith("/jugador") && !session.IsInRole("Jugador")) { ... }
    if (path.StartsWith("/entrenador") && !session.IsInRole("Entrenador")) { ... }
    if (path.StartsWith("/arbitro") && !session.IsInRole("Arbitro")) { ... }
    if (path.StartsWith("/espectador") && !session.IsInRole("Espectador")) { ... }

    await next();
});
```

---

## 8. Inyección de Dependencias — Registro Completo

```csharp
// Program.cs
// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITorneoService, TorneoService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

// Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Razor Pages
builder.Services.AddRazorPages();
```

---

## 9. Constantes del Dominio

```csharp
// Roles
public static class RolesConstantes
{
    public const string Administrador = "Administrador";
    public const string Espectador    = "Espectador";
    public const string Jugador       = "Jugador";
    public const string Entrenador    = "Entrenador";
    public const string Arbitro       = "Arbitro";
}

// Formatos de torneo
public static class FormatosTorneo
{
    public const string TodosContraTodos    = "TodosContraTodos";
    public const string EliminacionDirecta  = "EliminacionDirecta";
    public const string GruposYEliminacion  = "GruposYEliminacion";
}

// Estados de torneo
public static class EstadosTorneo
{
    public const string Borrador   = "Borrador";
    public const string Activo     = "Activo";
    public const string Finalizado = "Finalizado";
}

// Estados de partido
public static class EstadosPartido
{
    public const string Programado = "Programado";
    public const string EnCurso    = "EnCurso";
    public const string Finalizado = "Finalizado";
    public const string Cancelado  = "Cancelado";
}

// Estados de solicitud
public static class EstadosSolicitud
{
    public const string Pendiente = "Pendiente";
    public const string Aceptada  = "Aceptada";
    public const string Rechazada = "Rechazada";
}

// Estados de árbitro
public static class EstadosAprobacionArbitro
{
    public const string Pendiente  = "Pendiente";
    public const string Aprobado   = "Aprobado";
    public const string Suspendido = "Suspendido";
    public const string Rechazado  = "Rechazado";
}

// Posiciones jugador
public static class PosicionesJugador
{
    public const string Portero   = "Portero";
    public const string Defensa   = "Defensa";
    public const string Medio     = "Medio";
    public const string Delantero = "Delantero";
}
```

---

## 10. QuestPDF — Patrón de Generación

```csharp
// PdfService.cs
public byte[] GenerarReporteTorneosPdf(List<Torneo> torneos)
{
    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.Content().Column(col =>
            {
                col.Item().Text("Reporte de Torneos").Bold().FontSize(18);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);  // Nombre
                        cols.RelativeColumn(2);  // Estado
                        cols.RelativeColumn(2);  // Formato
                    });
                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("Estado").Bold();
                        header.Cell().Text("Formato").Bold();
                    });
                    // Rows
                    foreach (var t in torneos)
                    {
                        table.Cell().Text(t.Nombre);
                        table.Cell().Text(t.Estado);
                        table.Cell().Text(t.Formato);
                    }
                });
            });
        });
    }).GeneratePdf();
}
```

### Uso en PageModel (handler de descarga)
```csharp
public async Task<IActionResult> OnGetExportPdfAsync()
{
    var torneos = await _torneoService.GetTorneosAsync();
    var pdf = _pdfService.GenerarReporteTorneosPdf(torneos);
    return File(pdf, "application/pdf", "torneos.pdf");
}
```

---

## 11. Validación en ViewModels

```csharp
public class TorneoInputModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El formato es requerido")]
    public string Formato { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DateGreaterThan("FechaInicio", ErrorMessage = "La fecha de fin debe ser posterior a la de inicio")]
    public DateTime FechaFin { get; set; }
}
```

---

## 12b. AJAX Polling — Panel en Vivo *(Sprint 4)*

Para el panel del árbitro en vivo se usa GET polling en lugar de SignalR (no configurado).

### Handler GET sin antiforgery (PageModel)
```csharp
// En EnVivo.cshtml.cs
[IgnoreAntiforgeryToken]
public async Task<IActionResult> OnGetEstadoAsync(int partidoId)
{
    var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
    return new JsonResult(vm);
}
```

### Cliente JavaScript (polling cada 10s)
```javascript
const partidoId = @Model.PartidoId;

async function actualizarEstado() {
    const r = await fetch(`?handler=Estado&partidoId=${partidoId}`);
    const data = await r.json();
    document.getElementById('goles-local').textContent = data.golesLocal;
    document.getElementById('goles-visitante').textContent = data.golesVisitante;
    document.getElementById('minuto').textContent = data.minutoActual + "'";
    actualizarIncidencias(data.incidencias);
}

setInterval(actualizarEstado, 10000);
actualizarEstado(); // Carga inicial
```

---

## 12c. Patrón de Notificación Cross-User *(Sprint 4)*

Desde cualquier servicio que necesite notificar a múltiples usuarios:

```csharp
// En PartidoEnVivoService o ConvocatoriaService
public class PartidoEnVivoService : IPartidoEnVivoService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacionService _notif;

    public async Task<bool> FinalizarPartidoAsync(int partidoId, int arbitroId, bool enviarNotif = true)
    {
        var partido = await _context.Partidos.FindAsync(partidoId);
        if (partido == null || partido.Estado != EstadosPartido.EnCurso) return false;

        partido.Estado = EstadosPartido.Finalizado;
        partido.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (enviarNotif)
            await _notif.NotificarPartidoAsync(partidoId, "Partido Finalizado",
                $"El partido ha finalizado {partido.GolesLocal}-{partido.GolesVisitante}",
                TiposNotificacion.Partido);

        return true;
    }
}
```

### Polling del bell badge (sidebar, cada 60s)
```javascript
// En _Sidebar.cshtml <script>
async function actualizarNotificaciones() {
    const r = await fetch('/Notificaciones?handler=Conteo');
    const data = await r.json();
    const badge = document.getElementById('notif-badge');
    badge.textContent = data.noLeidas;
    badge.style.display = data.noLeidas > 0 ? 'inline-flex' : 'none';
}
setInterval(actualizarNotificaciones, 60000);
actualizarNotificaciones();
```

---

## 12d. Validación de Compatibilidad TipoEquipo *(Sprint 4)*

La validación de si un jugador puede unirse a un equipo se hace con el método estático en `TiposEquipo`:

```csharp
// En BuscarEquipos.cshtml.cs o en ConvocatoriaService
var jugador = await _context.Jugadores
    .Include(j => j.Persona)
    .FirstOrDefaultAsync(j => j.Id == jugadorId);

var (ok, razon) = TiposEquipo.ValidarJugador(
    equipo.TipoEquipo,
    jugador.Persona.Genero,
    jugador.Persona.FechaNacimiento);

if (!ok)
{
    ErrorMessage = razon;
    return Page();
}
```

### Nuevas constantes Sprint 4
```csharp
// EstadosPartido actualizado
public static class EstadosPartido
{
    public const string Programado   = "Programado";
    public const string EnCurso      = "EnCurso";
    public const string Finalizado   = "Finalizado";
    public const string Cancelado    = "Cancelado";
    public const string Reprogramado = "Reprogramado";  // Nuevo Sprint 4
}

// TiposEquipo (en Equipo.cs)
public static class TiposEquipo
{
    public const string Libre = "Libre"; public const string Masculino = "Masculino";
    public const string Femenino = "Femenino"; public const string Juvenil = "Juvenil";
    public const string Amateur = "Amateur"; public const string Veterano = "Veterano";
    public static (bool ok, string razon) ValidarJugador(string tipo, string? genero, DateTime? dob) { ... }
}

// GenerosPersona (en Persona.cs)
public static class GenerosPersona
{
    public const string Masculino = "Masculino";
    public const string Femenino = "Femenino";
    public const string Otro = "Otro";
}
```

### DI Registration Sprint 4 (Program.cs)
```csharp
// Agregar a los servicios existentes:
builder.Services.AddScoped<IConvocatoriaService, ConvocatoriaService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IPartidoEnVivoService, PartidoEnVivoService>();
```

---

## 12. Notas de Deuda Técnica

| Punto | Descripción |
|-------|-------------|
| Sin transacciones | `AuthService` no usa `using var tx = await _context.Database.BeginTransactionAsync()` — riesgo de datos inconsistentes en registros con múltiples `SaveChanges` |
| IV fijo AES | El IV es `key[0..15]` — aceptable para demo, NO para producción |
| Sin 2FA | Campo `AutenticacionDosFactores` existe pero no implementado |
| Sin email | `NotificacionesEmail` en preferencias espectador pero no hay sender |
| Sin paginación | Tablas cargan todos los registros — añadir `.Skip().Take()` si escala |
