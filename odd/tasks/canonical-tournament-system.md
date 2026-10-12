# Canonical Tournament System — ODD Task Plan

**Goal:** Audit parity against upstream master, preserve verified local/RAR/archive improvements, and fix only confirmed defects. See [Canonical Tournament System Design](../../docs/superpowers/specs/2026-10-12-canonical-tournament-system-design.md).

**Current status:** Planning/spec/task files were committed as `0d7ef11` (`docs: define canonical tournament consolidation plan`). No production-source changes, tests, pushes, or deletions have occurred. The application is a root-level ASP.NET Core Razor Pages app. Do not change appsettings secrets or include them in Git.

## Ordered work units

1. **Audit baseline and deltas.** Compare the root-level application with upstream master and inspect all meaningful local/RAR/archive deltas. The local folder matches `ExtremelyAdvanced.zip` across 160 selected source files; ten ZIPs were fingerprinted, older snapshots add no selected source paths missing from local, and `BackUp2`/`BackUpLogOut` fingerprints match. Review all meaningful differences in the March 1 RAR's 15 differing page files. Create `docs/canonical-parity-audit.md` with a reasoned disposition for every meaningful delta, recording provenance, parity, and why each change is retained or rejected. Verify parity for existing fixture/calendar, convocation, report/PDF, notification, and other upstream functionality without planning to rebuild it. Verify this audit slice, then make one focused work-unit commit.
2. **Create the focused test project.** Create `tests/Blazebol.Tests/` using existing .NET conventions and add the project to `ControlTorneosFutbol.sln`. Keep dependencies limited to the behaviors under test. Build/verify the project and solution integration, then commit this slice.
3. **Write focused behavior tests first.** Cover: (a) an unauthorized or non-assigned referee cannot read or write live-match incidents; (b) submitted team/player IDs must belong to the match and invalid submissions return accurate failure responses; and (c) a valid assigned referee's incident operation succeeds. Add regression assertions for existing spectator draft filtering and upcoming-match count only if straightforward in the selected test setup. Test other behavior only when the parity audit or code inspection establishes a concrete change. Capture the failing behavior for the confirmed incident defect before implementation. Verify these tests and commit this test slice.
4. **Fix the confirmed live incident defect only.** Make the smallest change in the live-match service/page path required by failing tests to enforce referee assignment and submitted team/player membership, including accurate failure responses. Do not rebuild fixture/calendar, convocations, report/PDF, notifications, or other existing upstream behavior. Do not claim spectator implementation changes unless tests reveal a real regression. Do not modify `Migrations/` or migration-related `Program.cs`, and do not port the local broken startup `ExecuteSqlRawAsync` guard for `SELECT`. Run focused tests and commit the verified fix.
5. **Integrated verification.** Run focused tests first, then all applicable repository validation. Record exact commands and observed results; inspect changed paths and confirm appsettings secrets are excluded. Preserve upstream's manual migration behavior. Commit any necessary, separately verified in-scope integration correction as a focused slice.
6. **Feature-branch delivery.** Push the feature branch only after every required check passes. Never push to `master` or rewrite history. If checks fail, do not push; report the observed failure.
7. **Separate cleanup gate.** After successful verification and feature-branch push, inventory the exact project-folder/archive paths proposed for removal and present the concrete list for review and approval before any deletion. Delete only approved paths on that list. Preserve appsettings/secrets, the canonical clone, every unlisted path, and `C:/Users/frenzied` (the misplaced ambient Git root). If a target is not an exact inventoried and approved path, preserve it.

## Allowed edit surfaces

`docs/canonical-parity-audit.md`
`Services/PartidoEnVivoService.cs`
`Services/IPartidoEnVivoService.cs`
`Pages/Arbitro/Partidos/EnVivo.cshtml.cs`
`Pages/Espectador/Index.cshtml.cs`
`Pages/Espectador/Torneos.cshtml.cs`
`Pages/Espectador/TorneoDetalle.cshtml.cs`
`tests/Blazebol.Tests/**`
`ControlTorneosFutbol.sln`
