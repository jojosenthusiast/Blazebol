# Development Log — ControlTorneosFutbol
> Historial de desarrollo, estado actual y planes futuros.
> Actualizado: 2026-03-12

---

## Sistema en Números (snapshot 2026-03-12)

| Métrica | Valor |
|---------|-------|
| Entidades (DbSets) | 27 (22 core + 5 Sprint 4+5) |
| Servicios (interfaces) | 8 |
| Páginas Razor (aprox.) | 65+ |
| Migraciones aplicadas | 7 |
| Roles del sistema | 5 |
| Historias de usuario | 25 |
| Historias completadas | 10 ✅ |
| En progreso Sprint 4 | 5 🔄 |
| Pendientes | 10 |

---

## ✅ Completado — Sprint 1 (Auth + Admin base)
**Período:** Feb 2026

- Autenticación completa: login, logout, lockout (5 intentos → 30 min), auditoría de accesos
- Registro de 4 tipos de usuario: Espectador, Jugador, Entrenador, Árbitro
- Cifrado AES-256-CBC para DUI/pasaporte, BCrypt para contraseñas
- Admin Dashboard básico con conteos
- CRUD completo de Torneos (crear, editar, desactivar, filtrar)
- Gestión de Usuarios: listar, activar/desactivar, desbloquear, exportar PDF
- Auditoría de accesos con IP, timestamp, motivo
- Aprobación de árbitros (Pendiente → Aprobado/Rechazado)
- Exportar PDF: usuarios, torneos, jugadores
- Seed inicial: 5 roles, ligas, temporadas, categorías, admin@ex.com

---

## ✅ Completado — Sprint 2 (Jugador/Entrenador/Árbitro base)
**Período:** Feb-Mar 2026

- Jugador: buscar equipos (filtro por torneo), enviar solicitud de unirse
- Jugador: ver solicitudes enviadas (estado pendiente/aceptada/rechazada)
- Entrenador: plantilla (listar jugadores activos por torneo)
- Entrenador: agregar jugador desde búsqueda (torneoId)
- Entrenador: editar número de camiseta
- Entrenador: desactivar jugador (soft delete)
- Entrenador: solicitudes de jugadores (3 tabs: entrantes, invitaciones enviadas, historial)
- Entrenador: sistema de invitaciones (esInvitacion flag en SolicitudJugadorEquipo)
- Árbitro: lista de partidos asignados
- Árbitro: lista de jugadores por partido (separada local/visitante)
- Espectador: preferencias (equipo favorito, notificaciones email)

---

## ✅ Completado — Sprint 3 (UI/UX + Fixture + Features avanzadas)
**Período:** Mar 2026

- **Sidebar rediseñado:** sin top navbar, perfil de usuario en footer con dropdown (Cerrar Sesión, Mi Perfil para Jugador), menú mobile con hamburger
- **Dashboard Admin profesional:** grid 2/3+1/3, actividad reciente (audit log), acciones rápidas, torneos activos, alertas de seguridad
- **Perfil modal Jugador:** popup AJAX desde sidebar footer (sin página separada), actualiza nombre en sesión
- **Fixture automático:** 3 formatos: TodosContraTodos (círculo), EliminacionDirecta (bracket), GruposYEliminacion (serpentino por grupo)
- **Asignar detalles fixture:** asignar fecha, sede y árbitro por partido
- **Páginas públicas:** landing de torneos, detalle torneo (clasificación, grupos, partidos), plantilla equipo, reglamento PDF
- **Calendario público:** vista de próximos partidos por torneo
- **Verificación pre-partido árbitro:** 5 checks automáticos + veredicto + botón "Iniciar Partido"
- **Jugador Partidos/Index:** vista de partidos (EnCurso/Próximos/Resultados) de su equipo
- **Entrenador Partidos/Index + Convocatoria básica:** partidos del equipo con "Ver Convocatoria"
- **WhatsApp URL en torneo:** campo en detalles de torneo, badge en Details
- **Sede Supabase local:** migración a 127.0.0.1:54322 (IPv4, no localhost)
- **Migraciones:** LandingPageTorneoFields, FixtureAutomatico, AddFixtureColumns

---

## 🔄 En Progreso — Sprint 4
**Período:** Mar 2026 — ACTIVO

### Completado dentro del Sprint 4
- ✅ Nuevas entidades migradas: `Convocatoria`, `ConvocatoriaJugador`, `ReportePartido`, `IncidenciaPartido`, `HistorialPartido`, `NotificacionInterna`
- ✅ Columnas nuevas en entidades existentes: `Persona.Genero`, `Equipo.TipoEquipo`, `Partido.MinutoActual/FechaInicioReal/RazonReprogramacion`
- ✅ Estado `Reprogramado` agregado a EstadosPartido
- ✅ Migración `Sprint4_5_ConvocatoriasReportesNotificaciones` aplicada a BD
- ✅ `ApplicationDbContext` actualizado con 6 nuevos DbSets + configuraciones

### En progreso (agente implementando)
- 🔄 `NotificacionService` + `ConvocatoriaService` + `PartidoEnVivoService`
- 🔄 Registro de servicios en `Program.cs`
- 🔄 `RegisterEntrenador`: campo TipoEquipo + validación
- 🔄 `RegisterJugador`: campo Genero
- 🔄 `BuscarEquipos`: filtrado por compatibilidad TipoEquipo ↔ Genero/Edad
- 🔄 `Entrenador/Partidos/Convocatoria/Crear` — crear/publicar convocatoria (US22+US23)
- 🔄 `Entrenador/Partidos/Convocatoria/Ver` — vista publicada con confirmaciones
- 🔄 `Jugador/Partidos/Index` — estado convocatoria + confirmar asistencia (US19+US23)
- 🔄 `Arbitro/Partidos/EnVivo` — panel de partido en vivo (registrar goles, tarjetas, cambios)
- 🔄 `Arbitro/Partidos/Reporte` — reporte post-partido + exportar PDF (US12)
- 🔄 `Admin/Partidos/Editar` — validación conflicto horario + historial cambios (US7)
- 🔄 Notificaciones sidebar (campanita con contador, lista de 10 recientes)
- 🔄 Fixes menores: US10 (mensaje plantilla vacía), US13 (filtros árbitro), US16 (badges Reprogramado/Sede pendiente), US24 (Entrenador/Reglamento)

---

## 📋 Próximo — Sprint 5
**Período:** Planificado post-Sprint 4

### US17 — Estadísticas reales de jugador
- Calcular desde `IncidenciaPartido`: goles, tarjetas, cambios
- Nueva página `/Jugador/Estadisticas` — tabla histórica por torneo
- Dashboard jugador con stats reales (actualmente muestra 0s)

### US6 — Afiche de partido/torneo + correo
- Integrar `MailKit` o `SendGrid` como servicio SMTP
- `EmailService` con `SendAfficheAsync(destinatarios, torneo/partido)`
- Diseñar plantilla HTML del afiche (o QuestPDF)
- Página `/Admin/Torneos/EnviarAfiche`

### US4 completar — Normalizar validaciones
- Componente parcial `_ValidationSummary.cshtml` reutilizable
- Validación conflicto horario ya incluida en Sprint 4 (Admin/Partidos/Editar)
- Revisar consistencia visual en todos los formularios

### US17 bloque equipo — Stats colectivas
- Posición en tabla de clasificación desde el dashboard del jugador
- GF/GC del equipo, últimos 5 resultados

---

## 💡 Mejoras QoL / Extras Recomendadas

> Estas no son historias formales pero agregan valor real al producto. Se recomienda implementarlas en orden de impacto.

### Corto plazo (fácil de implementar, alto impacto)
- **PDF de convocatoria** — el entrenador exporta la lista para llevarla al partido impresa
- **Copiar enlace de convocatoria** — botón "Compartir por WhatsApp" que abre wa.me con el estado
- **Paginación en tablas largas** — actualmente todos los registros se cargan en memoria; con 500+ usuarios/torneos se vuelve lento
- **Loading skeleton** — en lugar de pantalla en blanco durante carga AJAX, mostrar placeholders grises animados
- **Toast cuando llega notificación** — el polling actual (60s) ya trae el conteo; mostrar un toast discreto si cambia
- **Breadcrumbs en páginas anidadas** — `/Admin/Torneos/Edit` no tiene navegación de vuelta visible; agregar `<nav aria-label="breadcrumb">`
- **Confirmación antes de cerrar sesión** — SweetAlert2 "¿Cerrar sesión?"
- **Buscar equipo por nombre** — en BuscarEquipos actualmente solo filtra por torneo; agregar input de texto

### Mediano plazo (semanas de desarrollo, alto valor)
- **Sanciones acumuladas automáticas** — si un jugador acumula X tarjetas amarillas (configurable en ConfiguracionTorneo), el sistema lo suspende automáticamente del próximo partido y notifica al entrenador
- **Importar plantilla desde CSV** — el entrenador sube un archivo con nombre, DOB, número, posición; el sistema los registra en masa
- **Foto de perfil para jugadores/entrenadores** — upload con preview, redimensionado automático a 200x200px
- **Historial de rendimiento del jugador** — ver goles/tarjetas acumuladas en múltiples torneos/equipos con gráfica de barras
- **Estadísticas de árbitro** — partidos arbitrados, tarjetas promedio por partido, torneos asignados
- **Widget tabla de posiciones embeddable** — URL pública tipo `/embed/clasificacion?torneoId=X` para mostrar en iframe en sitios externos
- **PWA (Progressive Web App)** — el árbitro puede usar el panel en vivo sin conexión (service worker, caché offline)
- **Perfil público del jugador** — URL `/Jugador/Publico/{id}` con stats públicas (sin datos sensibles)

### Largo plazo (semanas-meses, visión de producto)
- **SignalR real-time** — reemplazar polling AJAX por WebSocket para actualizaciones instantáneas en el panel del árbitro y en espectadores
- **App móvil** — React Native o Flutter usando una API REST que se exponga del backend (árbitro en campo con tablet, jugador ve resultados en el teléfono)
- **Sistema de pagos** — Stripe/PayPal para cobrar inscripción a torneos, pagos de árbitros
- **Análisis de video** — integración con plataformas de clips deportivos, enlazar incidencias a segundos de video
- **Gestión de instalaciones deportivas** — módulo de canchas con calendario de disponibilidad, evitar doble reserva
- **Portal de sponsors** — stats de visibilidad del logo en la plataforma, reportes de impresiones
- **API REST pública** — para futuras apps móviles o integraciones de terceros
- **Dashboard analítico admin** — gráficas de tendencias (goles por jornada, equipos más activos, árbitros con más partidos)

---

## 🐛 Deuda Técnica Conocida

| Item | Descripción | Prioridad |
|------|-------------|-----------|
| IV fijo AES | El IV de AES-256-CBC usa los primeros 16 bytes de la clave (debería ser random por cifrado) | Media |
| UserName en sesión | `session.UserName = email.Split('@')[0]` — temporal, no refleja nombre real | Baja |
| Sin transacciones DB | `AuthService` no usa transacciones (comentado en el código) | Media |
| Placeholder Usuarios/Pendientes | `Pages/Admin/Usuarios/Pendientes.cshtml` existe sin code-behind | Alta |
| Sin tests | Cero xUnit/NUnit en el proyecto — todo se prueba manualmente | Alta |
| Sin paginación | Todas las tablas cargan todos los registros de BD | Media |
| Sin rate limiting | El middleware de lockout protege login pero no hay rate limit general en la API | Baja |

---

## 🗂 Estructura de Documentación

| Archivo | Propósito |
|---------|-----------|
| `Scripts/USER_STORIES.md` | Las 25 historias con criterios verbatim + estado actual |
| `Scripts/DEVELOPMENT_LOG.md` | Este archivo — historial, en progreso, planes |
| `Scripts/DATABASE_SCHEMA.md` | Schema completo de BD con todas las entidades y relaciones |
| `Scripts/API_REFERENCE.md` | Servicios (interfaces + métodos), rutas, ViewModels |
| `Scripts/BACKEND_PATTERNS.md` | Patrones de código reutilizables (PageModel, AJAX, DI, etc.) |
| `Scripts/FRONTEND_GUIDE.md` | CSS variables, componentes Bootstrap, guía UI/UX |
| `Scripts/PROJECT_STATUS.md` | Estado por sprint, conteos, environment vars |
| `Scripts/SPRINT_2_TASKS.md` | Histórico de tareas Sprint 2 + Sprint 4 tracking |
| `Scripts/TESTING_GUIDE.md` | Flujos de prueba por rol, credenciales, errores comunes |
| `ANALISIS_SISTEMA.md` (raíz) | Análisis completo del sistema, estado de 25 historias, plan de desarrollo |
