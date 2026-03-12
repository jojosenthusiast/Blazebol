# API_REFERENCE.md — ControlTorneosFutbol

> Referencia de todos los servicios, helpers, ViewModels y constantes del sistema.

---

## 1. IAuthService / AuthService

**Namespace:** `ControlTorneosFutbol.Services`
**Archivo:** `Services/AuthService.cs`

### Métodos

```csharp
// Registro de Espectador
Task<(bool Success, string Message, Guid? UserId)>
    RegisterEspectadorAsync(RegisterEspectadorViewModel model)
// Crea: Usuario + Persona + UsuarioRol(Espectador)

// Registro de Jugador
Task<(bool Success, string Message, Guid? UserId)>
    RegisterJugadorAsync(RegisterJugadorViewModel model)
// Crea: Usuario + Persona + Jugador + UsuarioRol(Jugador)

// Registro de Entrenador
Task<(bool Success, string Message, Guid? UserId)>
    RegisterEntrenadorAsync(RegisterEntrenadorViewModel model)
// Crea: Usuario + Persona + Entrenador + Equipo + EquipoEntrenador + UsuarioRol(Entrenador)

// Registro de Árbitro
Task<(bool Success, string Message, Guid? UserId)>
    RegisterArbitroAsync(RegisterArbitroViewModel model)
// Crea: Usuario + Persona + Arbitro(EstadoAprobacion=Pendiente) + UsuarioRol(Arbitro)

// Login
Task<(bool Success, string Message, Usuario? Usuario)>
    LoginAsync(string email, string password)
// Verifica: existencia, activo, lockout, BCrypt.Verify
// Efectos: incrementa contador, bloquea si ≥5 fallos, resetea contador en éxito
// Log: auditoriaaccesos siempre

// Verificar email único
Task<bool> EmailExistsAsync(string email)
```

---

## 2. ITorneoService / TorneoService

**Namespace:** `ControlTorneosFutbol.Services`
**Archivo:** `Services/TorneoService.cs`

### Métodos

```csharp
// Obtener todos los torneos con filtros opcionales
Task<List<Torneo>> GetTorneosAsync(
    string? estado = null,
    int? anio = null,
    int? categoriaId = null)
// Include: Liga, Temporada, Categoria
// OrderBy: FechaCreacion DESC

// Obtener torneo por ID (con toda la configuración)
Task<Torneo?> GetTorneoByIdAsync(int id)
// Include: Liga, Temporada, Categoria, ConfiguracionTorneo

// Crear torneo + ConfiguracionTorneo
Task<(bool Success, string Message, int? TorneoId)>
    CreateTorneoAsync(TorneoViewModel model)

// Actualizar torneo
Task<(bool Success, string Message)>
    UpdateTorneoAsync(int id, TorneoViewModel model)

// Eliminar torneo (cascade en BD)
Task<(bool Success, string Message)>
    DeleteTorneoAsync(int id)
```

---

## 3. IPdfService / PdfService

**Namespace:** `ControlTorneosFutbol.Services`
**Archivo:** `Services/PdfService.cs`

### Métodos

```csharp
// Reporte de usuarios (tabla con datos personales)
byte[] GenerarReporteUsuariosPdf(List<UsuarioReporteViewModel> usuarios)

// Reporte de torneos (nombre, estado, formato, fechas)
byte[] GenerarReporteTorneosPdf(List<Torneo> torneos)

// Reporte de jugadores (nombre, posición, equipo)
byte[] GenerarReporteJugadoresPdf(List<JugadorReporteViewModel> jugadores)
```

### Uso en PageModel
```csharp
// Handler de descarga PDF:
public async Task<IActionResult> OnGetExportPdfAsync()
{
    var torneos = await _torneoService.GetTorneosAsync();
    var pdf = _pdfService.GenerarReporteTorneosPdf(torneos);
    return File(pdf, "application/pdf", "reporte-torneos.pdf");
}
```

---

## 4. IEncryptionService / EncryptionService

**Namespace:** `ControlTorneosFutbol.Services`
**Archivo:** `Services/EncryptionService.cs`

### Métodos

```csharp
// Hash de contraseña con BCrypt (workFactor=11)
string HashPassword(string password)

// Verificar contraseña contra hash almacenado
bool VerifyPassword(string password, string hash)

// Encriptar dato sensible con AES-256-CBC
string Encrypt(string plainText)

// Desencriptar dato sensible
string Decrypt(string cipherText)
```

> **Nota:** IV es fijo (primeros 16 bytes de la key) — simplificación para demo.

---

## 5. SessionHelper

**Namespace:** `ControlTorneosFutbol.Helpers`
**Archivo:** `Helpers/SessionHelper.cs`
**Tipo:** Static extension methods sobre `ISession`

### Métodos

```csharp
// Establecer toda la sesión de una vez
void SetUserSession(this ISession session, Guid userId, string role, string name, string email)

// Leer UserId (Guid)
Guid? GetUserId(this ISession session)

// Leer rol del usuario
string? GetUserRole(this ISession session)

// Leer nombre de usuario
string? GetUserName(this ISession session)

// Leer email
string? GetUserEmail(this ISession session)

// ¿Está autenticado? (UserId != null)
bool IsAuthenticated(this ISession session)

// ¿Está en un rol específico?
bool IsInRole(this ISession session, string role)

// Cerrar sesión (session.Clear())
void ClearUserSession(this ISession session)
```

---

## 6. ViewModels (Formularios de Registro)

### `RegisterEspectadorViewModel`
```csharp
public class RegisterEspectadorViewModel
{
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] [MinLength(6)] public string Password { get; set; }
    [Required] [Compare("Password")] public string ConfirmPassword { get; set; }
    [Required] [MaxLength(100)] public string Nombre { get; set; }
    [Required] [MaxLength(100)] public string ApellidoPaterno { get; set; }
    [MaxLength(100)] public string? ApellidoMaterno { get; set; }
}
```

### `RegisterJugadorViewModel`
```csharp
public class RegisterJugadorViewModel : RegisterEspectadorViewModel
{
    public string? PosicionPreferida { get; set; }  // Portero/Defensa/Medio/Delantero
    public int? NumeroCamiseta { get; set; }
    public decimal? Peso { get; set; }
    public decimal? Altura { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Dui { get; set; }   // Encriptado antes de guardar
    public string? Telefono { get; set; }
}
```

### `RegisterEntrenadorViewModel`
```csharp
public class RegisterEntrenadorViewModel : RegisterEspectadorViewModel
{
    [Required] public string NombreEquipo { get; set; }
    public string? AbreviacionEquipo { get; set; }  // 3 letras
    public IFormFile? Logo { get; set; }             // Subida a wwwroot/uploads/logos/
    public string? ColorPrimario { get; set; }
    public string? ColorSecundario { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Dui { get; set; }
    public string? Telefono { get; set; }
}
```

### `RegisterArbitroViewModel`
```csharp
public class RegisterArbitroViewModel : RegisterEspectadorViewModel
{
    [Required] public string NumeroLicencia { get; set; }
    public List<string> CategoriasArbitra { get; set; } = new();
    // Opciones: Juvenil, Amateur, Municipal, Regional, Nacional, Internacional
    public DateTime? FechaNacimiento { get; set; }
    public string? Dui { get; set; }
    public string? Telefono { get; set; }
}
```

---

## 7. ViewModels (Dashboard y Vistas)

### `ProximoPartidoDashVM`
```csharp
// Namespace: ControlTorneosFutbol.Pages.Calendario
public class ProximoPartidoDashVM
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreLocal { get; set; }
    public string NombreVisitante { get; set; }
    public string AbrevLocal { get; set; }        // 3 letras o primeras 3 del nombre
    public string AbrevVisitante { get; set; }
    public string NombreTorneo { get; set; }
    public int TorneoId { get; set; }
    public string Estado { get; set; }
    public string? Sede { get; set; }
    public bool EsMiPartido { get; set; }         // true si el usuario participa
    public string ColorTorneo { get; set; } = "#a3ff12";  // de TorneoColors[id%7]
}
```

### `PartidoCalendarioVM`
```csharp
// Namespace: ControlTorneosFutbol.Pages.Calendario
public class PartidoCalendarioVM
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreLocal { get; set; }
    public string NombreVisitante { get; set; }
    public string AbrevLocal { get; set; }
    public string AbrevVisitante { get; set; }
    public string? LogoLocal { get; set; }
    public string? LogoVisitante { get; set; }
    public int? GolesLocal { get; set; }
    public int? GolesVisitante { get; set; }
    public string Estado { get; set; }
    public string? Sede { get; set; }
    public int TorneoId { get; set; }
    public string NombreTorneo { get; set; }
    public string ColorTorneo { get; set; }
    public bool EsMiPartido { get; set; }
}
```

### `CheckPrePartidoViewModel`
```csharp
// Namespace: ControlTorneosFutbol.Pages.Arbitro.Partidos (o similar)
public class CheckPrePartidoViewModel
{
    public string Nombre { get; set; }          // Nombre del check
    public string Descripcion { get; set; }     // Detalle para mostrar
    public bool Pasa { get; set; }              // true = verde, false = rojo
    public string? MensajeError { get; set; }   // Si Pasa=false
}
```

---

## 8. TorneoViewModel (Crear/Editar)

```csharp
public class TorneoViewModel
{
    [Required] public string Nombre { get; set; }
    [Required] public string Formato { get; set; }     // FormatosTorneo.*
    [Required] public int LigaId { get; set; }
    [Required] public int TemporadaId { get; set; }
    [Required] public int CategoriaId { get; set; }
    [Required] public DateTime FechaInicio { get; set; }
    [Required] public DateTime FechaFin { get; set; }
    public int? NumeroEquiposMaximo { get; set; }
    public int NumeroEquiposMinimo { get; set; } = 2;

    // ConfiguracionTorneo
    public int PuntosPorVictoria { get; set; } = 3;
    public int PuntosPorEmpate { get; set; } = 1;
    public int PuntosPorDerrota { get; set; } = 0;
    public int TarjetasAmarillasParaSuspension { get; set; } = 3;
    public int PartidosSuspensionTarjetaRoja { get; set; } = 2;
    public int? MaximoJugadoresConvocados { get; set; } = 18;
    public int MinimoJugadoresTitulares { get; set; } = 11;
}
```

---

## 5b. IConvocatoriaService / ConvocatoriaService *(Sprint 4)*

**Archivo:** `Services/ConvocatoriaService.cs`

```csharp
// Obtener convocatoria de un equipo para un partido específico
Task<Convocatoria?> GetByPartidoEquipoAsync(int partidoId, int equipoId)

// Crear nueva o actualizar convocatoria existente (estado Borrador)
Task<Convocatoria> CrearOrActualizarAsync(int partidoId, int equipoId, int entrenadorId, string? observaciones)

// Agregar un jugador a la convocatoria con su rol y posición
Task<bool> AgregarJugadorAsync(int convocatoriaId, int ejtId, string rol, string? posicion)

// Quitar un jugador de la convocatoria
Task<bool> QuitarJugadorAsync(int convocatoriaId, int ejtId)

// Publicar convocatoria → estado Publicada + notificaciones opcionales a jugadores
Task<bool> PublicarAsync(int convocatoriaId, bool enviarNotificaciones = true)

// Jugador confirma/rechaza asistencia
Task<bool> ConfirmarAsistenciaAsync(int convocatoriaId, int ejtId, string estadoConfirmacion, string? nota)

// Lista de jugadores convocados con su rol y estado de confirmación
Task<List<ConvocatoriaJugador>> GetJugadoresAsync(int convocatoriaId)

// Contar cuántos tienen rol Titular
int ContarTitulares(List<ConvocatoriaJugador> jugadores)
```

---

## 5c. INotificacionService / NotificacionService *(Sprint 4)*

**Archivo:** `Services/NotificacionService.cs`

```csharp
// Crear una notificación para un usuario específico
Task CrearAsync(Guid usuarioId, string titulo, string mensaje,
    string tipo, int? referenciaId = null, string? urlAccion = null)

// Cantidad de notificaciones no leídas (para el bell badge)
Task<int> ContarNoLeidasAsync(Guid usuarioId)

// Últimas N notificaciones (default 10) — para dropdown del bell
Task<List<NotificacionInterna>> GetRecientesAsync(Guid usuarioId, int top = 10)

// Marcar una notificación como leída
Task MarcarLeidaAsync(int notificacionId, Guid usuarioId)

// Marcar todas las notificaciones del usuario como leídas
Task MarcarTodasLeidasAsync(Guid usuarioId)

// Notificar a todos los involucrados en un partido (árbitro + entrenadores + jugadores activos)
Task NotificarPartidoAsync(int partidoId, string titulo, string mensaje, string tipo)
```

---

## 5d. IPartidoEnVivoService / PartidoEnVivoService *(Sprint 4)*

**Archivo:** `Services/PartidoEnVivoService.cs`

```csharp
// Iniciar partido: estado Programado → EnCurso, seta FechaInicioReal, notifica
Task<bool> IniciarPartidoAsync(int partidoId, int arbitroId, bool enviarNotificaciones = true)

// Registrar incidencia (gol, tarjeta, cambio, lesión...)
Task<bool> RegistrarIncidenciaAsync(
    int partidoId, string tipo, int equipoId,
    int? ejtJugadorId, int minuto, string? descripcion = null,
    bool esAutogol = false, bool esPenal = false,
    int? ejtJugadorSalidaId = null)

// Recalcular GolesLocal/GolesVisitante contando incidencias tipo Gol
Task RecalcularGolesAsync(int partidoId)

// Finalizar partido: estado EnCurso → Finalizado, notifica
Task<bool> FinalizarPartidoAsync(int partidoId, int arbitroId, bool enviarNotificaciones = true)

// Lista de incidencias ordenadas por minuto
Task<List<IncidenciaPartido>> GetIncidenciasAsync(int partidoId)

// ViewModel resumen de estado actual del partido (para polling AJAX)
Task<PartidoEnVivoVM> GetEstadoEnVivoAsync(int partidoId)
```

---

## 5e. ViewModels Sprint 4

### `PartidoEnVivoVM`
```csharp
public class PartidoEnVivoVM
{
    public int Id { get; set; }
    public string Estado { get; set; }
    public int? MinutoActual { get; set; }
    public int GolesLocal { get; set; }
    public int GolesVisitante { get; set; }
    public string NombreLocal { get; set; }
    public string NombreVisitante { get; set; }
    public string? LogoLocal { get; set; }
    public string? LogoVisitante { get; set; }
    public List<IncidenciaVM> Incidencias { get; set; } = new();
}

public class IncidenciaVM
{
    public int Minuto { get; set; }
    public string Tipo { get; set; }
    public string NombreJugador { get; set; }
    public string NombreEquipo { get; set; }
    public bool EsAutogol { get; set; }
}
```

### `ConvocatoriaJugadorVM`
```csharp
public class ConvocatoriaJugadorVM
{
    public int EjtId { get; set; }
    public string NombreCompleto { get; set; }
    public int? NumeroCamiseta { get; set; }
    public string? Posicion { get; set; }
    public string Rol { get; set; }              // Titular / Suplente / Reserva
    public string EstadoConfirmacion { get; set; } // Pendiente / Confirmo / NoDisponible
    public string? Nota { get; set; }
}
```

---

## 9. Constantes del Dominio

```csharp
// Roles
RolesConstantes.Administrador  = "Administrador"
RolesConstantes.Espectador     = "Espectador"
RolesConstantes.Jugador        = "Jugador"
RolesConstantes.Entrenador     = "Entrenador"
RolesConstantes.Arbitro        = "Arbitro"

// Formatos de torneo
FormatosTorneo.TodosContraTodos    = "TodosContraTodos"
FormatosTorneo.EliminacionDirecta  = "EliminacionDirecta"
FormatosTorneo.GruposYEliminacion  = "GruposYEliminacion"

// Estados de torneo
EstadosTorneo.Borrador   = "Borrador"
EstadosTorneo.Activo     = "Activo"
EstadosTorneo.Finalizado = "Finalizado"

// Estados de partido
EstadosPartido.Programado    = "Programado"
EstadosPartido.EnCurso       = "EnCurso"
EstadosPartido.Finalizado    = "Finalizado"
EstadosPartido.Cancelado     = "Cancelado"
EstadosPartido.Reprogramado  = "Reprogramado"  // Sprint 4

// Tipos de equipo (Sprint 4)
TiposEquipo.Libre     = "Libre"
TiposEquipo.Masculino = "Masculino"
TiposEquipo.Femenino  = "Femenino"
TiposEquipo.Juvenil   = "Juvenil"    // Sub-18
TiposEquipo.Amateur   = "Amateur"    // 18+
TiposEquipo.Veterano  = "Veterano"   // 40+

// Géneros de persona (Sprint 4)
GenerosPersona.Masculino = "Masculino"
GenerosPersona.Femenino  = "Femenino"
GenerosPersona.Otro      = "Otro"

// Estados de convocatoria (Sprint 4)
EstadosConvocatoria.Borrador  = "Borrador"
EstadosConvocatoria.Publicada = "Publicada"

// Estados de confirmación de convocatoria (Sprint 4)
EstadosConfirmacion.Pendiente      = "Pendiente"
EstadosConfirmacion.Confirmo       = "Confirmo"
EstadosConfirmacion.NoDisponible   = "NoDisponible"

// Roles en convocatoria (Sprint 4)
RolesConvocatoria.Titular  = "Titular"
RolesConvocatoria.Suplente = "Suplente"
RolesConvocatoria.Reserva  = "Reserva"

// Estados de reporte de partido (Sprint 4)
EstadosReportePartido.Borrador  = "Borrador"
EstadosReportePartido.Enviado   = "Enviado"
EstadosReportePartido.Reabierto = "Reabierto"

// Tipos de incidencia (Sprint 4)
TiposIncidencia.Gol           = "Gol"
TiposIncidencia.TarjetaAmarilla = "TarjetaAmarilla"
TiposIncidencia.TarjetaRoja   = "TarjetaRoja"
TiposIncidencia.Expulsion     = "Expulsion"
TiposIncidencia.Lesion        = "Lesion"
TiposIncidencia.Cambio        = "Cambio"
TiposIncidencia.Suspension    = "Suspension"
TiposIncidencia.Retraso       = "Retraso"

// Tipos de notificación interna (Sprint 4)
TiposNotificacion.Partido      = "Partido"
TiposNotificacion.Convocatoria = "Convocatoria"
TiposNotificacion.Solicitud    = "Solicitud"
TiposNotificacion.Sistema      = "Sistema"

// Estados de solicitud
EstadosSolicitud.Pendiente = "Pendiente"
EstadosSolicitud.Aceptada  = "Aceptada"
EstadosSolicitud.Rechazada = "Rechazada"

// Estados de árbitro
EstadosAprobacionArbitro.Pendiente  = "Pendiente"
EstadosAprobacionArbitro.Aprobado   = "Aprobado"
EstadosAprobacionArbitro.Suspendido = "Suspendido"
EstadosAprobacionArbitro.Rechazado  = "Rechazado"

// Posiciones de jugador
PosicionesJugador.Portero   = "Portero"
PosicionesJugador.Defensa   = "Defensa"
PosicionesJugador.Medio     = "Medio"
PosicionesJugador.Delantero = "Delantero"
```

---

## 10. Métodos Estáticos de Utilidad

```csharp
// En Calendario/Index.cshtml.cs
public static string GetTorneoColor(int torneoId)
// Paleta: ["#a3ff12","#5ac8fa","#ff9500","#bf5af2","#ff2d55","#34c759","#ffd60a"]
// Uso:    TorneoColors[Math.Abs(torneoId) % 7]
// ⚠️ Duplicado como private static en cada dashboard backend

// Abreviación de equipo (fallback si es null)
var abrev = equipo.Abreviacion ?? equipo.Nombre.Substring(0, Math.Min(3, equipo.Nombre.Length)).ToUpper();
```

---

## 11. Rutas Disponibles

### Admin
| Ruta | Descripción |
|------|-------------|
| `/Admin/Dashboard` | Dashboard principal admin |
| `/Admin/Torneos/Index` | Lista torneos |
| `/Admin/Torneos/Create` | Crear torneo |
| `/Admin/Torneos/Edit?id=` | Editar torneo |
| `/Admin/Torneos/Details?id=` | Ver detalle |
| `/Admin/Torneos/InscribirEquipo?id=` | Inscribir equipo a torneo |
| `/Admin/Torneos/ReglasTorneo?id=` | Reglas personalizadas |
| `/Admin/Partidos/Index` | Lista partidos |
| `/Admin/Partidos/Crear` | Crear partido |
| `/Admin/Partidos/Editar?id=` | Editar partido |
| `/Admin/Partidos/Details?id=` | Ver partido |
| `/Admin/Usuarios/Index` | Gestión usuarios |
| `/Admin/Usuarios/Details?id=` | Ver usuario |
| `/Admin/Arbitros/Pendientes` | Aprobar/rechazar árbitros |
| `/Admin/Auditoria` | Log de accesos |

### Jugador
| Ruta | Descripción |
|------|-------------|
| `/Jugador/Dashboard` | Dashboard jugador |
| `/Jugador/BuscarEquipos` | Explorar equipos |
| `/Jugador/Solicitudes` | Mis solicitudes |
| `/Jugador/Partidos/Index` | Mis partidos |
| `/Jugador/Perfil` | Ver/editar perfil |

### Entrenador
| Ruta | Descripción |
|------|-------------|
| `/Entrenador/Dashboard` | Dashboard entrenador |
| `/Entrenador/MiEquipo` | Datos del equipo |
| `/Entrenador/Plantilla/Index` | Plantilla del equipo |
| `/Entrenador/Plantilla/Agregar` | Agregar jugador |
| `/Entrenador/Plantilla/Editar?id=` | Editar jugador |
| `/Entrenador/Solicitudes/Index` | Solicitudes pendientes |
| `/Entrenador/Partidos/Index` | Partidos del equipo |
| `/Entrenador/Partidos/Convocatoria?id=` | Detalle partido |

### Entrenador *(rutas Sprint 4)*
| Ruta | Descripción |
|------|-------------|
| `GET /Entrenador/Partidos/Convocatoria/Crear?partidoId=X` | Crear/editar convocatoria (borrador) |
| `POST /Entrenador/Partidos/Convocatoria/Crear` | Guardar y/o publicar convocatoria |
| `GET /Entrenador/Partidos/Convocatoria/Ver?convocatoriaId=X` | Vista convocatoria publicada (con confirmaciones) |
| `GET /Entrenador/Reglamento` | Reglamento del torneo del equipo activo |

### Árbitro *(rutas Sprint 4)*
| Ruta | Descripción |
|------|-------------|
| `/Arbitro/Dashboard` | Dashboard árbitro |
| `/Arbitro/Partidos/Index` | Mis partidos |
| `/Arbitro/Partidos/Jugadores?id=` | Plantillas del partido |
| `/Arbitro/Partidos/Verificacion?id=` | Verificación pre-partido (5 checks) |
| `GET /Arbitro/Partidos/EnVivo?partidoId=X` | Panel árbitro en vivo — marcador, incidencias |
| `POST /Arbitro/Partidos/EnVivo` | Registrar incidencia (gol, tarjeta, cambio...) |
| `GET /Arbitro/Partidos/EnVivo?handler=Estado&partidoId=X` | Polling AJAX — retorna `PartidoEnVivoVM` JSON |
| `GET/POST /Arbitro/Partidos/Reporte?partidoId=X` | Reporte post-partido + exportar PDF |

### Admin *(rutas Sprint 4)*
| Ruta | Descripción |
|------|-------------|
| `GET/POST /Admin/Partidos/Editar?id=` | Reprogramar partido con validación de conflicto horario |

### Jugador *(rutas Sprint 4)*
| Ruta | Descripción |
|------|-------------|
| `GET /Jugador/Partidos/Index` | Mis partidos — ahora incluye estado de convocatoria |
| `POST /Jugador/Partidos/Index?handler=Confirmar` | Confirmar/rechazar asistencia a convocatoria |

### Espectador
| Ruta | Descripción |
|------|-------------|
| `/Espectador/Index` | Dashboard espectador |
| `/Espectador/Torneos` | Lista de torneos |
| `/Espectador/TorneoDetalle?id=` | Detalle torneo |
| `/Espectador/Preferencias` | Configurar preferencias |

### Compartido
| Ruta | Descripción |
|------|-------------|
| `/Calendario/Index` | Calendario unificado |
| `/Account/Login` | Inicio de sesión |
| `/Account/Logout` | Cerrar sesión |
| `/Account/Register` | Registro espectador |
| `/Account/RegisterJugador` | Registro jugador |
| `/Account/RegisterEntrenador` | Registro entrenador |
| `/Account/RegisterArbitro` | Registro árbitro |
