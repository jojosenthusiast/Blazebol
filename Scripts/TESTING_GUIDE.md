# TESTING_GUIDE.md — ControlTorneosFutbol

> Guía completa de pruebas: credenciales, flujos por rol, datos de prueba, errores comunes.

---

## 1. Credenciales por Rol

### Producción (Railway)

| Rol | Email | Contraseña |
|-----|-------|-----------|
| Administrador | `admin@ex.com` | `Admin123!` |
| Espectador | `espectador1@demo.com` | `Usuario123!` |
| Espectador 2..8 | `espectador{2-8}@demo.com` | `Usuario123!` |
| Jugador (equipo 1) | `jug_e1_1@demo.com` hasta `jug_e1_10@demo.com` | `Usuario123!` |
| Jugador (equipo N) | `jug_e{1-24}_{1-10}@demo.com` | `Usuario123!` |
| Entrenador | `entrenador1@demo.com` hasta `entrenador24@demo.com` | `Usuario123!` |
| Árbitro | `arbitro1@demo.com` hasta `arbitro8@demo.com` | `Usuario123!` |

### Desarrollo Local

> Mismas credenciales. Correr `seed_demo.sql` en Supabase Studio (http://127.0.0.1:54323) para poblar.

---

## 2. Configuración Local

### Prerrequisitos
1. [Supabase CLI](https://supabase.com/docs/guides/cli) instalado
2. .NET 8 SDK instalado
3. Proyecto clonado

### Iniciar entorno local
```bash
# 1. Iniciar Supabase local
supabase start

# 2. Correr migraciones EF Core
dotnet ef database update --project ControlTorneosFutbol

# 3. (Opcional) Poblar datos demo
# Abrir Supabase Studio: http://127.0.0.1:54323
# SQL Editor → pegar contenido de Scripts/seed_demo.sql → Run

# 4. Re-insertar admin (seed_demo.sql hace TRUNCATE)
# SQL Editor:
INSERT INTO public.usuarios (id, email, passwordhash, nombreusuario, activo)
VALUES (gen_random_uuid(), 'admin@ex.com',
    '$2a$11$HASH_AQUI', 'admin', true);
-- (ver MEMORY.md para el hash correcto o generarlo con BCrypt workFactor=11)

# 5. Iniciar app
dotnet run --project ControlTorneosFutbol
# → http://localhost:5000 (o el puerto asignado)
```

---

## 3. Flujos de Prueba por Rol

### 3.1 Administrador

#### Login
1. Ir a `/Account/Login`
2. Email: `admin@ex.com` / Password: `Admin123!`
3. → Redirige a `/Admin/Dashboard`

#### Verificar Dashboard
- [ ] Stats: Total Torneos, Usuarios, Árbitros Pendientes, Partidos Programados
- [ ] Widget "Próximos Partidos" muestra partidos reales (mínimo 1 si hay seed)
- [ ] Botón "Ver Calendario" lleva al calendario unificado
- [ ] Tabla "Torneos Recientes" muestra torneos

#### CRUD Torneos
- [ ] `/Admin/Torneos/Index` — lista con filtros funcionando
- [ ] Crear torneo: llenar todos los campos, guardar → aparece en lista
- [ ] Editar torneo: cambiar estado de Borrador → Activo
- [ ] Exportar PDF: botón azul con `bi-file-earmark-pdf` → descarga `torneos.pdf`
- [ ] Inscribir equipo: desde Details del torneo

#### Gestión de Partidos
- [ ] `/Admin/Partidos/Crear` — seleccionar torneo, 2 equipos distintos, árbitro, fecha
- [ ] `/Admin/Partidos/Editar?id=X` — cambiar estado a EnCurso, agregar goles
- [ ] Validar: no se puede poner el mismo equipo local y visitante

#### Gestión de Usuarios
- [ ] `/Admin/Usuarios/Index` — lista todos los usuarios
- [ ] Filtrar por rol/estado
- [ ] Desactivar usuario → estado cambia
- [ ] Reactivar usuario
- [ ] Desbloquear (solo aparece si tiene FechaBloqueoFin activa)

#### Árbitros Pendientes
- [ ] `/Admin/Arbitros/Pendientes` — lista árbitros con EstadoAprobacion = Pendiente
- [ ] Aprobar → estado cambia a Aprobado, árbitro puede recibir partidos
- [ ] Rechazar → estado cambia a Rechazado

#### Auditoría
- [ ] `/Admin/Auditoria` — log de todos los accesos
- [ ] Filtrar por fecha, resultado, email
- [ ] Stats: total accesos, fallidos, exitosos, bloqueados

---

### 3.2 Espectador

#### Login
1. Email: `espectador1@demo.com` / Password: `Usuario123!`
2. → Redirige a `/Espectador/Index`

#### Flujo de prueba
- [ ] Dashboard con lista de torneos activos
- [ ] Click en torneo → `/Espectador/TorneoDetalle?id=X`
  - [ ] Tab Clasificación — tabla de posiciones (si hay partidos finalizados)
  - [ ] Tab Grupos — si el torneo usa GruposYEliminacion
  - [ ] Tab Partidos — partidos por estado con badges correctos
- [ ] Calendario: `/Calendario/Index` — ver partidos de todos los equipos
- [ ] Preferencias: `/Espectador/Preferencias`
  - [ ] Seleccionar equipo favorito → guardar
  - [ ] Cambiar email de notificaciones → guardar → toast de éxito

---

### 3.3 Jugador

#### Login
1. Email: `jug_e1_1@demo.com` / Password: `Usuario123!`
2. → Redirige a `/Jugador/Dashboard`

#### Flujo de prueba
- [ ] Dashboard muestra banner del equipo actual (si está en un torneo activo)
- [ ] Widget "Próximos Partidos" muestra partidos de su equipo
- [ ] `/Jugador/Partidos/Index` — partidos del equipo (EnCurso, Próximos, Resultados)
- [ ] `/Jugador/BuscarEquipos` — lista equipos con logo + entrenador + abreviación
- [ ] Solicitar unirse a un equipo → aparece en Solicitudes
- [ ] `/Jugador/Solicitudes` — ver estado de solicitudes enviadas

#### Flujo sin equipo
- [ ] Login con jugador sin equipo (o crear uno desde /Account/RegisterJugador)
- [ ] Dashboard muestra banner "Sin equipo - Buscar equipos"
- [ ] BuscarEquipos disponible, Partidos muestra empty state

---

### 3.4 Entrenador

#### Login
1. Email: `entrenador1@demo.com` / Password: `Usuario123!`
2. → Redirige a `/Entrenador/Dashboard`

#### Flujo de prueba
- [ ] Dashboard con info del equipo (nombre, abreviación)
- [ ] Widget "Próximos Partidos" de su equipo
- [ ] `/Entrenador/MiEquipo` — datos del equipo
- [ ] `/Entrenador/Plantilla/Index` — jugadores del equipo (filtrar por torneo)
- [ ] Agregar jugador: buscar por nombre → invitar → solicitud creada
- [ ] `/Entrenador/Solicitudes/Index` — aceptar/rechazar solicitudes pendientes
  - [ ] Al aceptar → jugador aparece en plantilla
- [ ] `/Entrenador/Partidos/Index` — partidos del equipo
- [ ] Click "Ver Convocatoria" → convocatoria con plantillas de ambos equipos

---

### 3.5 Árbitro

#### Login
1. Email: `arbitro1@demo.com` / Password: `Usuario123!`
2. → Redirige a `/Arbitro/Dashboard`

> Los árbitros del seed_demo.sql ya tienen EstadoAprobacion = 'Aprobado' y están asignados a partidos.

#### Flujo de prueba
- [ ] Dashboard con card "Ver Calendario"
- [ ] `/Arbitro/Partidos/Index` — mis partidos asignados
- [ ] Click "Verificar Partido" → `/Arbitro/Partidos/Verificacion?id=X`
  - [ ] Panel de 5 checks: Estado ✓, Cuórum Local ✓/✗, Cuórum Visitante ✓/✗, Cuerpo Técnico ✓/✗
  - [ ] Si todos pasan → banner verde "Partido listo para comenzar"
  - [ ] Si alguno falla → banner rojo con el check fallido en rojo
- [ ] `/Calendario/Index` — ver todos los partidos como árbitro (los suyos marcados diferente)

#### Probar árbitro pendiente
1. Registrar nuevo árbitro en `/Account/RegisterArbitro`
2. Login → dashboard dice "cuenta pendiente de aprobación" (o redirige según impl)
3. Admin aprueba → árbitro puede recibir asignaciones

---

## 4. Pruebas de Seguridad

### Lockout
1. Ir a `/Account/Login`
2. Intentar login con email válido pero contraseña incorrecta
3. Repetir 5 veces → en el 5to intento: "Cuenta bloqueada por 30 minutos"
4. Verificar en `/Admin/Usuarios/Index` → muestra "Bloqueado" con fecha
5. Admin puede desbloquear manualmente

### Protección de rutas
- [ ] Sin login → acceder a `/Admin/Dashboard` → redirige a `/Account/Login`
- [ ] Login como Jugador → acceder a `/Admin/Dashboard` → redirige a `/Account/Login`
- [ ] Login como Espectador → acceder a `/Jugador/Dashboard` → redirige

### Back button protection
1. Login → navegar a dashboard
2. Logout
3. Presionar "Atrás" en el browser
4. → Debe redirigir a Login (no mostrar la página privada cached)

---

## 5. Datos de Prueba Específicos

### Partido con cuórum completo (para verificación árbitro)
```sql
-- Verificar que hay ≥11 jugadores en equipo 1 para torneo 1
SELECT COUNT(*) FROM equiposjugadorestorneos
WHERE equipoid = 1 AND torneoid = 1 AND activo = true;
```

### Verificar partidos del mes actual
```sql
SELECT p.id, el.nombre AS local, ev.nombre AS visitante, p.fecha, p.estado
FROM partidos p
JOIN equipos el ON el.id = p.equipolocalid
JOIN equipos ev ON ev.id = p.equipovisitanteid
WHERE EXTRACT(MONTH FROM p.fecha) = EXTRACT(MONTH FROM NOW())
  AND EXTRACT(YEAR FROM p.fecha) = EXTRACT(YEAR FROM NOW())
ORDER BY p.fecha;
```

### Crear solicitud de prueba
```sql
-- Jugador 1 solicita unirse al equipo 2 para el torneo 3
INSERT INTO solicitudesjugadorequipo (jugadorid, equipoid, torneoid, estado, esinvitacion)
VALUES (1, 2, 3, 'Pendiente', false);
```

---

## 6. Resetear Datos

### Resetear solo la BD (mantener estructura)
```bash
# En Supabase Studio → SQL Editor
-- Limpiar todo preservando estructura
TRUNCATE TABLE public.preferenciasespectador, public.solicitudesjugadorequipo,
    public.equiposjugadorestorneos, public.equiposentrenadores,
    public.equipostorneos, public.partidos, public.reglaspersonalizadastorneo,
    public.configuracionestorneo, public.grupos, public.torneos,
    public.equipos, public.arbitros, public.entrenadores,
    public.jugadores, public.personas, public.auditoriaaccesos,
    public.usuariosroles, public.usuarios
    RESTART IDENTITY CASCADE;

-- Los roles y datos base (liga, temporada, categorías) se vuelven a sembrar
-- al reiniciar la app (Program.cs seed solo si tablas vacías)
```

### Re-correr seed completo
```bash
# 1. TRUNCATE (incluido en seed_demo.sql Part 0)
# 2. Correr seed_demo.sql completo en SQL Editor
# 3. Re-insertar admin manualmente (el seed borra todo)
INSERT INTO public.usuarios (id, email, passwordhash, nombreusuario, activo, bloqueohabilitado)
VALUES (gen_random_uuid(), 'admin@ex.com',
    '$2a$11$6LJfBW9j3vRNQZVKHQjwzuVVMVG5h5Y6K5Oqh8Hd9bN3iMvK2wXKq',
    'admin', true, true);

-- Obtener el id recién insertado
WITH new_admin AS (SELECT id FROM usuarios WHERE email = 'admin@ex.com')
INSERT INTO personas (id, usuarioid, nombre, apellidopaterno, activo)
SELECT gen_random_uuid(), id, 'Admin', 'Sistema', true FROM new_admin;

WITH new_admin AS (SELECT id FROM usuarios WHERE email = 'admin@ex.com'),
     rol_admin AS (SELECT id FROM roles WHERE nombre = 'Administrador')
INSERT INTO usuariosroles (usuarioid, rolid)
SELECT u.id, r.id FROM new_admin u, rol_admin r;
```

---

## 7. Errores Comunes y Soluciones

| Error | Causa | Solución |
|-------|-------|---------|
| `NETE0001` / conexión rechazada | Supabase no está corriendo | `supabase start` |
| `127.0.0.1` vs `localhost` | Windows resuelve localhost → IPv6 | Usar siempre `127.0.0.1` |
| `MSB3027` / archivo bloqueado | App corriendo al hacer `dotnet build` | Detener la app primero |
| Login con credenciales correctas falla | Seed TRUNCATE eliminó admin | Re-insertar admin (ver arriba) |
| `column "esinvitacion" does not exist` | Migración Sprint3Fields no aplicada | `dotnet ef database update` |
| `column "abreviacion" does not exist` | Igual que arriba | `dotnet ef database update` |
| PDF vacío | QuestPDF no configurado | Verificar `QuestPDF.Settings.License = LicenseType.Community` en Program.cs |
| Logo de equipo no muestra | Ruta `/uploads/logos/xxx.png` | Verificar `wwwroot/uploads/logos/` existe y hay permisos |
| Toast no aparece | `alert-success` div con `d-none` | Verificar que site.js carga y DOMContentLoaded se ejecuta |
| Calendario vacío | Sin partidos en el mes | Correr seed_demo.sql para tener partidos en marzo 2026 |

---

## 8. Verificación de Build

```bash
# Verificar que compila sin errores CS/RZ
dotnet build ControlTorneosFutbol

# Output esperado:
# Build succeeded.
# 0 Warning(s)
# 0 Error(s)

# Nota: MSB3027 es "archivo locked" (app corriendo) — NO es un error de código
```

---

## 9. URLs Clave

| Ambiente | URL |
|---------|-----|
| Desarrollo | `http://localhost:5000` (o el puerto asignado por Kestrel) |
| Supabase Studio local | `http://127.0.0.1:54323` |
| Railway (producción) | `https://[nombre-app].railway.app` |

---

## 9b. Flujos de Prueba — Sprint 4

### Flujo Convocatoria Completo (US22 + US23)

**Rol: Entrenador**
1. Login como `entrenador1@demo.com`
2. Ir a `/Entrenador/Partidos/Index` → ver partidos del equipo
3. Click "Crear Convocatoria" en un partido próximo
4. En `/Entrenador/Partidos/Convocatoria/Crear?partidoId=X`:
   - [ ] Aparecen jugadores de la plantilla en tarjetas
   - [ ] Toggle Titular/Suplente/Reserva por jugador
   - [ ] Contador de titulares (mínimo 11)
   - [ ] Botón "Guardar Borrador" — estado Borrador
   - [ ] Botón "Publicar" → error si <11 titulares; éxito si ≥11
5. Al publicar → notificaciones enviadas a jugadores convocados

**Rol: Jugador**
1. Login como `jug_e1_1@demo.com` (jugador del equipo del entrenador1)
2. Ir a `/Jugador/Partidos/Index`
   - [ ] Badge "Convocado" visible en el partido
   - [ ] Botón "Confirmar Asistencia" disponible
3. Click "Confirmar Asistencia" → estado cambia a Confirmo
4. Click "No Disponible" → estado cambia a NoDisponible
   - [ ] Notificación aparece en sidebar del jugador

### Flujo Árbitro — Partido en Vivo (US12)

**Prerrequisito:** Partido con estado Programado asignado al árbitro

1. Login como `arbitro1@demo.com`
2. Ir a `/Arbitro/Partidos/Verificacion?id=X`
   - [ ] 5 checks automáticos verificados
   - [ ] Banner verde "Partido listo para comenzar"
   - [ ] Botón "Iniciar Partido" visible y activo
3. Click "Iniciar Partido" → redirige a `/Arbitro/Partidos/EnVivo?partidoId=X`
4. En panel EnVivo:
   - [ ] Marcador 0-0 inicial
   - [ ] MinutoActual visible (o cronómetro)
   - [ ] Botón GOL → modal: seleccionar equipo → seleccionar jugador → confirmar
   - [ ] Gol registrado → marcador actualiza automáticamente
   - [ ] Botón TARJETA AMARILLA → mismo flujo
   - [ ] Botón TARJETA ROJA → mismo flujo
   - [ ] Botón CAMBIO → seleccionar jugador que sale + que entra
   - [ ] Lista de incidencias en orden cronológico
   - [ ] Polling cada 10s actualiza estado sin recargar página
5. Click "Finalizar Partido" → estado cambia a Finalizado
6. Redirige a `/Arbitro/Partidos/Reporte?partidoId=X`
   - [ ] Resumen de incidencias + marcador final
   - [ ] Campo observaciones
   - [ ] Botón "Exportar PDF" → descarga reporte en PDF

### Flujo Reprogramación (US7)

1. Login como `admin@ex.com`
2. Ir a `/Admin/Partidos/Editar?id=X`
3. Cambiar fecha a una donde el árbitro ya tiene otro partido:
   - [ ] Error: "El árbitro ya tiene un partido el [fecha] en [torneo]"
4. Cambiar fecha a una sin conflicto:
   - [ ] Estado cambia a Reprogramado
   - [ ] Campo RazonReprogramacion requerido
   - [ ] `HistorialPartido` creado con fecha anterior/nueva
   - [ ] Notificaciones enviadas al árbitro y a entrenadores de ambos equipos

### Flujo Notificaciones

1. Login con cualquier usuario que haya recibido una notificación
2. Ver campanita en sidebar:
   - [ ] Badge rojo con número de no leídas
   - [ ] Click → dropdown con últimas 10 notificaciones
   - [ ] Cada notificación muestra tipo (badge) + título + fecha
   - [ ] Click en notificación → navega a urlAccion
   - [ ] Botón "Marcar todas como leídas" → badge desaparece

---

## 10. Casos de Prueba Críticos

| # | Caso | Resultado esperado |
|---|------|-------------------|
| 1 | Login con credenciales incorrectas 5 veces | Cuenta bloqueada 30 min, log en auditoría |
| 2 | Admin desbloquea cuenta | ContadorAccesosFallidos=0, FechaBloqueoFin=null |
| 3 | Registrar jugador con email ya existente | Error: "El correo ya está registrado" |
| 4 | Crear partido con mismo equipo local y visitante | Error de validación |
| 5 | Árbitro pendiente intenta acceder a /Arbitro/* | Acceso denegado o mensaje de pendiente |
| 6 | Admin aprueba árbitro | EstadoAprobacion = "Aprobado" |
| 7 | Jugador solicita unirse a equipo ya completo | Toast de advertencia (si implementado) |
| 8 | Entrenador acepta solicitud | EquipoJugadorTorneo creado, SolicitudEstado = Aceptada |
| 9 | Exportar PDF de torneos | Descarga `torneos.pdf` con datos correctos |
| 10 | Espectador accede a /Admin/* | Redirige a Login |
| 11 | Verificación partido con <11 jugadores | Check falla, banner rojo |
| 12 | Verificación partido con ≥11 jugadores y entrenadores | Todos checks pasan, banner verde |
