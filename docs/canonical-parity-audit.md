# Canonical parity audit

**Baseline:** `jojosenthusiast/Blazebol` master at `f39507b`, in canonical clone branch `feat/canonical-consolidation`. The user approved this design on 2026-10-12. Audit scope was selected source files (`.cs`, `.cshtml`, `.css`, `.js`, `.sql`, `.csproj`, `.sln`) from ten ZIPs and one March 1 RAR; appsettings, binaries, and vendor assets were excluded. This is a targeted parity audit, not an exhaustive semantic review of every shared file. No appsettings or secrets were inspected.

## Snapshot comparison

- The local snapshot matches `ExtremelyAdvanced.zip` across 160 selected source files.
- Older ZIPs have no selected source paths absent from the local snapshot.
- Across archive snapshots, 45 unique selected source paths differ. The staged text sources are grouped under `C:/tmp/Blazebol-version-audit/`; older archive differences were reviewed by category and disposition below, not treated as grounds for wholesale porting.
- The March 1 RAR has the same selected source paths as local and 15 changed page files. Exact staged paths:
  - `Backup1_RAR/pages/admin/torneos/details.cshtml`
  - `Backup1_RAR/pages/admin/torneos/details.cshtml.cs`
  - `Backup1_RAR/pages/admin/torneos/edit.cshtml`
  - `Backup1_RAR/pages/admin/torneos/edit.cshtml.cs`
  - `Backup1_RAR/pages/admin/torneos/index.cshtml`
  - `Backup1_RAR/pages/arbitro/partidos/index.cshtml`
  - `Backup1_RAR/pages/entrenador/plantilla/editar.cshtml`
  - `Backup1_RAR/pages/entrenador/plantilla/editar.cshtml.cs`
  - `Backup1_RAR/pages/espectador/index.cshtml`
  - `Backup1_RAR/pages/espectador/index.cshtml.cs`
  - `Backup1_RAR/pages/espectador/torneodetalle.cshtml`
  - `Backup1_RAR/pages/espectador/torneodetalle.cshtml.cs`
  - `Backup1_RAR/pages/espectador/torneos.cshtml.cs`
  - `Backup1_RAR/pages/jugador/dashboard.cshtml`
  - `Backup1_RAR/pages/jugador/dashboard.cshtml.cs`

## Dispositions

### Retain upstream canonical behavior

- **Spectator pages:** RAR `Index`, `Torneos`, and `TorneoDetalle` differ from local. Upstream retains active/scheduled tournament filtering, favorites and upcoming/result data, draft exclusion, standings, and final champion/table. Retain upstream rather than replacing it with the RAR/local variants.
- **Player dashboard:** RAR leaves upcoming matches empty; upstream queries scheduled games and retains pending-request indicators. Retain upstream.
- **Coach roster editing:** RAR contains a shirt-number uniqueness check in player editing, and upstream also has that check. No port is needed.
- **Older schema, models, and registration snapshots:** these predate `TipoEquipo`, player compatibility, logo upload, and the fixture/convocation/report/notification schema now present upstream. Retain upstream schema and capabilities; do not back-port outdated snapshots.
- **Authentication transaction pattern:** the older `AuthService` transaction pattern matches upstream; no meaningful improvement requiring a port was identified.
- **Tournament and player features present in upstream:** upstream has broader tournament landing/fixture field persistence; player eligibility filtering by gender/age and server-side submission validation; active tournament editing for already-assigned deactivated league/season; end-date validation; player Profile JSON endpoints; player/coach dashboards and upcoming matches; player convocations and confirmations with `Reprogramado` handling; admin rescheduling history, reasons, notifications, and ±2-hour conflict prevention; and match readiness checks plus a start handler. Retain these upstream features; no wholesale local port is justified.

### Local-only candidates and migration risks

- **Npgsql retries:** local configures five retries not present upstream. Whether this helps depends on the deployed pooler and observed transient failures; require deployment evidence before considering it. Do not port based only on snapshot comparison.
- **HTTPS redirect:** local unconditionally redirects to HTTPS, while upstream uses a Railway proxy-aware condition. This is deployment-dependent; retain upstream until deployment requirements are established.
- **Automatic startup migration guard:** do not port. The local guard runs `ExecuteSqlRawAsync("SELECT COUNT(*)")` and migrates only if the `usuarios` table is absent. Upstream's manual SQL migration process remains canonical. Do not alter migrations or migration behavior in this work.
- **Migration concerns to verify against deployment/DB before any migration operation:** `20260310230931_AddFixtureColumns.cs:42–48` sets a default of `false` while the entity default is `true`. `20260310214627_FixtureAutomatico.cs:11–28` uses `IF NOT EXISTS`, but its `Down` at `:31–90` drops fields, creating rollback/data-loss risk. The actual database and deployment state are unknown; no migration change is authorized here.

## Confirmed live-match security defects

These are confirmed source-level defects in the upstream baseline and define the planned narrow implementation/test scope; no source changes are made by this documentation unit.

- `Pages/Arbitro/Partidos/EnVivo.cshtml.cs` has `[IgnoreAntiforgeryToken]` (line 12). `OnGetAsync` checks referee assignment (around line 59), but `OnGetEstadoAsync` checks authentication only (around lines 105–111).
- Incident POST handlers (around lines 115–173) do not check referee assignment. `Program.cs` role-gates `/arbitro` only (lines 144–148), which does not establish assignment to a particular match.
- `Services/PartidoEnVivoService.cs` checks only that a match exists and is in progress (lines 55–58); supplied team/player IDs are persisted directly (around lines 65–67). EF relations ensure referenced records exist, not that they belong to that match (`Data/ApplicationDbContext.cs:340–353`).
- Match convocations are scoped to match/team (`PartidoEnVivoService.cs:236–238`), but arbitrary team incidents are interpreted as away-team goals (`:106–118`).
- Incident handlers ignore a `false` service result and return `ok=true` (page model lines 120–127, 140–145, 154–159, 168–173), so failures are misreported.
- `EnVivo.cshtml`'s start fetch and shared `fetchPost` (around lines 585–594) send no antiforgery token, and no token is rendered. `Program.cs` configures the `X-CSRF-TOKEN` header.

Planned tests/fix should cover assignment enforcement for state reads and incident writes; team IDs and match-convocation player IDs belonging to the match; propagation of failure results; and antiforgery token/header handling for both AJAX paths. Keep the change scoped to the live-match service/page and relevant view, with an isolated test host/token/filter approach for page antiforgery if practical. Do not change `Program.cs` role or migration configuration.

## Separately gated follow-up

Player match-convocation forms in upstream `Pages/Jugador/Partidos/Index.cshtml:35–68` are constructed as raw strings without an antiforgery token, while POST handlers in `.cs:160–169` normally validate tokens. This is a concrete potentially broken feature/security concern, but it is separate from the live-match scope and must be reviewed/approved as a later unit; do not silently include it.

## Test/implementation plan updates

- No tests currently exist; the app targets .NET 8. Create the xUnit project at `tests/ControlTorneosFutbol.Tests/`, with EF InMemory 8.0.x for scoped service tests.
- Because tests are nested under the web project root, add `<Compile Remove="tests/**/*.cs" />` to `ControlTorneosFutbol.csproj` to prevent test sources being compiled into the app.
- Add `Pages/Arbitro/Partidos/EnVivo.cshtml` to the live-match edit surface for rendering/submitting antiforgery tokens.
- Do not modify `Program.cs` role/migration configuration, migrations, appsettings, or the separately gated player-convocation path.

## Audit limits and state

The audit was targeted to the selected source extensions and identified/archive-staged differences; it does not claim semantic inspection of every shared page, JavaScript interaction, styling detail, SQL path, or every behavior in the application. Appsettings/secrets and vendor/binary assets were excluded. Deployment and database state are unknown, so the retry, HTTPS, and migration observations are risks, not operational conclusions.

Follow-up delivery status (2026-10-12): the canonical implementation and focused tests are committed on `feat/canonical-consolidation` and pushed to `origin` at `64b7f02`. The focused suite passed 16/16; the forced full solution build succeeded with 0 errors and 17 pre-existing warnings. Native review was approved and acknowledged. No cleanup deletion has occurred; exact staging cleanup remains subject to user approval. No appsettings or secrets were inspected.
