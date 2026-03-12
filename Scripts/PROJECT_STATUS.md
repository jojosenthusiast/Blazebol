# PROJECT_STATUS.md — ControlTorneosFutbol

> Última actualización: 2026-03-08
> Estado general: Sprint 3 completado ✅

---

## 1. Stack Técnico Completo

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Framework | ASP.NET Core | 8.0 |
| Patrón | Razor Pages | — |
| ORM | Entity Framework Core | 8.0.0 |
| BD Driver | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 |
| Base de datos | PostgreSQL (Supabase local) | — |
| Puerto local | 54322 (IPv4: 127.0.0.1) | — |
| Auth | Custom session-based | — |
| Hashing | BCrypt.Net-Next | 4.0.3 |
| Encriptación | AES-256-CBC (DUI/Pasaporte) | — |
| PDF | QuestPDF | 2026.2.1 |
| UI | Bootstrap 5 + Bootstrap Icons CDN | — |
| JS | jQuery + jQuery Validation | CDN |
| Alertas | SweetAlert2 | CDN |
| Deploy | Railway (producción) | — |

### Connection String (desarrollo local)
```
Host=127.0.0.1;Database=postgres;Username=postgres;Password=postgres;Port=54322
```
> ⚠️ Usar `127.0.0.1`, NO `localhost` — Windows resuelve `localhost` → `::1` (IPv6) pero Supabase solo escucha en IPv4.

### Connection String (Railway producción)
```
Host=${{PGHOST}};Database=${{PGDATABASE}};Username=${{PGUSER}};Password=${{PGPASSWORD}};Port=${{PGPORT}}
```

---

## 2. Estado de Sprints

### Sprint 1 — Completado ✅

| Historia | Descripción | Estado |
|---------|-------------|--------|
| H1 | Inscripción de equipos a torneos | ✅ |
| H2 | Registro de jugadores | ✅ |
| H3 | Registro de entrenadores | ✅ |
| H4 | Registro de árbitros (con workflow de aprobación) | ✅ |
| H5 | Login y autenticación | ✅ |
| H6 | Lockout por intentos fallidos (5 intentos → 30 min) | ✅ |
| H7 | Auditoría de accesos | ✅ |
| H8 | Dashboard Administrador | ✅ |
| H9 | CRUD Torneos completo + PDF export | ✅ |
| H10 | Gestión de Usuarios (activar/desactivar/desbloquear) | ✅ |
| H11 | Panel de árbitros pendientes (aprobar/rechazar) | ✅ |
| H12 | Dashboard Espectador con lista de torneos | ✅ |
| H13 | Detalle de torneo para espectador (clasificación, grupos, partidos) | ✅ |

### Sprint 2 — Completado ✅

| Historia | Descripción | Estado |
|---------|-------------|--------|
| H14 | Verificación pre-partido para árbitros (5 checks automáticos) | ✅ |
| H15 | Vista de partidos del árbitro | ✅ |
| H16 | Vista de partidos del jugador | ✅ |
| H17 | Vista de partidos del entrenador + convocatoria | ✅ |
| H18 | Gestión de plantilla (búsqueda + agregar jugadores) | ✅ |
| H19 | Sistema de solicitudes (jugador → equipo / invitación entrenador → jugador) | ✅ |
| H20 | Buscar equipos para jugadores | ✅ |
| H21 | Admin CRUD de partidos (crear, editar estado/goles/árbitro/sede) | ✅ |
| H22 | Calendario mensual role-aware con 2 meses, side panel, countdown | ✅ |
| H23 | Widget "Próximos Partidos" en dashboards de Admin/Jugador/Entrenador | ✅ |
| H24 | Exportar PDF (usuarios, torneos, jugadores) | ✅ |
| H25 | Preferencias Espectador (equipo favorito, notificaciones) | ✅ |

### Sprint 3 — Completado ✅

| Característica | Descripción | Estado |
|---------------|-------------|--------|
| Sidebar | Sin top navbar, perfil en footer con dropdown, mobile hamburger | ✅ |
| Dashboard Admin | Grid 2/3+1/3, audit log, acciones rápidas, torneos activos | ✅ |
| Perfil modal Jugador | Popup AJAX desde sidebar, actualiza nombre en sesión | ✅ |
| Fixture automático | 3 formatos: TodosContraTodos, EliminacionDirecta, GruposYEliminacion | ✅ |
| Páginas públicas | Landing torneos, detalle torneo (clasificación/grupos/partidos), reglamento PDF | ✅ |
| Calendario público | Vista próximos partidos por torneo | ✅ |
| Verificación pre-partido | 5 checks + veredicto + botón "Iniciar Partido" | ✅ |
| WhatsApp en torneo | Campo URL + badge en Details | ✅ |

### Sprint 4 — EN PROGRESO 🔄

| Característica | Historia | Estado |
|---------------|---------|--------|
| 6 nuevas entidades + migración aplicada | — | ✅ |
| TipoEquipo en registro de entrenador | US22 | 🔄 |
| Género en registro de jugador | US19 | 🔄 |
| BuscarEquipos: filtro por compatibilidad | US20 | 🔄 |
| Entrenador/Convocatoria/Crear (US22+US23) | US22+US23 | 🔄 |
| Entrenador/Convocatoria/Ver con confirmaciones | US23 | 🔄 |
| Jugador/Partidos: estado convocatoria + confirmar | US19+US23 | 🔄 |
| Árbitro/Partidos/EnVivo — panel en vivo | US12 | 🔄 |
| Árbitro/Partidos/Reporte + PDF | US12 | 🔄 |
| Admin/Partidos/Editar: conflicto horario + historial | US7 | 🔄 |
| Notificaciones internas (campanita sidebar) | — | 🔄 |
| Services: NotificacionService, ConvocatoriaService, PartidoEnVivoService | — | 🔄 |

---

## 3. Todas las Entidades de la BD

### Módulo: Usuarios y Autenticación (4 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Usuario` | `usuarios` | `Guid` | Cuenta de acceso: email, passwordHash, lockout, activo |
| `Rol` | `roles` | `int` | Roles del sistema: Administrador, Espectador, Jugador, Entrenador, Arbitro |
| `UsuarioRol` | `usuariosroles` | `int` | M:N junction entre usuarios y roles |
| `AuditoriaAccesos` | `auditoriaaccesos` | `int` | Log de intentos de login (exitosos y fallidos) |

### Módulo: Personas y Roles Especializados (4 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Persona` | `personas` | `Guid` | Datos personales: nombre, apellidos, DUI (encriptado), teléfono |
| `Jugador` | `jugadores` | `int` | Posición, número camiseta, peso, altura |
| `Entrenador` | `entrenadores` | `int` | Datos del entrenador (vínculo con Persona) |
| `Arbitro` | `arbitros` | `int` | Número licencia, categorías (JSON), estado aprobación |

### Módulo: Estructura Organizacional (3 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Liga` | `ligas` | `int` | Organización que gestiona el torneo |
| `Temporada` | `temporadas` | `int` | Año/nombre de la temporada |
| `Categoria` | `categorias` | `int` | Categoría etaria/nivel del torneo |

### Módulo: Torneos (3 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Torneo` | `torneos` | `int` | Nombre, formato, estado, fechas, max/min equipos |
| `ConfiguracionTorneo` | `configuracionestorneo` | `int` | Puntos, desempates, tarjetas, convocatoria |
| `ReglaPersonalizadaTorneo` | `reglaspersonalizadastorneo` | `int` | Reglas adicionales ordenadas por torneo |

### Módulo: Equipos (5 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Equipo` | `equipos` | `int` | Nombre, abreviación (3 letras), logo, colores, sede |
| `EquipoTorneo` | `equipostorneos` | `int` | Equipo inscrito en torneo |
| `EquipoEntrenador` | `equiposentrenadores` | `int` | Relación equipo-entrenador |
| `EquipoJugadorTorneo` | `equiposjugadorestorneos` | `int` | Jugador en equipo+torneo, número camiseta |
| `SolicitudJugadorEquipo` | `solicitudesjugadorequipo` | `int` | Solicitud o invitación (EsInvitacion flag) |

### Módulo: Competencia (2 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Grupo` | `grupos` | `int` | Grupo dentro de un torneo (fase de grupos) |
| `Partido` | `partidos` | `int` | Fecha, sede, equipos, árbitro, goles, estado |

### Módulo: Extras (1 tabla)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `PreferenciaEspectador` | `preferenciasespectador` | `int` | Equipo favorito, notificaciones email |

### Módulo: Sprint 4 — Convocatorias, Reportes, Notificaciones (6 tablas)

| Entidad C# | Tabla PostgreSQL | PK | Descripción |
|-----------|-----------------|-----|-------------|
| `Convocatoria` | `convocatorias` | `int` | Convocatoria de un equipo para un partido (Borrador/Publicada) |
| `ConvocatoriaJugador` | `convocatoriasjugadores` | `int` | Jugador en convocatoria con rol (Titular/Suplente/Reserva) y confirmación |
| `ReportePartido` | `reportespartido` | `int` | Reporte post-partido del árbitro (Borrador/Enviado/Reabierto) |
| `IncidenciaPartido` | `incidenciaspartido` | `int` | Goles, tarjetas, cambios, lesiones por partido y minuto |
| `HistorialPartido` | `historialpartido` | `int` | Registro de reprogramaciones/cancelaciones de partido |
| `NotificacionInterna` | `notificacionesinternas` | `int` | Notificaciones cross-rol con badge de no leídas |

**Total: 28 tablas / 27 entidades C#** (22 base + 6 Sprint 4)

---

## 4. Constraints Críticos

```
usuarios.email          → UNIQUE
roles.nombre            → UNIQUE
usuariosroles(usuarioid, rolid) → UNIQUE
arbitros.numerolicencia → UNIQUE
equipos.nombre          → UNIQUE
equipostorneos(equipoid, torneoid) → UNIQUE
equiposjugadorestorneos(jugadorid, torneoid) → UNIQUE
equiposjugadorestorneos(equipoid, torneoid, numerocamiseta) → UNIQUE
preferenciasespectador.usuarioid → UNIQUE (1:1)
```

### Comportamientos ON DELETE

| Relación | Behavior |
|----------|---------|
| Usuario → Persona | CASCADE |
| Torneo → Partidos | CASCADE |
| Torneo → ReglasPersonalizadas | CASCADE |
| Partido → Árbitro | SET NULL |
| Partido → EquipoLocal/Visitante | RESTRICT |
| AuditoriaAccesos → Usuario | SET NULL |
| SolicitudJugadorEquipo → EntrenadorResponsable | SET NULL |

---

## 5. Estructura de Carpetas del Proyecto

```
ControlTorneosFutbol/
├── Data/
│   ├── ApplicationDbContext.cs       ← EF context, 21 DbSets, OnModelCreating
│   └── Entities/
│       ├── Usuario.cs, Rol.cs, UsuarioRol.cs, AuditoriaAccesos.cs
│       ├── Persona.cs, Jugador.cs, Entrenador.cs, Arbitro.cs
│       ├── Liga.cs, Temporada.cs, Categoria.cs
│       ├── Torneo.cs                  ← + FormatosTorneo, EstadosTorneo (static classes)
│       ├── ConfiguracionTorneo.cs
│       ├── ReglaPersonalizadaTorneo.cs
│       ├── Equipo.cs
│       ├── EquipoTorneo.cs, EquipoEntrenador.cs, EquipoJugadorTorneo.cs
│       ├── SolicitudJugadorEquipo.cs  ← + EstadosSolicitud
│       ├── Grupo.cs
│       ├── Partido.cs                 ← + EstadosPartido
│       └── PreferenciaEspectador.cs
├── Helpers/
│   └── SessionHelper.cs              ← Extension methods para ISession
├── Migrations/
│   ├── 20260226XXXXXX_InitialCreate.cs
│   ├── 20260228041058_AddPartidoEntity.cs
│   └── 20260301002438_Sprint3Fields.cs  ← esinvitacion + abreviacion
├── Models/
│   └── ViewModels/                   ← DTOs para formularios y vistas
├── Pages/
│   ├── Account/                      ← Login, Logout, Register (4 tipos)
│   ├── Admin/
│   │   ├── Dashboard.cshtml(.cs)
│   │   ├── Torneos/                  ← Index, Create, Edit, Details, InscribirEquipo, ReglasTorneo
│   │   ├── Usuarios/                 ← Index, Details
│   │   ├── Partidos/                 ← Index, Crear, Editar, Details
│   │   ├── Arbitros/                 ← Pendientes
│   │   └── Auditoria.cshtml(.cs)
│   ├── Jugador/
│   │   ├── Dashboard.cshtml(.cs)
│   │   ├── BuscarEquipos.cshtml(.cs)
│   │   ├── Solicitudes.cshtml(.cs)
│   │   ├── Perfil.cshtml(.cs)
│   │   └── Partidos/Index.cshtml(.cs)
│   ├── Entrenador/
│   │   ├── Dashboard.cshtml(.cs)
│   │   ├── MiEquipo.cshtml(.cs)
│   │   ├── Plantilla/                ← Index, Agregar, Editar
│   │   ├── Solicitudes/Index.cshtml(.cs)
│   │   └── Partidos/                 ← Index, Convocatoria
│   ├── Arbitro/
│   │   ├── Dashboard.cshtml(.cs)
│   │   └── Partidos/                 ← Index, Jugadores, Verificacion
│   ├── Espectador/
│   │   ├── Index.cshtml(.cs)         ← Dashboard
│   │   ├── Torneos.cshtml(.cs)
│   │   ├── TorneoDetalle.cshtml(.cs)
│   │   └── Preferencias.cshtml(.cs)
│   ├── Calendario/
│   │   └── Index.cshtml(.cs)         ← Calendario unificado todos los roles
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _Sidebar.cshtml
├── Services/
│   ├── AuthService.cs / IAuthService.cs
│   ├── TorneoService.cs / ITorneoService.cs
│   ├── PdfService.cs / IPdfService.cs
│   ├── EncryptionService.cs / IEncryptionService.cs
│   ├── ConvocatoriaService.cs / IConvocatoriaService.cs  ← Sprint 4
│   ├── NotificacionService.cs / INotificacionService.cs  ← Sprint 4
│   └── PartidoEnVivoService.cs / IPartidoEnVivoService.cs ← Sprint 4
├── Scripts/
│   └── seed_demo.sql                 ← Seed completo: 24 equipos, 240 jugadores, 140 partidos
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   └── uploads/logos/               ← Logos de equipos subidos
└── Program.cs                       ← DI, middleware, DB seed inicial
```

---

## 6. Servicios Implementados

### AuthService (`IAuthService`)
- `RegisterEspectadorAsync(model)` → `(bool, string, Guid?)`
- `RegisterJugadorAsync(model)` → `(bool, string, Guid?)`
- `RegisterEntrenadorAsync(model)` → `(bool, string, Guid?)`
- `RegisterArbitroAsync(model)` → `(bool, string, Guid?)`
- `LoginAsync(email, password)` → `(bool, string, Usuario?)`
- `EmailExistsAsync(email)` → `bool`

### TorneoService (`ITorneoService`)
- `GetTorneosAsync(estado?, anio?, categoriaId?)` → `List<Torneo>`
- `GetTorneoByIdAsync(id)` → `Torneo?`
- `CreateTorneoAsync(model)` → `(bool, string, int?)`
- `UpdateTorneoAsync(id, model)` → `(bool, string)`
- `DeleteTorneoAsync(id)` → `(bool, string)`

### PdfService (`IPdfService`)
- `GenerarReporteUsuariosPdf(usuarios)` → `byte[]`
- `GenerarReporteTorneosPdf(torneos)` → `byte[]`
- `GenerarReporteJugadoresPdf(jugadores)` → `byte[]`

### EncryptionService (`IEncryptionService`)
- `HashPassword(password)` → `string` (BCrypt)
- `VerifyPassword(password, hash)` → `bool`
- `Encrypt(plainText)` → `string` (AES-256-CBC)
- `Decrypt(cipherText)` → `string`

### ConvocatoriaService (`IConvocatoriaService`) — Sprint 4
- `GetByPartidoEquipoAsync(partidoId, equipoId)` → `Convocatoria?`
- `CrearOrActualizarAsync(...)` → `Convocatoria`
- `AgregarJugadorAsync(...)` → `bool`
- `QuitarJugadorAsync(...)` → `bool`
- `PublicarAsync(convocatoriaId, notif)` → `bool`
- `ConfirmarAsistenciaAsync(...)` → `bool`
- `GetJugadoresAsync(convocatoriaId)` → `List<ConvocatoriaJugador>`

### NotificacionService (`INotificacionService`) — Sprint 4
- `CrearAsync(usuarioId, titulo, mensaje, tipo, referenciaId?, urlAccion?)` → `Task`
- `ContarNoLeidasAsync(usuarioId)` → `int`
- `GetRecientesAsync(usuarioId, top=10)` → `List<NotificacionInterna>`
- `MarcarLeidaAsync(notificacionId, usuarioId)` → `Task`
- `MarcarTodasLeidasAsync(usuarioId)` → `Task`
- `NotificarPartidoAsync(partidoId, titulo, mensaje, tipo)` → `Task`

### PartidoEnVivoService (`IPartidoEnVivoService`) — Sprint 4
- `IniciarPartidoAsync(partidoId, arbitroId, notif)` → `bool`
- `RegistrarIncidenciaAsync(...)` → `bool`
- `RecalcularGolesAsync(partidoId)` → `Task`
- `FinalizarPartidoAsync(partidoId, arbitroId, notif)` → `bool`
- `GetIncidenciasAsync(partidoId)` → `List<IncidenciaPartido>`
- `GetEstadoEnVivoAsync(partidoId)` → `PartidoEnVivoVM`

**Total servicios: 7** (4 base + 3 Sprint 4)

---

## 7. Datos Base Poblados (Seed en Program.cs)

Al iniciar la app, `Program.cs` siembra si las tablas están vacías:

```csharp
// Roles (5 registros)
"Administrador", "Espectador", "Jugador", "Entrenador", "Arbitro"

// Liga base
Liga: "Liga Municipal de Fútbol"

// Temporada base
Temporada: "2026", Año: 2026

// Categorías (5 registros)
"Sub-15", "Sub-17", "Sub-20", "Amateur", "Profesional"

// Usuario admin
Email: "admin@ex.com", Role: "Administrador"
```

### Seed Demo Completo (`Scripts/seed_demo.sql`)

> ⚠️ El script hace TRUNCATE de TODO, incluyendo el admin. Re-insertar admin manualmente después.

```
24 equipos (entrenadores1..24@demo.com)
240 jugadores (10 por equipo, jug_e{1-24}_{1-10}@demo.com)
8 árbitros (arbitro1..8@demo.com)  — EstadoAprobacion: Aprobado
8 espectadores (espectador1..8@demo.com)
6 torneos (2024–2027, marzo 2026 tiene 3 torneos activos simultáneamente)
140 partidos (round-robin scheduling con intervalos variables)
```

**Contraseña para todos los usuarios demo:** `Usuario123!`
**Hash BCrypt:** `$2a$11$6LJfBW9j3vRNQZVKHQjwzuVVMVG5h5Y6K5Oqh8Hd9bN3iMvK2wXKq`

---

## 8. Variables de Entorno Railway

| Variable | Valor |
|----------|-------|
| `PORT` | `8080` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${{PORT}}` |
| `PGHOST` | (provisto por Railway) |
| `PGDATABASE` | (provisto por Railway) |
| `PGUSER` | (provisto por Railway) |
| `PGPASSWORD` | (provisto por Railway) |
| `PGPORT` | (provisto por Railway) |
