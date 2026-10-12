# Canonical Tournament System Design

Establish a canonical version of the root-level ASP.NET Core Razor Pages application using upstream `jojosenthusiast/Blazebol` master as the baseline. Audit parity and all meaningful local/RAR/archive deltas, preserve verified improvements, and fix only confirmed defects. The sole confirmed implementation defect so far is live-match incident authorization and submitted team/player ID validation.

## Repository shape and approach

The application is at repository root, with `Pages/`, `Data/`, `Models/`, `Services/`, `Migrations/`, root `Program.cs`, and `ControlTorneosFutbol.sln`. The application is Razor Pages. Create focused tests in `tests/Blazebol.Tests/` and add that project to the solution.

Upstream already implements fixture/calendar, convocations, live match, report/PDF, notifications, spectator upcoming-match queries, draft filtering, favorites, finished-tournament summaries, and player/coach details. Verify these capabilities for parity; do not rebuild them. Add narrowly scoped regression assertions for existing spectator draft filtering and upcoming-match count only if straightforward in the selected test setup. Test other behavior only when the parity audit or code inspection establishes a concrete change. For live-match incidents, cover authorization to read and write, submitted-ID membership and accurate failure responses, and successful handling by an assigned referee. Write tests first for the confirmed defect, then make the smallest demonstrated fix.

The local folder matches `ExtremelyAdvanced.zip` across 160 selected source files. Ten ZIPs were fingerprinted; older snapshots contain no selected source paths missing from local. `BackUp2` and `BackUpLogOut` have identical selected-source fingerprints. The March 1 RAR has the same selected source paths but differs in 15 page files. Review all meaningful local/RAR/archive deltas and record a reasoned disposition for each before choosing the canonical behavior.

## Findings and required disposition

| Area | Disposition |
|---|---|
| Local/RAR/archive differences | Create `docs/canonical-parity-audit.md` with a reasoned disposition for all meaningful local, RAR, and archive deltas. Preserve only verified improvements; do not wholesale copy archives. |
| Existing upstream capabilities | Verify fixture/calendar, convocations, report/PDF, notifications, and other listed existing features for parity; do not rebuild them or set a blanket requirement to test every existing path. Add regression tests only for the narrow spectator assertions when straightforward, or for concrete changes established by audit/code inspection. |
| Live incident security | Tests must establish that an unauthorized/non-assigned referee cannot read or write incidents; submitted team/player IDs must belong to the match and invalid submissions must return accurate failure responses; a valid assigned referee's incident operation must succeed. Fix only failures confirmed by these tests. |
| Migration behavior | Upstream's manual migration policy is canonical. Do not port the local broken startup `ExecuteSqlRawAsync` migration guard for a `SELECT`; do not modify `Migrations/` or `Program.cs` for migration behavior. |

## Non-goals

- No feature duplication where parity with upstream is established.
- No changes to appsettings secrets or other credentials; secrets must remain excluded from Git.
- No automatic startup migrations, deployment, release, push to `master`, or history rewrite.
- This planning-only change makes no production-source changes, tests, pushes, or deletions. The only planned commit is the design/task documentation.
- Never delete or otherwise alter the misplaced ambient Git root `C:/Users/frenzied`.

## Acceptance and validation

- `docs/canonical-parity-audit.md` records a reasoned disposition for all meaningful local/RAR/archive deltas, including the 15 differing RAR page files.
- Add `tests/Blazebol.Tests/` to `ControlTorneosFutbol.sln`.
- Focused incident tests cover read/write denial for an unauthorized or non-assigned referee; rejection of unrelated team/player IDs with accurate failure responses; and success for a valid assigned referee. Add spectator draft-filter and upcoming-match-count regression assertions only if straightforward in the chosen test setup. Test any other behavior only when audit or code inspection establishes a concrete change.
- Write tests first for the confirmed incident defect. Make only the smallest implementation change needed to pass those tests. Do not claim spectator implementation changes unless tests prove a real regression.
- Preserve upstream's manual migration behavior; do not modify `Migrations/` or migration-related `Program.cs`, and never port the broken local guard.
- Run focused tests first, then all applicable validation. Record exact commands and observed results. Push the feature branch only after successful required checks; never push to `master` or rewrite history.
- Keep appsettings secrets out of Git. Cleanup is separate from implementation and requires review/approval of the concrete exact path list after successful verification and push.

## Delivery boundaries

Work in small ordered slices. Verify each slice and make a focused work-unit commit only after its checks pass. Push the feature branch only after all required checks succeed. Cleanup is a separate, explicitly approved action: inventory exact project-folder/archive paths, present the concrete list for review before deletion, and delete only approved listed targets. Preserve appsettings/secrets, all unlisted paths, the canonical clone, and `C:/Users/frenzied` (the misplaced ambient Git root).

**Current status:** Planning/spec/task files have been created; no production-source changes, tests, pushes, or deletions have occurred. The only planned commit is the design/task documentation.
