# SPRINT_2_TASKS.md — ControlTorneosFutbol

> Checklist detallado de las historias de usuario del Sprint 2.
> Estado actual: **COMPLETADO** ✅ (todas implementadas en Sprint 3)

---

## Historias de Usuario Sprint 2

### H14 — Verificación Pre-Partido (Árbitro) ✅

**Como árbitro**, quiero verificar automáticamente el estado del partido antes de iniciarlo para asegurar que se cumplen los requisitos mínimos.

**Implementado en:** `Pages/Arbitro/Partidos/Verificacion.cshtml(.cs)`

- [x] Panel de 5 checks automáticos:
  - [x] Check 1: Estado del partido es "Programado"
  - [x] Check 2: Cuórum equipo local (≥ `MinimoJugadoresTitulares` del ConfiguracionTorneo)
  - [x] Check 3: Cuórum equipo visitante (≥ `MinimoJugadoresTitulares`)
  - [x] Check 4: Cuerpo técnico equipo local (entrenador asignado)
  - [x] Check 5: Cuerpo técnico equipo visitante (entrenador asignado)
- [x] Banner de veredicto: verde "✓ Partido listo para comenzar" si todos pasan
- [x] Banner rojo "✗ Partido NO puede iniciarse" si alguno falla
- [x] `PuedeProceeder = Checks.All(c => c.Pasa)` en PageModel
- [x] Vista de jugadores convocados por equipo (tabla de plantilla)

**Archivos:**
- `Pages/Arbitro/Partidos/Verificacion.cshtml`
- `Pages/Arbitro/Partidos/Verificacion.cshtml.cs`

---

### H15 — Vista de Partidos del Árbitro ✅

**Como árbitro**, quiero ver mis partidos asignados organizados por estado.

**Implementado en:** `Pages/Arbitro/Partidos/Index.cshtml(.cs)`

- [x] Lista de partidos filtrada por árbitro logueado
- [x] Badges de estado (Programado / EnCurso / Finalizado / Cancelado)
- [x] Botón "Verificar Partido" → `/Arbitro/Partidos/Verificacion?id=`
- [x] Empty state con ícono `bi-calendar-x`
- [x] Enlace a Calendario desde Dashboard de árbitro

**Archivos:**
- `Pages/Arbitro/Partidos/Index.cshtml`
- `Pages/Arbitro/Partidos/Index.cshtml.cs`

---

### H16 — Vista de Partidos del Jugador ✅

**Como jugador**, quiero ver los partidos de mi equipo organizados por estado.

**Implementado en:** `Pages/Jugador/Partidos/Index.cshtml(.cs)`

- [x] Lookup: usuario → persona → jugador → equiposjugadorestorneos → equipo
- [x] Filtra partidos donde `equipolocalid = equipoId OR equipovisitanteid = equipoId`
- [x] Secciones: En Curso, Próximos, Resultados
- [x] Badge del equipo del jugador (local o visitante)
- [x] Goles mostrados en partidos finalizados
- [x] Sede con ícono `bi-geo-alt`

**Archivos:**
- `Pages/Jugador/Partidos/Index.cshtml`
- `Pages/Jugador/Partidos/Index.cshtml.cs`

---

### H17 — Vista de Partidos del Entrenador + Convocatoria ✅

**Como entrenador**, quiero ver los partidos de mi equipo y la convocatoria detallada.

**Implementado en:**
- `Pages/Entrenador/Partidos/Index.cshtml(.cs)` — lista de partidos
- `Pages/Entrenador/Partidos/Convocatoria.cshtml(.cs)` — detalle completo

- [x] Lookup: usuario → persona → entrenador → equiposentrenadores → equipo
- [x] Lista de partidos con botón "Ver Convocatoria"
- [x] Convocatoria: plantillas de ambos equipos + tabla de posiciones del torneo
- [x] Distingue partido como local vs visitante

**Archivos:**
- `Pages/Entrenador/Partidos/Index.cshtml`
- `Pages/Entrenador/Partidos/Index.cshtml.cs`
- `Pages/Entrenador/Partidos/Convocatoria.cshtml`
- `Pages/Entrenador/Partidos/Convocatoria.cshtml.cs`

---

### H18 — Gestión de Plantilla (Entrenador) ✅

**Como entrenador**, quiero buscar jugadores y agregarlos a la plantilla de mi equipo.

**Implementado en:** `Pages/Entrenador/Plantilla/`

- [x] Index: lista jugadores del equipo en el torneo seleccionado
- [x] Agregar: búsqueda por nombre + filtro por torneoId
- [x] Mostrar posición, edad, email del jugador candidato
- [x] Crear `EquipoJugadorTorneo` al aceptar
- [x] Editar: cambiar número de camiseta o posición
- [x] `MiEquipo.cshtml` — ver datos generales del equipo

**Archivos:**
- `Pages/Entrenador/Plantilla/Index.cshtml(.cs)`
- `Pages/Entrenador/Plantilla/Agregar.cshtml(.cs)`
- `Pages/Entrenador/Plantilla/Editar.cshtml(.cs)`
- `Pages/Entrenador/MiEquipo.cshtml(.cs)`

---

### H19 — Sistema de Solicitudes ✅

**Como jugador**, quiero solicitar unirme a un equipo.
**Como entrenador**, quiero invitar jugadores y gestionar solicitudes.

**Implementado en:**
- `Pages/Jugador/Solicitudes.cshtml(.cs)` — jugador ve sus solicitudes
- `Pages/Entrenador/Solicitudes/Index.cshtml(.cs)` — entrenador acepta/rechaza

- [x] `SolicitudJugadorEquipo.EsInvitacion` distingue dirección de la solicitud
- [x] Jugador puede enviar solicitud desde BuscarEquipos
- [x] Entrenador puede invitar desde Plantilla/Agregar
- [x] Entrenador ve: nombre, email, teléfono, posición, edad del jugador
- [x] Jugador ve: nombre del equipo, logo, entrenador, torneo
- [x] Al aceptar solicitud: crear `EquipoJugadorTorneo` si no existe
- [x] Estados: Pendiente → Aceptada / Rechazada

**Archivos:**
- `Pages/Jugador/Solicitudes.cshtml`
- `Pages/Jugador/Solicitudes.cshtml.cs`
- `Pages/Entrenador/Solicitudes/Index.cshtml`
- `Pages/Entrenador/Solicitudes/Index.cshtml.cs`

---

### H20 — Buscar Equipos (Jugador) ✅

**Como jugador**, quiero explorar equipos disponibles para solicitar unirme.

**Implementado en:** `Pages/Jugador/BuscarEquipos.cshtml(.cs)`

- [x] Lista todos los equipos activos con logo, nombre, abreviación
- [x] Muestra nombre del entrenador
- [x] Botón "Solicitar" que crea solicitud (navegación o directo)
- [x] Filtro por torneo

**Archivos:**
- `Pages/Jugador/BuscarEquipos.cshtml`
- `Pages/Jugador/BuscarEquipos.cshtml.cs`

---

### H21 — Admin CRUD de Partidos ✅

**Como administrador**, quiero gestionar el calendario de partidos.

**Implementado en:** `Pages/Admin/Partidos/`

- [x] Index: lista todos los partidos con filtros (torneo, estado, equipo)
- [x] Crear: seleccionar torneo, equipos, árbitro, fecha, sede
- [x] Editar: cambiar estado (Programado→EnCurso→Finalizado), goles, árbitro, sede
- [x] Details: ver detalle completo del partido
- [x] Validación: no permitir mismo equipo como local y visitante

**Archivos:**
- `Pages/Admin/Partidos/Index.cshtml(.cs)`
- `Pages/Admin/Partidos/Crear.cshtml(.cs)`
- `Pages/Admin/Partidos/Editar.cshtml(.cs)`
- `Pages/Admin/Partidos/Details.cshtml(.cs)`

---

### H22 — Calendario Mensual Role-Aware ✅

**Como cualquier usuario**, quiero ver un calendario visual con los partidos del mes.

**Implementado en:** `Pages/Calendario/Index.cshtml(.cs)`

- [x] Grid de 2 meses (mes actual + siguiente)
- [x] Dots de color por torneo en cada día con partidos
- [x] Panel lateral: partidos de hoy, partidos del día seleccionado, próximos 7
- [x] Countdown al próximo partido
- [x] Filtro por torneo (GET form)
- [x] Navegación mes anterior/siguiente
- [x] `EsMiPartido` flag por rol:
  - [x] Jugador → via `EquiposJugadoresTorneos`
  - [x] Entrenador → via `EquiposEntrenadores`
  - [x] Árbitro → via `Partido.ArbitroId`
  - [x] Admin/Espectador → todos los partidos
- [x] Botón "Ver Detalles" role-aware:
  - [x] Admin → `/Admin/Partidos/Editar?id=`
  - [x] Árbitro → `/Arbitro/Partidos/Verificacion?id=`
  - [x] Entrenador → `/Entrenador/Partidos/Convocatoria?id=`
  - [x] Espectador → `/Espectador/TorneoDetalle?id=`
- [x] Leyenda de colores por torneo

**Archivos:**
- `Pages/Calendario/Index.cshtml`
- `Pages/Calendario/Index.cshtml.cs`

---

### H23 — Widget Próximos Partidos en Dashboards ✅

**Como usuario**, quiero ver los próximos partidos directamente en mi dashboard.

- [x] Admin Dashboard: próximos 5 partidos globales
- [x] Jugador Dashboard: próximos partidos de su equipo
- [x] Entrenador Dashboard: próximos partidos de su equipo
- [x] Árbitro Dashboard: card "Ver Calendario" (card de acción)
- [x] `ProximoPartidoDashVM` como ViewModel compartido (namespace `Pages.Calendario`)
- [x] `.match-mini-card` CSS en site.css
- [x] Color dot por torneo: `TorneoColors[torneoId % 7]`

**Archivos:**
- `Pages/Admin/Dashboard.cshtml(.cs)`
- `Pages/Jugador/Dashboard.cshtml(.cs)`
- `Pages/Entrenador/Dashboard.cshtml(.cs)`
- `Pages/Arbitro/Dashboard.cshtml(.cs)`
- `wwwroot/css/site.css` (clases `.match-mini-*`)

---

### H24 — Exportar PDF ✅

**Como administrador**, quiero exportar reportes en PDF.

- [x] PDF de usuarios con info personal (tabla)
- [x] PDF de torneos (nombre, estado, formato, fechas)
- [x] PDF de jugadores (nombre, posición, equipo)
- [x] QuestPDF 2026.2.1 configurado con licencia Community
- [x] Botones PDF con `btn-info btn-icon` + `bi-file-earmark-pdf`

**Archivos:**
- `Services/PdfService.cs`
- `Services/IPdfService.cs`

---

### H25 — Preferencias Espectador ✅

**Como espectador**, quiero guardar mis preferencias de visualización.

**Implementado en:** `Pages/Espectador/Preferencias.cshtml(.cs)`

- [x] Equipo favorito (select de todos los equipos)
- [x] Notificaciones por email (checkbox)
- [x] Email de contacto para notificaciones
- [x] 1:1 con usuario (UNIQUE en `usuarioid`)
- [x] Upsert: crea si no existe, actualiza si ya existe

**Archivos:**
- `Pages/Espectador/Preferencias.cshtml`
- `Pages/Espectador/Preferencias.cshtml.cs`

---

## Resumen de Estado

| Historia | Título | Estado |
|---------|--------|--------|
| H14 | Verificación pre-partido árbitro | ✅ Completo |
| H15 | Partidos del árbitro | ✅ Completo |
| H16 | Partidos del jugador | ✅ Completo |
| H17 | Partidos del entrenador + convocatoria | ✅ Completo |
| H18 | Gestión de plantilla | ✅ Completo |
| H19 | Sistema de solicitudes | ✅ Completo |
| H20 | Buscar equipos | ✅ Completo |
| H21 | Admin CRUD partidos | ✅ Completo |
| H22 | Calendario mensual | ✅ Completo |
| H23 | Widget próximos partidos dashboards | ✅ Completo |
| H24 | Exportar PDF | ✅ Completo |
| H25 | Preferencias espectador | ✅ Completo |

---

## Sprint 4 — EN PROGRESO 🔄

### Entidades y Migración

- [x] `Convocatoria` + `ConvocatoriaJugador` — convocatorias de equipos para partido
- [x] `ReportePartido` + `IncidenciaPartido` — reporte árbitro + incidencias en vivo
- [x] `HistorialPartido` — log de reprogramaciones/cancelaciones
- [x] `NotificacionInterna` — sistema de notificaciones cross-rol
- [x] Columnas nuevas: `Persona.Genero`, `Equipo.TipoEquipo`, `Partido.MinutoActual/FechaInicioReal/RazonReprogramacion`
- [x] Estado `Reprogramado` en `EstadosPartido`
- [x] Migración `Sprint4_5_ConvocatoriasReportesNotificaciones` aplicada a BD

### Servicios

- [ ] `IConvocatoriaService` / `ConvocatoriaService` — gestión de convocatorias
- [ ] `INotificacionService` / `NotificacionService` — notificaciones internas
- [ ] `IPartidoEnVivoService` / `PartidoEnVivoService` — panel en vivo + incidencias
- [ ] Registrar en `Program.cs`

### Registro de Entrenador / Jugador

- [ ] `RegisterEntrenador.cshtml(.cs)` — campo `TipoEquipo` + validación
- [ ] `RegisterJugador.cshtml(.cs)` — campo `Genero`
- [ ] `BuscarEquipos.cshtml.cs` — filtro compatibilidad `TipoEquipo ↔ Genero/Edad`

### Convocatorias (US22 + US23)

- [ ] `Entrenador/Partidos/Convocatoria/Crear.cshtml(.cs)` — crear/publicar convocatoria
  - Jugadores del equipo en tarjetas con toggle Titular/Suplente/Reserva
  - Validación: al menos 11 titulares antes de publicar
  - Botón "Publicar" → estado Publicada + notifica jugadores
- [ ] `Entrenador/Partidos/Convocatoria/Ver.cshtml(.cs)` — vista convocatoria publicada
  - Lista de jugadores con estado de confirmación (Pendiente/Confirmo/NoDisponible)

### Jugador — Convocatoria (US19 + US23)

- [ ] `Jugador/Partidos/Index.cshtml(.cs)` — actualizar para mostrar estado convocatoria
  - Badge "Convocado" si está en una convocatoria publicada
  - Botón "Confirmar Asistencia" / "No Disponible"

### Árbitro — Panel en Vivo (US12)

- [ ] `Arbitro/Partidos/EnVivo.cshtml(.cs)` — panel en vivo
  - Marcador central (GolesLocal vs GolesVisitante)
  - Botones de acción rápida: GOL, TARJETA AMARILLA, TARJETA ROJA, CAMBIO
  - Modal: seleccionar equipo → seleccionar jugador → confirmar
  - Lista de incidencias cronológica
  - Polling AJAX cada 10s (`?handler=Estado&partidoId=X`)
  - Botón "Finalizar Partido"
- [ ] `Arbitro/Partidos/Reporte.cshtml(.cs)` — reporte post-partido
  - Resumen: incidencias del partido, marcador final
  - Observaciones adicionales, minutos jugados, prórroga
  - Exportar PDF del reporte

### Reprogramación (US7)

- [ ] `Admin/Partidos/Editar.cshtml.cs` — validación de conflicto horario
  - Verificar que árbitro no tiene otro partido en ±2h
  - Registrar `HistorialPartido` si cambia fecha o estado → Reprogramado
  - Notificar a árbitro + entrenadores del partido

### Notificaciones Sidebar

- [ ] `_Sidebar.cshtml` — campanita con badge de no leídas
  - Polling AJAX cada 60s para actualizar conteo
  - Dropdown con últimas 10 notificaciones
  - "Marcar todas como leídas"

### Fixes menores

- [ ] US10: mensaje en plantilla vacía del equipo
- [ ] US13: filtros en lista de árbitro (fecha, equipo)
- [ ] US16: badges `Reprogramado` y `Sede pendiente` en partidos
- [ ] US24: `Entrenador/Reglamento` — reglamento del torneo activo del equipo
