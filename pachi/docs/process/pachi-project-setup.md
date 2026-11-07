# Pachi Project Setup (Brownfield)

> For: Axel, bringing the existing Pachi repo into line with the
> [playbook](playbook.md). Lets you: go from today's red, messy state to the
> [fresh-project target](fresh-project-setup.md) one ticket-sized step at a time.

This is the [fresh-project checklist](fresh-project-setup.md) (E0–E11) adapted
to the repo as it is on 2026-11-03. It replaces Part A's step list as the thing
you actually work through. Part A keeps the reasoning; this file holds the
concrete steps.

**How to read it**

- Steps are in dependency order. Each one ends with an **Exit** check.
- **⚖** marks a judgement call, with a recommendation.
- `Fresh: E<n>` says which fresh-project step a section corresponds to, so you
  can check the target there.
- Paths are relative to the Godot project (`pachi/pachi/`). The git root is
  one level up and stays there (see P2).
- All setup work happens on the `f-claude-refactor` branch, which is the
  working branch for this checklist. It merges to `main` once the setup is
  done (P12), not step by step.
- Once P8 (Trello) is done, put each remaining step on the board as a card in
  the Setup list.

---

## Where the repo is today (2026-11-03)

This updates the snapshot in [playbook §1](playbook.md#1-where-the-project-is-today).

| Area | State | Gap vs. target |
|---|---|---|
| Gate | `scripts/verify.sh` is **red at stage 1**: `dotnet format` flags `src/pockets/Pocket.cs` (248 whitespace errors) and `src/levels/Level.cs` (10). Stages 2–3 don't run. Stage 3 skips silently if Godot is missing. No log scan. | P1, P6 |
| Repo root | The git root is `pachi/`, one level above the Godot project. Git also tracks `pachi.01`–`pachi.04` (~2.6k files), `ref/` (5,095 files), `vendor/` (916 files), and `art/` (2 files). `.git` is 352 MB. This nesting is intentional and stays. Claude config is split: `pachi/.claude/settings.local.json` vs. `pachi/pachi/.mcp.json`. | P2 |
| Tags | None, locally or on origin. | P1 |
| Dead code | `src/cards.old/` is **already gone**, and `main_game.tscn` no longer references it. Still present: `.agents/archive/` (25 skills), `AGENTS.old.md`, `skills-lock.json`, `scripts/verify.ps1`, and `scripts/kill.sh` (whose shebang is broken: `!/bin/bash`). | P3 |
| Toolchain | Godot `4.7.2.stable.mono`, SDK 10.0.114, `Godot.NET.Sdk/4.7.2`. Build props are inline in `Pachi.csproj`, with `AnalysisLevel=latest-recommended` (unpinned). No `global.json`, `Directory.Build.props`, or `.godot-version`. `csharp-ls` and `git-lfs` aren't installed. | P0, P4 |
| Tests | Engine-only GoDotTest: `tests/TestRunner.cs`, `IntegrationTestClass.cs`, and `SceneSmokeTests` (a hand-written list of scenes, not all of them). `AutoFireSampler` is near-pure logic but imports Godot. No `core/`. | P5, P6 |
| Determinism | `Ball.cs:252` uses unseeded `GD.RandRange` for launch spread. `GameConfig.Rng` exists. | P6 |
| Engine config | Rapier2D, Mobile renderer, 960×540 viewport stretch, pixel snap, one input action (`launcher_charge`), one named layer (`Ball`). The `_mcp_game_helper` autoload comes from `godot_ai`. Two `static Instance` globals: `GameConfig`, `GlobalEvents`. One export preset (Windows Desktop) with an empty exclude filter. No ADRs. | P7 |
| Claude/agents | `AGENTS.md` (Trello pointer only) and `CODE_STYLE.md` (~9 KB). No `CLAUDE.md` or `.claude/`. `.mcp.json` (godot-ai 4.3.0 + Trello) is **gitignored** even though it holds no secrets. | P9 |
| Work state | `TODO.md`, `docs/plan/` (pitch template + beetles pitch), `docs/notes/` (playtest). | P8 |
| CI | None. | P10 |
| Untracked | `.flowbot/` (a db) isn't ignored by any repo `.gitignore`; check whether a global ignore covers it. | P3 |

---

## P0. Machine prerequisites  *(Fresh: E0)*

- [x] Already present: `godot-mono` 4.7.2 mono, .NET SDK 10.0.114, `jq`, `gh`, `uvx`.
- [x] Install `csharp-ls`: `dotnet tool install --global csharp-ls`.
- [x] In Claude Code: `/plugin install csharp-lsp@claude-plugins-official`.
- [x] Install Godot 4.7.2 export templates (needed in P7).
- [x] `git-lfs`: not needed (no LFS).
- [x] Trello MCP connected (`/mcp` shows `trello`).

**Exit:** `which csharp-ls` succeeds, and `/mcp` lists `trello` and `godot-ai`.

## P1. Get to green and tag the baseline  *(Fresh: E1 tag; playbook A1)*

The gate has to be green before anything else changes. Otherwise every later
step is verified against a red baseline.

- [x] Tag the pre-setup state of `main` before touching anything:
      `git tag legacy/pre-setup-2026-11 main && git push origin legacy/pre-setup-2026-11`.
      (Done 2026-11-06, at `d637e6e`.)
- [x] `./scripts/verify.sh --fix`, then review the diff. It should be
      whitespace-only in `Pocket.cs` and `Level.cs`. Commit it on its own
      ("format: fix whitespace"), so later blame isn't polluted.
- [x] `./scripts/verify.sh`. If stage 2 or 3 fails, fix only what's needed
      to get green; don't refactor here. Record anything non-trivial you had
      to fix in the commit message.
- [x] Work stays on `f-claude-refactor` (see "How to read it").

**Exit:** `./scripts/verify.sh` exits 0 on a clean checkout of
`f-claude-refactor`. From here on, red on that branch stops everything.

## P2. Keep the nested layout, and make it explicit  *(Fresh: E1 layout; playbook A0)*

The fresh target puts git and Godot in one directory. Pachi deliberately
doesn't: the git root (`pachi/`) holds the old prototypes `pachi.01`–`pachi.04`,
`ref/`, `vendor/`, and `art/` next to the live project (`pachi/pachi/`), so past
projects stay one `cd` away. No LFS either; binaries stay in plain git.
This step only pins down the consequences of nesting so later steps don't
trip on them.

- [x] **Claude runs from `pachi/pachi/`.** Move the root
      `pachi/.claude/settings.local.json` (it only enables `godot-ai` and
      `trello`) into `pachi/pachi/.claude/`, and delete the root `.claude/`.
      All Claude config lives next to `.mcp.json` from now on.
      (Done 2026-11-06; `settings.local.json` is now gitignored.)
- [x] **Keep agents out of the prototypes.** Add `Read(../pachi.0*/**)` and
      `Edit(../pachi.0*/**)` to `permissions.deny` (done early in
      `.claude/settings.json`). `../ref/` stays readable; it's the
      docs source the playbook points at.
- [x] **Root `.gitignore`.** Leave it as the outer ignore (`.vscode/`,
      `build/`); project ignores stay in `pachi/pachi/.gitignore` (P3).
- [ ] **Worktrees.** A `git worktree add` checks out the *whole* repo, so the
      project sits at `<worktree>/pachi/`. Check where `claude -w` puts its
      session and note the `cd pachi` (plus the one-off `--import`) in
      `CLAUDE.md` (P9) if needed. *Deferred to P9's `claude -w smoke` check.*
- [ ] **CI** (P10): set `defaults.run.working-directory: pachi` and scope
      path filters to `pachi/**`, so prototype or `ref/` changes don't run
      the gate. *Deferred to P10.*

⚖ **Shrink `.git`?** It's 352 MB, mostly `ref/`, `vendor/`, and the
prototypes. **Decided:** leave it.

**Exit:** a session started in `pachi/pachi/` shows both MCP servers in
`/mcp` with no root `.claude/`, and a read of `../pachi.01/` is denied.

## P3. Delete dead weight  *(playbook A1)*

- [x] Delete `.agents/`, `AGENTS.old.md`, and `skills-lock.json`.
- [x] `scripts/verify.ps1`: delete it unless you develop on Windows, because
      the ladder is bash-only. Fix `kill.sh`'s shebang, or delete it if unused.
      (Done 2026-11-06: both deleted; `kill.sh` was unreferenced.)
- [x] Remove the commented-out code blocks in `src/` (grep for `^\s*//` blocks
      in `Hopper.cs`, `Pocket.cs`, `Ball.cs`). (Checked 2026-11-06: none
      left, only explanatory comments.)
- [x] Extend `.gitignore`: `out/`, `coverage/`, `CLAUDE.local.md`,
      `.claude/settings.local.json`, `.flowbot/`. **Remove** `.mcp.json` from
      it (it gets committed in P9).
- [x] Add `.gdignore` to `docs/` and `scripts/` (and `out/` once it exists).

**Exit:** verify is green. A grep for `cards.old`, `AGENTS.old`, and
`.agents/` finds nothing outside the playbook.

## P4. Pin the toolchain  *(Fresh: E2; playbook A2)*

- [x] `global.json`:
      `{ "sdk": { "version": "10.0.100", "rollForward": "latestFeature" } }`.
      If you add `msbuild-sdks`, it must say `Godot.NET.Sdk: 4.7.2`, the same
      as the `.csproj`.
- [x] `.godot-version`: `4.7.2.stable.mono`. The scripts compare it against
      the prefix of `godot-mono --version` (the full string has an
      `.arch_linux.<hash>` suffix).
- [x] `Directory.Build.props`: move `Nullable`, `TreatWarningsAsErrors`,
      `AnalysisLevel`, and `EnforceCodeStyleInBuild` out of `Pachi.csproj`.
      Pin `AnalysisLevel` to `8.0-recommended`. This may surface or hide some
      warnings; fix what appears in this commit. (Nothing appeared.)
- [x] `Pachi.csproj`: keep the Android `net9.0` line. Pin the GoDotTest and
      Shouldly versions (already exact). Add the `ExportRelease` conditions
      from E2 so `tests/**` and the test packages stay out of release builds.
      (Done 2026-11-06: the exclusion is `tests/**;**/tests/**`, because
      per-feature tests live in `src/<feature>/tests/`.)
- [x] `.editorconfig` already exists and is enforced. Leave it, apart from
      anything `CODE_STYLE.md` says that it doesn't yet enforce (see P9).

**Exit:** `dotnet build Pachi.sln` has 0 warnings. An unused local in any
`src/` script fails the build. A `git worktree add` builds after a single
`--import`.

## P5. Core library and fast tests  *(Fresh: E3; playbook A4)*

- [x] `dotnet new classlib -f net8.0 -n Pachi.Core -o core` and
      `dotnet new xunit -f net8.0 -n Pachi.Core.Tests -o core.tests`. Add
      Shouldly and a ProjectReference to core, then `dotnet sln add` both.
      Add `.gdignore` to each. (Done 2026-11-06. SDK 10's templates only offer
      `net10.0`, so both were retargeted to `net8.0` by hand. `core.tests` sets
      `RollForward=Major` so it runs on a machine with only the .NET 10 runtime.)
- [x] `Pachi.csproj`: `<Compile Remove="core/**;core.tests/**" />` plus
      `<ProjectReference Include="core/Pachi.Core.csproj" />`.
- [x] **First tenant: `AutoFireSampler`.** It's already near-pure logic,
      seeded with `System.Random`, and has tests. Replace its `using Godot`
      dependencies with primitives, move it to `core/`, and port
      `src/launcher/tests/AutoFireSamplerTests.cs` to xUnit.
      `Launcher.cs` keeps calling it. (Done: the Godot `Curve` became a
      `Func<float, float>` weight; `Launcher` passes `curve.Sample`.)
- [x] Don't move anything else now. Pocket input matching moves when the
      beetle work next touches it (playbook A4: no big-bang migration).

**Exit:** `dotnet test core.tests` passes in seconds with Godot off `PATH` (e.g. in a
`mcr.microsoft.com/dotnet/sdk` container), and
`verify.sh` is still green.
(Checked 2026-11-06 with a `PATH` holding only `dotnet`, since no container
runtime is installed: 4 tests pass in about a second.)

## P6. Engine harness and verification ladder  *(Fresh: E4 + E5; playbook A3)*

Do this as several commits. Each script contract is the
[E5 table](fresh-project-setup.md#e5-verification-ladder-scripts); only the
Pachi-specific deltas are listed here.

Engine harness (`tests/`):

- [x] Replace the hand-written list in `SceneSmokeTests` with an
      **all-scenes test** that enumerates `res://src/**/*.tscn`. Keep the
      existing per-scene assertions only where they check something the
      load test doesn't (exports wired).
      (Done 2026-11-06: `AllScenesTests`; the rest became `SceneExportTests`.
      `hopper.tscn` and `launcher.tscn` are skipped as context-only, since they
      need wiring from `main_game.tscn`, which is tested.)
- [x] Seed: `TestRunner` and the input-playback helper read `--seed` and set
      `GameConfig.Rng`. Change `Ball.cs:252` launch spread to use
      `GameConfig.Rng`; the audio pitch at `Ball.cs:446` stays cosmetic.
      (Done: `Ball.cs:252` was actually the *nudge* spread. Harnesses set
      `GameConfig.SeedOverride` and `GD.Seed`; default seed 1.)
- [x] Input-playback helper (debug-only, under `tests/`) reading
      `tests/input/*.json`. One first script, `tests/input/launch_one.json`,
      presses `launcher_charge`.
- [x] Note the GoDotTest single-suite arguments for `CLAUDE.md`:
      `./scripts/verify.sh -- --run-tests=<Suite>` or
      `--run-tests=<Suite>.<Method>` (same after `--` on a raw
      `godot-mono --headless -s tests/TestRunner.cs`).

Scripts (`scripts/`):

- [x] Split today's `verify.sh` into `check.sh` (format → scene lint →
      build → `dotnet test core.tests`; `--fix`, `--quiet`) and `verify.sh`
      (check → `--import` if `.godot/` is missing → engine tests with
      `--log-file out/logs/test.log` → log scan).
- [x] `verify.sh` **fails** when Godot is missing (today it warns and skips).
- [x] Both scripts fail on a `.godot-version` mismatch.
- [x] Scene lint: a small script, which can be bash with `grep`/`awk` or a
      C# console project under `scripts/`, run on changed `.tscn`/`.tres` files.
- [x] `scripts/log-allowlist.txt`, starting empty. Run the gate once and
      triage anything that's noise. Expect Rapier2D or `godot_ai` lines; each
      allowlisted line gets a reason comment. (No noise appeared, so it's
      still empty.)
- [x] `capture.sh` per E5. Check whether Rapier2D draws collision shapes
      under `--debug-collisions`. (It does.)
- [x] `XDG_*` redirection to `out/` when the default dirs aren't writable.

**Exit:** run E5's seven break-it-on-purpose checks and revert each one.
For check 7 (determinism) under Rapier2D, two `capture.sh` runs of
`main_game.tscn` with `launch_one.json` at the same seed must match, or
ADR 0002 records that they don't and that comparisons use ranges.
(Done 2026-11-06: all seven checks fail as intended. Check 7 isn't bit-exact
under Rapier2D; see `docs/decisions/0002-capture-determinism.md`.)

## P7. Record the engine baseline as ADR 0001  *(Fresh: E6)*

Fresh E6 *decides* these settings. Here they're already decided, so the ADR
**records** them, and the decisions are only re-opened where flagged.

- [x] `docs/decisions/0000-template.md` (Context / Decision / Consequences).
- [x] `docs/decisions/0001-engine-baseline.md` records:
  - Mobile renderer; 960×540, `viewport` stretch; pixel snap; nearest-mipmap texture filter.
  - **Rapier2D** physics, and why. Switching now would mean re-tuning every feel parameter.
  - Named physics layers. Only `Ball` (layer 2) is named; name every layer
    in use while you're in the editor.
  - Named input actions: `launcher_charge`.
  - Main scene: `src/main_game/main_game.tscn` (fresh layout says
    `src/main/main.tscn`; **don't rename**, it isn't worth the churn).
- [x] `docs/decisions/0003-globals.md` (0002 went to capture determinism in P6): justify or schedule removal of the two
      `static Instance` globals (`GameConfig`, `GlobalEvents`). Fresh E6 says no
      autoloads without an ADR. These aren't autoloads, but they're the
      same kind of global state.
- [x] `_mcp_game_helper`: check whether `godot_ai`'s export plugin strips it.
      If not, exclude it from release (feature tag or exclude filter). Note the
      outcome in ADR 0001.
      (Done 2026-11-06: its `EditorExportPlugin` strips the autoload; the
      Linux `.pck` has no `_mcp_game_helper`. Layer 1 is now named `Board`.)
- [x] Export presets: add a **Linux/X11** preset (you develop on Linux), and
      add `tests/*` to every preset's exclude filter. Then run
      `godot-mono --headless --export-release "Linux" build/pachi`.

**Exit:** the exported Linux build launches and plays. ADRs 0001 and 0003 exist.

## P8. Trello board and migrating work state  *(Fresh: E8; playbook A6.8)*

Do this before P9, because the skills need the board.

- [ ] Set up the "Pachi Agent" board with the lists, labels, and two template
      cards from [A6.8](playbook.md#a6-configure-claude-code).
      Pitch template = `docs/plan/_package-template.md`; ticket template =
      Appendix D plus the B14 checklist.
- [ ] Migrate:
  - `docs/plan/202610-beetles.md` → pitch card in **Pitches**. Don't slice
        it yet; that's P11.
  - `docs/notes/20260827_jacob_playtest.md` → attachment on an Inbox card.
  - `TODO.md`: the Card Overhaul items go into the beetles pitch card. Misc,
        Juice, Visuals, and Feedback items become Ideas or Inbox cards.
        Prestige, Menus, and Save Games become single Ideas cards.
  - Remaining steps of this document → cards in **Setup**.
  - Anything settled about the game design (the README's "Vision" section,
        settled beetle rules) → `docs/design/`.
- [ ] Delete `docs/plan/`, `docs/notes/`, and `TODO.md` in one commit.

**Exit:** the board has every open item. `git ls-files docs` shows only
`process/`, `decisions/`, and `design/`.

## P9. Claude Code configuration  *(Fresh: E7; playbook A6)*

- [ ] `CLAUDE.md` from [Appendix A](playbook.md#appendix-a--draft-claudemd):
      - Docs path stays `../ref/godot-docs/`; add the worktree note from P2 if needed.
      - Main scene and globals per P7.
      - Drop the `Pocket.cs` gotcha if its multi-path wiring is already gone
        (it no longer has `GetNodeOrNull`).
- [ ] `AGENTS.md`: fold its one line into `CLAUDE.md` and delete it.
- [ ] `CODE_STYLE.md`: for each rule, if `.editorconfig` or an analyzer
      enforces it, delete it. Rules worth keeping go to
      `.claude/rules/godot-csharp.md` (Appendix C). Then delete the file.
- [ ] `.claude/settings.json` from Appendix B. Check the exact MCP tool names
      with `/mcp`; the Trello ones are `mcp__trello__trelloReadBoard`, etc.
      Fold the useful bits of your old root `settings.local.json` into the
      new `.claude/settings.local.json`.
- [ ] Commit `.mcp.json` (godot-ai pinned at 4.3.0, plus Trello).
- [ ] Skills `/shape`, `/slice`, and `/ticket` under `.claude/skills/`. Don't
      restore the archived Matt Pocock pack.
- [ ] `.worktreeinclude`: probably empty. Add it only if a worktree turns
      out to need a gitignored file.

**Exit:** E7's four checks: `/context` shows `CLAUDE.md` and the rule file
loads lazily; a `.uid` write is blocked; the Stop hook bounces a formatting
error; and in `claude -w smoke`, `./scripts/verify.sh` passes with no manual
steps. Also: `/ticket <a Setup card>` prints its spec and can comment.

## P10. CI and dependency hygiene  *(Fresh: E9; playbook A7)*

- [ ] `.github/workflows/ci.yml` on `axelmagn/pachi`: a **fast** job (no
      Godot: format, build, core tests) on every push, and a **full** job
      (setup-godot at `.godot-version`, `verify.sh` headless, upload
      `out/logs/` on failure) on PRs to `main`.
- [ ] The Rapier2D addon ships Linux `.so` binaries in-repo, so the full job
      should work on `ubuntu-latest`. Confirm it on the first run.
- [ ] Branch protection on `main` requires both jobs. ⚖ The fresh doc
      explains the solo-dev trade-off. **Recommendation:** turn it on. Claude
      is now a contributor.
- [ ] Renovate or Dependabot for NuGet, grouping `Godot.NET.Sdk` with
      `.godot-version` and `global.json`.

**Exit:** a PR with a failing core test is blocked. Fixing it unblocks the PR.

## P11. First real ticket through the B4 loop  *(Fresh: E10)*

There's no walking skeleton to build; the game already has every layer.
Instead, run **one real ticket** through the
[B4 loop](playbook.md#b4-building-a-ticket-with-claude-the-core-loop) to
prove the process end to end.

- [ ] `/slice` the beetles pitch card. Pick a ticket that touches core +
      engine + visual. Pocket input matching ("a beetle needing {red, red}
      pays out once") fits: it's the next real feature and also moves
      pocket logic into `core/`.
- [ ] Run the full B4 loop: plan comment, test-first core rule, a node that
      emits a past-tense signal with a GoDotTest, `capture.sh` evidence
      attached to the card, B5 review, B14 done, merged through CI.
- [ ] Log every workaround you needed on the card, then fix the setup.

**Exit:** the ticket merged, the card moved itself across the board, and
nothing needed a workaround (or every workaround is now fixed).

## P12. Fresh-clone acceptance test  *(Fresh: E11)*

- [ ] Rewrite `README.md`. Keep the Vision section (or move it to
      `docs/design/` and link it), and replace "Getting Started" with
      prerequisites (P0) plus "verify your setup"
      (`./scripts/check.sh`, then `./scripts/verify.sh`).
- [ ] Clone into a new directory, follow only the README, get both scripts
      green, and note the time in the README.
- [ ] Tag `baseline/1`. Every future cycle starts from it.
- [ ] Archive the Setup list. Update
      [playbook §1](playbook.md#1-where-the-project-is-today) with a
      one-line "superseded by this setup, see `baseline/1`".

**Exit:** a clean clone reaches a green gate using only the README.

---

## Deliberate deviations from the fresh-project target

| Fresh target | Pachi | Why |
|---|---|---|
| `src/main/main.tscn` | `src/main_game/main_game.tscn` | A rename buys nothing. |
| No autoloads or globals | `GameConfig` and `GlobalEvents` statics | Already load-bearing; ADR 0003 decides their fate. |
| Godot project is the git root | Project nested in `pachi/pachi/` | Keeps past prototypes and `ref/` next to the live project. |
| LFS before the first asset | No LFS | Plain git is simpler; binaries are already in history. |
| Choose the physics engine in a prototype | Rapier2D already chosen | Recorded in ADR 0001, not re-decided. |
| Full engine-free core | Only `AutoFireSampler` + new rules | No big-bang migration (A4). |
| Walking skeleton | First real ticket (P11) | The layers already exist; it's the process that needs proving. |
