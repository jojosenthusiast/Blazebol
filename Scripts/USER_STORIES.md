# User Stories — ControlTorneosFutbol
> 25 historias de usuario con criterios de aceptación completos y estado de implementación.
> Actualizado: 2026-03-12

## Leyenda de estado
| Ícono | Estado | Rango |
|-------|--------|-------|
| ✅ | Completa | 100% |
| 🟡 | Casi completa | 80-99% |
| 🟠 | Parcial | 40-79% |
| 🔄 | En progreso Sprint 4 | — |
| ❌ | No iniciada | 0-39% |

---

## ADMINISTRADOR

---

### US1 — Gestión y calendarización de torneos ✅ 100%
> **"Como administrador quiero llevar una buena gestión (calendarización) para los torneos de fútbol"**

**Criterios de aceptación:**
- El sistema permite crear, editar, consultar y desactivar torneos
- Para crear un torneo el formulario exige: Nombre, temporada/año, fechas (inicio/fin), categoría/división, formato (liga o eliminación), estado (borrador/activo/finalizado)
- El administrador puede ver un listado de torneos con filtros por estado, año/temporada y categoría
- Un torneo finalizado no permite modificaciones de estructura, solo lectura de información

**Implementado en:**
- `Pages/Admin/Torneos/Index.cshtml` — listado con filtros
- `Pages/Admin/Torneos/Create.cshtml` — formulario de creación
- `Pages/Admin/Torneos/Edit.cshtml` — edición (bloqueada si Finalizado)
- `Pages/Admin/Torneos/Details.cshtml` — vista de detalle
- `Services/TorneoService.cs` — lógica CRUD + CanEditTorneoAsync

**Extras implementados:** generación automática de fixture (3 formatos), asignación de árbitros/sedes, landing page pública por torneo, URL grupo WhatsApp.

---

### US2 — Base de datos completa ✅ 100%
> **"Como administrador quiero una base de datos para tener información de todos los torneos, jugadores, entrenadores, árbitros registrados en el sistema"**

**Criterios de aceptación:**
- La aplicación persiste datos en una BD relacional: Torneos, Equipos, Jugadores, Entrenadores, Árbitros, Partidos
- Integridad referencial: no se puede registrar un jugador sin equipo (o sin vínculo válido)
- No se puede crear un partido sin torneo y sin equipos válidos del torneo
- Un jugador no puede estar dos veces en el mismo equipo/torneo
- Un árbitro/entrenador no puede tener dos perfiles duplicados con el mismo identificador
- El sistema permite exportar listados básicos (PDF) al menos de jugadores y torneos

**Implementado en:**
- `Data/ApplicationDbContext.cs` — 27 DbSets, restricciones UNIQUE e integridad referencial
- `Services/PdfService.cs` — GenerateUsuariosReport, GenerateTorneosReport, GenerateJugadoresReport
- Índices únicos en: (JugadorId+TorneoId), (NumeroLicencia árbitro), (PersonaId entrenador)

---

### US3 — Autenticación y seguridad ✅ 100%
> **"Como administrador quiero que el software cuente con métodos de autenticación para una mejor seguridad del software"**

**Criterios de aceptación:**
- El sistema incluye inicio de sesión y cierre de sesión para usuarios con credenciales únicas
- Las páginas administrativas requieren autorización por rol
- Tras N intentos fallidos, la cuenta queda temporalmente bloqueada o se requiere una espera

**Implementado en:**
- `Pages/Account/Login.cshtml` + `Logout.cshtml`
- `Services/AuthService.cs` — LoginAsync con BCrypt, lockout tras 5 intentos (30 min)
- `Program.cs` — middleware custom por ruta (/admin, /jugador, /entrenador, /arbitro, /espectador)
- `Data/Entities/AuditoriaAccesos.cs` — log completo de intentos

---

### US4 — Validaciones y mensajes de error 🟠 65%
> **"Como administrador quiero gestionar mensajes de error y validaciones en pantalla, para reducir errores de captura"**

**Criterios de aceptación:**
- Los formularios críticos (torneos, equipos, registro de personas, partidos) tienen validaciones en cliente y servidor
- Los mensajes de error se muestran cerca del campo
- Los mensajes son claros y accionables ("El nombre es obligatorio", "La fecha fin debe ser mayor a la fecha inicio")
- Si ocurre un error inesperado (excepción), el usuario ve un mensaje genérico
- Los errores se manejan de forma consistente en toda la app (misma estructura visual)

**Implementado:** validaciones en torneos/registro/perfil ✅ | mensajes cerca del campo ✅ | página de error genérico ✅
**Pendiente:** consistencia visual en partidos/árbitro/entrenador ⚠️ | validación conflicto horario en partidos ⚠️

---

### US5 — Cifrado de datos sensibles ✅ 100%
> **"Como administrador quiero cifrar datos sensibles (contraseñas, identificaciones), para proteger información personal"**

**Criterios de aceptación:**
- Las contraseñas no se guardan en texto plano, se guardarán usando un hash
- Los campos sensibles definidos se almacenan protegidos, al menos cifrado a nivel aplicación
- Los campos de contraseña estarán ocultos por asteriscos

**Implementado en:**
- `Services/EncryptionService.cs` — BCrypt.Net-Next (passwords), AES-256-CBC (DUI/pasaporte)
- Campos `<input type="password">` en todos los formularios
- `Persona.IdentificacionCifrada` — siempre cifrado antes de persistir

---

### US6 — Afiche de partido/torneo y envío por correo ❌ 0%
> **"Como administrador quiero generar un afiche de un partido o torneo y enviarlo por correo, para automatizar la difusión de información publicitaria"**

**Criterios de aceptación:**
- El administrador puede seleccionar un torneo o partido y generar automáticamente un afiche digital
- El afiche incluye: nombre del torneo, equipos, fecha, hora y sede
- El sistema permite enviar el afiche directamente a uno o varios correos electrónicos desde la plataforma
- El sistema muestra confirmación de envío exitoso o error de envío
- El afiche utiliza una plantilla prediseñada para mantener el formato uniforme

**Pendiente Sprint 5:** integrar MailKit/SendGrid + plantilla QuestPDF + página `/Admin/Torneos/EnviarAfiche`

---

### US7 — Reprogramación sin choques de horario 🔄 En progreso Sprint 4
> **"Como administrador quiero tener una cancelación adecuada para evitar choques entre juegos"**

**Criterios de aceptación:**
- El administrador puede cambiar la fecha y hora de un partido programado
- El sistema valida que no exista conflicto de horario con los mismos equipos o sede
- Al reprogramar, el estado del partido cambia a "Reprogramado"
- El sistema registra un historial de cambios de fecha/hora
- Los roles involucrados (árbitro, entrenador, jugador) reciben una notificación interna del cambio

**Implementado:** editar fecha en `Pages/Admin/Partidos/Editar.cshtml` ✅
**En progreso:** validación conflicto ⚙️ | estado "Reprogramado" ⚙️ | HistorialPartido ⚙️ | NotificacionInterna ⚙️

---

## ESPECTADOR

---

### US8 — Ver resultados de partidos 🟡 90%
> **"Como espectador quiero ver los resultados de los partidos para ver el desempeño de mi equipo"**

**Criterios de aceptación:**
- El espectador podrá acceder únicamente con permisos de lectura
- La pantalla permite ver el contenido del torneo, fecha y equipo
- Cada resultado muestra equipos, marcadores, fecha, sede y estado
- Si el partido aún no se ha jugado, se mostrará cómo programado

**Implementado en:** `Pages/Espectador/TorneoDetalle.cshtml`, `Pages/Torneos/Details.cshtml`
**Pendiente menor:** sede siempre visible en el listado de partidos públicos ⚠️

---

### US9 — Ver clasificación del torneo 🟡 90%
> **"Como espectador quiero ver la clasificación del torneo, para conocer qué equipos van liderando"**

**Criterios de aceptación:**
- El espectador puede ver la tabla de posiciones por torneo y, si aplica, por grupo/categoría
- La tabla se actualiza cuando un partido pasa a estado Finalizado/Cerrado
- La tabla se ordena automáticamente por puntos y luego por los criterios de desempate (diferencia de goles, goles a favor)

**Implementado en:** `Pages/Espectador/TorneoDetalle.cshtml` — clasificación en tiempo real desde partidos Finalizados
**Pendiente menor:** bracket visual para fase eliminatoria en formato GruposYEliminacion ⚠️

---

### US10 — Ver lista de jugadores de cada equipo 🟠 75%
> **"Como espectador quiero ver la lista de jugadores de cada equipo, para conocer a los participantes"**

**Criterios de aceptación:**
- El espectador puede acceder al perfil de un equipo y ver su plantilla
- La lista muestra datos públicos: mínimo nombre y número de camiseta; posición opcional
- La plantilla muestra solo jugadores activos
- No se muestran datos sensibles (identificación, fecha de nacimiento completa, dirección, etc.)
- Si un equipo no tiene jugadores cargados, se muestra un mensaje diciendo que la plantilla no está disponible

**Implementado en:** `Pages/Torneos/Equipo.cshtml` — muestra jugadores activos sin datos sensibles ✅
**Pendiente:** confirmar mensaje "plantilla no disponible" cuando no hay jugadores ⚠️

---

### US11 — Espectador: crear cuenta ✅ 100%
> **"Como espectador quiero crear una cuenta, para guardar mis preferencias dentro del sistema"**

**Criterios de aceptación:**
- El espectador puede registrarse con un formulario que solicita mínimo: nombre, correo, contraseña
- El sistema valida correo con formato correcto y contraseña con requisitos mínimos
- Al registrarse, el usuario queda con rol Espectador por defecto y acceso a funciones públicas
- El sistema impide registrar dos cuentas con el mismo correo

**Implementado en:** `Pages/Account/RegisterEspectador.cshtml` + `Services/AuthService.RegisterEspectadorAsync`

---

## ÁRBITRO

---

### US12 — Generar reportes de partidos 🔄 En progreso Sprint 4
> **"Como árbitro quiero generar reportes para retroalimentación de los partidos"**

**Criterios de aceptación:**
- El árbitro puede generar un reporte por partido únicamente si el partido está en estado Finalizado o Cerrado
- El reporte incluye: torneo, equipos, marcador, fecha/hora, sede, árbitro y un campo de observaciones
- El árbitro puede registrar incidencias en el reporte: tarjetas, expulsiones, lesiones, suspensión/retraso, comentarios
- El árbitro puede guardar como borrador y luego enviar/cerrar el reporte
- Una vez "enviado/cerrado", el reporte queda solo lectura; corrección requiere estado "reabierto" por administrador
- El reporte se puede exportar a PDF o visualizar en una vista imprimible

**Entidades:** `ReportePartido`, `IncidenciaPartido` — migradas ✅
**En progreso:** `Pages/Arbitro/Partidos/Reporte.cshtml` ⚙️ | `Pages/Arbitro/Partidos/EnVivo.cshtml` (registra incidencias en tiempo real) ⚙️ | PDF export ⚙️

---

### US13 — Historial de partidos arbitrados 🟠 55%
> **"Como árbitro quiero ver mi historial de partidos arbitrados, para llevar control de mi desempeño"**

**Criterios de aceptación:**
- El árbitro tiene una vista "Mi historial" que lista partidos donde él está asignado como árbitro
- La lista muestra: fecha, torneo, equipos, sede, estado y marcador (si existe)
- Permite filtros por rango de fechas, torneo y estado (programado, jugado, finalizado)
- Permite ordenar por fecha (asc/desc)
- Al abrir un partido del historial, el árbitro puede ver el detalle y el reporte si existe
- Solo el árbitro autenticado puede ver su historial

**Implementado:** `Pages/Arbitro/Partidos/Index.cshtml` — lista de partidos asignados ✅
**Pendiente:** filtro por rango de fechas ⚠️ | ordenamiento asc/desc ⚠️ | link al reporte desde detalle ⚠️

---

### US14 — Verificar identidad de jugadores y entrenadores ✅ 100%
> **"Como árbitro quiero verificar la identidad de jugadores y entrenadores, para evitar suplantaciones"**

**Criterios de aceptación:**
- Antes del partido, el árbitro puede acceder a una pantalla de "Verificación" para el partido asignado
- La pantalla muestra listado de participantes (jugadores y entrenadores) con datos básicos: nombre, número de camiseta, ID
- La verificación solo puede hacerse para el partido asignado al árbitro y antes de que inicie

**Implementado en:** `Pages/Arbitro/Partidos/Verificacion.cshtml` — verificación completa con 5 checks automáticos (quórum ≥11 local, quórum ≥11 visitante, cuerpo técnico local, cuerpo técnico visitante, estado partido) + veredicto APROBADO/RECHAZADO + botón "Iniciar Partido"

---

### US15 — Visualizar lista de jugadores inscritos ✅ 100%
> **"Como árbitro quiero visualizar la lista de jugadores inscritos antes del partido, para verificar alineaciones"**

**Criterios de aceptación:**
- El árbitro puede ver la lista de jugadores por equipo para el partido asignado
- La lista incluye nombre, número, posición y estado
- La lista se muestra separada por Equipo local y Equipo visitante
- La lista se mantiene disponible incluso sin reporte final como consulta previa al juego
- La vista no permite edición de jugadores

**Implementado en:** `Pages/Arbitro/Partidos/Jugadores.cshtml` y también en `Verificacion.cshtml`

---

### US16 — Ver sede, fecha y hora del partido 🟠 60%
> **"Como árbitro quiero ver la sede, fecha y hora del partido, para llegar puntualmente"**

**Criterios de aceptación:**
- En "Mis partidos" y en el detalle del partido, se muestra sede, dirección, fecha y hora
- Si el partido es reprogramado, el árbitro ve el nuevo horario y el sistema muestra un aviso de "Actualizado"
- El árbitro recibe una notificación cuando cambian fecha, hora o sede
- La hora se muestra en formato consistente
- Si falta información, el sistema muestra un mensaje "Sede pendiente de asignación"

**Implementado:** sede/fecha/hora en listado ✅ | formato consistente ✅
**Pendiente:** badge "Reprogramado/Actualizado" ⚙️ (depende de US7) | notificación ⚙️ | badge "Sede pendiente" ⚠️

---

## JUGADOR

---

### US17 — Ver estadísticas individuales y colectivas ❌ 5%
> **"Como jugador quiero ver mis estadísticas para evaluar mi rendimiento individual y colectivo"**

**Criterios de aceptación:**
- El jugador puede ver una sección "Mis estadísticas" con datos del torneo
- Se muestran estadísticas individuales: partidos jugados, goles, asistencias, tarjetas amarillas y tarjetas rojas
- Se muestran estadísticas colectivas: posición en tabla, puntos, GF/GC, últimos resultados
- Los valores se actualizan cuando el partido se marca como Finalizado
- Si no hay datos aún, el sistema muestra "Aún no hay estadísticas registradas"
- El jugador solo puede ver sus estadísticas junto a su trayectoria en diferentes torneos y equipos

**Estado:** el dashboard muestra 0s como placeholder ⚠️
**Bloqueo:** requiere `IncidenciaPartido` (ya migrada en Sprint 4) y que el árbitro registre incidencias (US12 en progreso)
**Sprint 5:** cálculo de stats desde IncidenciasPartido + página `/Jugador/Estadisticas`

---

### US18 — Editar perfil y datos personales ✅ 100%
> **"Como jugador quiero editar mi perfil y datos personales, para mantener mi información actualizada"**

**Criterios de aceptación:**
- El jugador puede acceder a "Mi perfil" y editar datos permitidos: nombre, teléfono, posición preferida, número de camiseta
- El sistema valida campos requeridos y longitud máxima de texto
- En caso de error, el sistema muestra mensajes claros y no borra la información ya ingresada

**Implementado en:** `Pages/Jugador/Perfil.cshtml` — modal popup moderno desde sidebar footer con AJAX (OnGetDatosAsync + OnPostGuardarAsync), actualiza nombre en sesión al guardar

---

### US19 — Conocer posición en la alineación 🔄 En progreso Sprint 4
> **"Como jugador quiero conocer mi posición en la alineación, para prepararme adecuadamente"**

**Criterios de aceptación:**
- En el detalle del partido, el jugador puede ver si está: Titular, Suplente o No convocado
- Si el entrenador aún no publicó alineación, el sistema muestra "Alineación pendiente"
- La información se muestra solo para partidos donde el jugador pertenece al equipo participante
- La alineación visible incluye al menos la posición asignada

**En progreso:** `Pages/Jugador/Partidos/Index.cshtml` — agregar estado de convocatoria por partido ⚙️ | depende de Convocatoria publicada (US22/US23 en progreso)

---

### US20 — Ver fechas y horarios de partidos ✅ 100%
> **"Como jugador quiero ver fechas y horarios de partidos, para organizar mi tiempo"**

**Criterios de aceptación:**
- El jugador tiene una vista "Mis partidos" con los próximos partidos de su equipo
- Cada partido muestra fecha, hora, rival, sede y estado
- Si un partido es reprogramado, el jugador ve el nuevo horario y una marca de cambio
- Si no hay partidos programados, se muestra "No hay partidos próximos"

**Implementado en:** `Pages/Jugador/Partidos/Index.cshtml` — EnCurso/Próximos/Resultados con toda la info
**Pendiente menor:** marca "Reprogramado" cuando dependa de US7 ⚙️

---

### US21 — Panel resumen dashboard jugador 🟡 80%
> **"Como jugador quiero ver un panel resumen con mi información principal, para tener todo en un solo lugar"**

**Criterios de aceptación:**
- Al iniciar sesión, el jugador ve un "Dashboard" con próximos 1–3 partidos, estado en la convocatoria, resumen corto de estadísticas y accesos rápidos
- Si falta información, el panel muestra estados "pendiente / sin datos" sin errores

**Implementado:** próximos partidos ✅ | accesos rápidos ✅ | estados "sin datos" ✅
**Pendiente:** estado en convocatoria (depende US23) ⚙️ | stats reales (depende US17) ⚙️

---

## ENTRENADOR

---

### US22 — Hacer alineaciones para los partidos 🔄 En progreso Sprint 4
> **"Como entrenador quiero hacer alineaciones para los partidos"**

**Criterios de aceptación:**
- Puede crear alineación solo para partidos de su equipo
- Selecciona titulares y suplentes desde la plantilla
- Validar pertenencia y estado activo
- Validar mínimo de 11 titulares
- Al publicar queda visible según configuración

**En progreso:** entidad `Convocatoria` + `ConvocatoriaJugador` migradas ✅ | `Pages/Entrenador/Partidos/Convocatoria/Crear.cshtml` ⚙️
**Nota:** US22 y US23 se implementan conjuntamente en la misma entidad Convocatoria (rol Titular/Suplente/Reserva)

---

### US23 — Hacer convocatorias para los partidos 🔄 En progreso Sprint 4
> **"Como entrenador quiero hacer convocatorias para los partidos"**

**Criterios de aceptación:**
- Puede crear convocatoria para partidos de su equipo
- Selecciona jugadores activos
- Validar máximo de convocados si aplica
- Enviar genera notificaciones
- Jugadores marcan estado: Confirmó / No puedo / Pendiente

**En progreso:** `ConvocatoriaService` ⚙️ | publicar genera NotificacionInterna a cada jugador convocado ⚙️ | `Pages/Jugador/Partidos/Index.cshtml` muestra estado de confirmación ⚙️

---

### US24 — Acceder al reglamento y normas 🟠 50%
> **"Como entrenador quiero acceder al reglamento y normas, para cumplir requisitos"**

**Criterios de aceptación:**
- Existe sección "Reglamento"
- Si no está cargado, muestra "Reglamento no disponible"
- El sistema verifica el cumplimiento automático de sanciones
- El entrenador solo visualiza si su equipo tiene violaciones

**Implementado:** reglamento público `/Torneos/Reglamento` ✅ | configuración en admin `/Admin/Torneos/ReglasTorneo` ✅
**Pendiente:** página `/Entrenador/Reglamento` específica para el entrenador ⚠️ | verificación automática de sanciones ❌

---

### US25 — Agregar, editar o eliminar jugadores del equipo ✅ 100%
> **"Como entrenador quiero agregar, editar o eliminar jugadores de mi equipo, para mantener la plantilla actualizada"**

**Criterios de aceptación:**
- Agregar jugadores con nombre, fecha de nacimiento y número de camiseta
- Evitar duplicados por ID en el mismo equipo/torneo
- Editar datos
- Desactivar en lugar de borrar para conservar historial

**Implementado en:**
- `Pages/Entrenador/Plantilla/Agregar.cshtml` — buscar y agregar jugador via solicitud/invitación
- `Pages/Entrenador/Plantilla/Editar.cshtml` — actualizar número de camiseta
- `Pages/Entrenador/Plantilla/Index.cshtml` — desactivar (flag activo en EquipoJugadorTorneo)
- `Pages/Entrenador/Solicitudes/Index.cshtml` — aprobar solicitudes con asignación de número

---

## Resumen General

| Estado | Historias | IDs |
|--------|-----------|-----|
| ✅ Completa (100%) | 10 | US1, US2, US3, US5, US11, US14, US15, US18, US20, US25 |
| 🟡 Casi completa (80-99%) | 3 | US8, US9, US21 |
| 🟠 Parcial (40-79%) | 6 | US4, US10, US13, US16, US24, US16 |
| 🔄 En progreso Sprint 4 | 5 | US7, US12, US19, US22, US23 |
| ❌ No iniciada | 1 | US6 |
| ❌ Bloqueada por Sprint 4 | 1 | US17 (necesita incidencias registradas) |

**Total completado o casi:** 13/25 (52%) — **En progreso activo:** 5/25 — **Restante:** 7/25
