# Fixture scheduling redesign — ODD Task Plan

**Goal:** Turn admin fixture scheduling into a select → preview → validate → commit workflow with a contextual inspector, and fix the scheduling defects found while grounding the Midnight Design Director studies in this codebase.

**Status (2026-10-16):** Planning only. No source file changed. Three rendered direction studies (A Planilla, B Descanso, C Bandeja) are awaiting the user's selection. The build/test toolchain was unavailable in the planning session (no .NET SDK; NuGet blocked), so no RED test has run yet.

## Findings from code inspection (baseline `8dc2984`)

| # | Where | Finding | Severity |
|---|---|---|---|
| F1 | `Pages/Admin/Partidos/Editar.cshtml.cs` | Conflict check runs only when `Fecha` changes. Changing `Sede` or `ArbitroId` alone bypasses it. | Defect |
| F2 | same | Referee double-booking is never checked. | Defect |
| F3 | same | `HistorialPartido.FechaAnterior` is computed after `partido.Fecha` is overwritten (`partido.Fecha == Input.Fecha ? null : …`), so it is always `null`. | Defect |
| F4 | same | `SedeAnterior = partido.Sede` is read after `partido.Sede` is overwritten, so it records the new sede. | Defect |
| F5 | same | `CambioArbitro` history stores neither the previous nor the new referee. Schema has no fields for them (no migrations allowed): use `Razon` text unless a migration is approved. | Gap |
| F6 | `Pages/Admin/Torneos/AsignarDetallesFixture.cshtml.cs` | Batch save applies date/sede/referee to every match with **no conflict check, no `HistorialPartido`, no notifications**. | Defect |
| F7 | Conflict rule | `±2 h` window is inclusive (`>=`/`<=`). With 90-min matches, a same-sede or same-team match exactly 2 h later is rejected, so the common 2-hour cadence is impossible. | Product decision |
| F8 | Conflict feedback | One generic error after submit; no list of what clashes, no preview, no suggestions. | UX gap (the redesign) |
| F9 | `Partido.Sede` | Free text. Venue conflicts depend on exact string equality (`"Cancha Norte"` ≠ `"cancha norte "`). | Gap |

Notifications (`NotificacionService.NotificarPartidoAsync`) are in-app to referee, active coaches and registered players, and fire only on reprogramación. Not on sede or referee change, and the replaced referee is never told.

## Decisions needed from the user

1. Direction A, B or C (or a reasoned hybrid) for the scheduling surface.
2. F7 rule: keep inclusive ±2 h, or overlap = match length + buffer (proposed 90 + 15 min)?
3. Rest-between-matches warning (proposed 18 h): adopt, change, or drop? It does not exist today.
4. F5: accept referee names in `Razon`, or approve a migration adding `ArbitroAnteriorId`/`ArbitroNuevoId`?
5. Should sede and referee changes also notify (today only reprogramación does)?

## Ordered work units (after decisions)

1. **Conflict service + RED tests.** `Services/IConflictosPartidoService` + implementation: evaluate a proposed (fecha, sede, árbitro) for a match against the tournament schedule; return typed issues (equipo, sede, árbitro, descanso) with severity and the clashing match. Normalize sede (trim, case-insensitive). xUnit + EF InMemory tests first, covering F1, F2, F7 (per decision), F9.
2. **History fixes (F3–F5) with RED tests.** Capture previous values before mutation.
3. **Editar uses the service** for any change to fecha/sede/árbitro; show all issues, not one generic line.
4. **Batch page (F6)** validates through the service, writes history, notifies.
5. **Preview endpoint**: `OnGetPrevisualizarAsync` JSON handler returning issues + ranked free slots + referee availability for the inspector.
6. **UI slice** for the chosen direction on `AsignarDetallesFixture` (desktop + mobile, keyboard, reduced motion, Bootstrap Icons already in use).
7. Verification: `dotnet test`, forced solution build, browser check of the new page.

## Boundaries (inherited from canonical plan)

No changes to `Program.cs`, `Migrations/`, appsettings/secrets. Feature branch only; never push to `master`. Tests before fixes.
