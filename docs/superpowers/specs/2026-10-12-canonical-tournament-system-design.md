# Canonical Tournament System Design

Establish a canonical version of the root-level ASP.NET Core Razor Pages application using upstream `jojosenthusiast/Blazebol` master at `f39507b` as the baseline. The user approved this design on 2026-10-12. Preserve upstream as the baseline, retain verified improvements, and fix only confirmed defects. The parity audit is recorded in [`docs/canonical-parity-audit.md`](../../canonical-parity-audit.md).

## Repository shape and approach

The application is at repository root, with `Pages/`, `Data/`, `Models/`, `Services/`, `Migrations/`, root `Program.cs`, and `ControlTorneosFutbol.sln`. The application is Razor Pages. Add focused xUnit tests in `tests/ControlTorneosFutbol.Tests/`; use EF InMemory 8.0.x for scoped service tests where suitable. Because tests are nested under the web project root, add `<Compile Remove="tests/**/*.cs" />` in `ControlTorneosFutbol.csproj` so the app does not compile test sources. Add the test project to the solution.

The audit confirmed upstream already implements fixture/calendar, convocations, report/PDF, notifications, spectator upcoming-match queries, draft filtering, favorites, finished-tournament summaries, and player/coach details. Retain upstream behavior; do not rebuild these capabilities. No wholesale local port is justified. The audit was targeted, not an exhaustive semantic inspection of every shared file or behavior.

## Confirmed live-match security defects and plan

- `Pages/Arbitro/Partidos/EnVivo.cshtml.cs` has `[IgnoreAntiforgeryToken]`. `OnGetAsync` checks assignment, but `OnGetEstadoAsync` checks authentication only. Incident POST handlers do not check referee assignment; the `/arbitro` role gate is not match-specific authorization.
- `Services/PartidoEnVivoService.cs` verifies only that the match exists and is in progress. Supplied team/player IDs are saved without confirming they belong to the match. EF relations validate existence, not match membership. Match convocations are scoped by match/team, but arbitrary team incidents can be treated as away-team goals.
- Page handlers ignore false service results and return `ok=true`.
- `Pages/Arbitro/Partidos/EnVivo.cshtml` start/shared POST AJAX paths do not send an antiforgery token, and no token is rendered. `Program.cs` configures the `X-CSRF-TOKEN` header.

Write focused tests first for state-read and incident-write authorization; team and match-convocation player membership; failure-result propagation; valid assigned-referee success; and antiforgery token/header behavior for both AJAX paths. A minimal isolated test host/token/filter test is acceptable for page antiforgery if practical. Make only the smallest demonstrated fix in the service/page/view. Do not change `Program.cs` role or migration configuration.

Independent review of the live-match change found three additional same-feature defects to test and fix before delivery: refreshed incident-feed values are interpolated into `innerHTML` without encoding (stored-XSS risk); the generic incident handler accepts arbitrary types including `Gol`, which leaves the score inconsistent; and the page offers match start for `Reprogramado` while the service rejects it. RED tests were added before these fixes.

Own-goal service semantics were independently checked: `EquipoId` denotes the team whose player committed the own goal, and score recalculation credits the opponent. The service-level regression `Autogol_valida_jugador_del_equipo_que_lo_comete_y_suma_al_oponente` confirms the membership check and score calculation. A final review then found a separate UI mapping bug: the toolbar `Gol Local/Visitante` identifies the credited side, but `toggleAutogol()` computes `equipoContrario` and mistakenly uses `equipoActual`, so it selects the credited side's roster and leaves the hidden incident team ID unchanged. Page-level test `Autogol_actualiza_equipo_incidente_y_carga_convocados_del_oponente` observed RED (16 total, 1 failure/15 passes); fix the UI to use the opposing team's roster/ID when autogol is checked, restoring the credited team when unchecked.

A final review also found team names interpolated into inline `onclick` JavaScript arguments (`EnVivo.cshtml:179,182`), a stored-XSS risk in this same live-match page. Although those lines were unchanged in the earlier diff, remove the unsafe handler as part of this focused page fix, using encoded data attributes/event listeners and a RED regression test. Remaining separate follow-ups: a concurrency race between incident registration and finalization (state-check/persistence sites unchanged; needs PostgreSQL concurrency strategy/test) and player match-convocation POST antiforgery.

## Audit findings and dispositions

| Area | Disposition |
|---|---|
| Local/RAR/archive differences | Findings and reasoned dispositions are in the parity audit. Retain verified upstream capabilities; do not wholesale copy local/archive snapshots. |
| Existing upstream capabilities | Retain tournament/fixture, player eligibility, dashboards, convocations, rescheduling, match readiness, spectator, report, and notification behavior confirmed by audit. Test only the live-match defects and other concrete regressions established by inspection. |
| Npgsql retries and HTTPS redirect | Local-only candidates are deployment-dependent. Require evidence from deployment/pooler before considering; do not port speculatively. |
| Migration behavior | Upstream's manual SQL migration process is canonical. Do not port the local broken startup `ExecuteSqlRawAsync("SELECT COUNT(*)")` guard. Do not modify `Migrations/` or migration-related `Program.cs`. Migration default/rollback concerns are documented in the audit; actual DB/deployment state is unknown. |
| Player match-convocation antiforgery | Raw-string forms may omit antiforgery tokens while POST handlers validate them. This is a concrete, separate follow-up, gated for later user review; do not include in the live-match unit. |

## Non-goals and boundaries

- No feature duplication where parity with upstream is established.
- No changes to appsettings secrets or other credentials; secrets remain excluded from Git and are not inspected.
- No automatic startup migrations, deployment, release, push to `master`, or history rewrite.
- No changes to migrations or migration behavior.
- Do not include the separately gated player-convocation antiforgery follow-up without later approval.
- Never delete or otherwise alter the misplaced ambient Git root `C:/Users/frenzied`.

## Acceptance and validation

- The targeted audit records the source scan scope, snapshot counts, exact 15 RAR differing paths, dispositions, live-match defects, migration/deployment risks, audit limitations, and separately gated follow-up.
- Add `tests/ControlTorneosFutbol.Tests/` and integrate it with `ControlTorneosFutbol.sln`; exclude `tests/**/*.cs` from the web project's compile inputs.
- Live-match tests cover denied state reads and incident writes for unauthorized/non-assigned referees; rejection of unrelated team/player IDs with accurate failure responses; successful assigned-referee handling; antiforgery behavior for both AJAX paths; safe incident-feed rendering; rejection of invalid generic incident types; and consistent start behavior for `Reprogramado` matches.
- Write behavior tests before the implementation fix. Make only the smallest implementation change needed to pass. No spectator implementation changes unless tests prove a real regression.
- Preserve upstream's manual migration behavior; do not modify `Migrations/` or migration-related `Program.cs`, and never port the broken local guard.
- Run focused tests first, then applicable validation. Record exact commands and observed results. Push the feature branch only after successful required checks; never push to `master` or rewrite history.
- Keep appsettings secrets out of Git. Cleanup is separate from implementation and requires review/approval of the concrete exact path list after successful verification and push.

## Delivery boundaries

Work in small ordered slices. Verify each slice and make a focused work-unit commit only after its checks pass. Push the feature branch only after all required checks succeed. Cleanup is separate, explicitly approved action: inventory exact project-folder/archive paths and present the concrete list for review before any deletion. Preserve appsettings/secrets, all unlisted paths, the canonical clone, and `C:/Users/frenzied` (the misplaced ambient Git root).

**Current status:** The user-approved design/audit is `70db5ed`; test-project scaffold is `da7978c`; RED test commits are `2873186`, `f633948`, `3d58e2d`, and `4885953`. The own-goal UI mapping test is RED: 16 tests total, 1 failure/15 passes. Forced full solution build previously succeeded with 0 errors and 17 pre-existing warnings. Team-name handlers use safe data attributes/listeners. Commit the RED test and fix own-goal team/roster mapping before final review and implementation commit; no push or deletion has occurred.
