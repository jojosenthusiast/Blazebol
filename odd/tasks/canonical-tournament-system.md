# Canonical Tournament System — ODD Task Plan

**Goal:** Preserve upstream `jojosenthusiast/Blazebol` master (`f39507b`) as canonical baseline, retain verified improvements, and fix only confirmed defects. See [Canonical Tournament System Design](../../docs/superpowers/specs/2026-10-12-canonical-tournament-system-design.md) and [Canonical parity audit](../../docs/canonical-parity-audit.md).

**Current status:** User approved the design on 2026-10-12. Planning/spec/task documents were previously committed as `0d7ef11` and `4ee4e75`. Task 1's targeted archive/local parity audit is documented and passed parent `git diff --check` (LF-to-CRLF warnings only); its focused documentation commit is next. No production-source changes, tests, push, or deletions have occurred. The application is a root-level ASP.NET Core Razor Pages app. Do not inspect or change appsettings secrets or include them in Git.

## Ordered work units

1. **Parity audit — documented.** Compare against upstream master using the selected source scan described in the audit. The local snapshot matches `ExtremelyAdvanced.zip` across 160 selected source files; older ZIPs add no selected source paths missing from local; archive snapshots have 45 unique differing selected source paths; and the March 1 RAR has the same selected paths with 15 changed page files, listed exactly in the audit. The audit records dispositions, upstream capabilities, local/deployment risks, migration concerns, and scope limitations. This is targeted, not an exhaustive semantic audit of every shared file.
2. **Create focused test project.** Create `tests/ControlTorneosFutbol.Tests/` using xUnit and EF InMemory 8.0.x for scoped service tests. Add the project to `ControlTorneosFutbol.sln` and add `<Compile Remove="tests/**/*.cs" />` to `ControlTorneosFutbol.csproj` because tests are nested under the web root. Verify project and solution integration.
3. **Write focused behavior tests first.** Cover (a) unauthorized/non-assigned referee denial for live state reads and incident writes; (b) rejection of unrelated team IDs and player IDs not in the match's convocations, with accurate failure results; (c) successful incident operation by an assigned referee; and (d) antiforgery token/header handling for both AJAX paths in `Pages/Arbitro/Partidos/EnVivo.cshtml`. Use a minimal isolated test host/token/filter approach for page antiforgery if practical. Add spectator draft/upcoming assertions only if straightforward and justified; test other behavior only for concrete changes established by audit/code inspection.
4. **Fix the confirmed live-match defects only.** Make the smallest service/page/view change needed to pass the focused tests, including assignment enforcement, ID membership checks, accurate result propagation, and antiforgery handling. Do not change `Program.cs` role/migration configuration. Do not claim spectator implementation changes unless tests prove a real regression.
5. **Integrated verification.** Run focused tests first, then applicable validation. Record exact commands and observed results; inspect changed paths and confirm appsettings secrets are excluded. Preserve upstream's manual migration behavior. Commit any necessary, separately verified in-scope integration correction as a focused slice.
6. **Feature-branch delivery.** Push the feature branch only after every required check passes. Never push to `master` or rewrite history. If checks fail, do not push; report the observed failure.
7. **Separate cleanup gate.** After successful verification and feature-branch push, inventory the exact project-folder/archive paths proposed for removal and present the concrete list for review and approval before any deletion. Delete only approved paths on that list. Preserve appsettings/secrets, the canonical clone, every unlisted path, and `C:/Users/frenzied` (the misplaced ambient Git root). If a target is not an exact inventoried and approved path, preserve it.

## Allowed edit surfaces

- `docs/canonical-parity-audit.md`
- `docs/superpowers/specs/2026-10-12-canonical-tournament-system-design.md`
- `odd/tasks/canonical-tournament-system.md`
- `Services/PartidoEnVivoService.cs`
- `Services/IPartidoEnVivoService.cs`
- `Pages/Arbitro/Partidos/EnVivo.cshtml.cs`
- `Pages/Arbitro/Partidos/EnVivo.cshtml`
- `Pages/Espectador/Index.cshtml.cs`
- `Pages/Espectador/Torneos.cshtml.cs`
- `Pages/Espectador/TorneoDetalle.cshtml.cs`
- `ControlTorneosFutbol.csproj`
- `tests/ControlTorneosFutbol.Tests/**`
- `ControlTorneosFutbol.sln`

The player match-convocation antiforgery issue in `Pages/Jugador/Partidos/Index.cshtml` and its page model is separately gated for later review and is not an allowed surface for this live-match work. Do not modify `Program.cs`, migrations, appsettings, or other paths.
