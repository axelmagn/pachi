# Pachi Development Playbook

How Pachi is set up, and how each feature goes from idea to shipped. It is
written for the human developer. It decides where the LLM fits; it is not the
LLM's instruction file (that's `CLAUDE.md`, drafted in [Appendix A](#appendix-a--draft-claudemd)).

- **Last reviewed:** 2026-11-02
- **Applies to:** Godot 4.7.2 Mono, .NET 8 target (SDK 10 installed), Rapier2D, Claude Code
- **Evidence tags:** `[n]` points to [Sources](#appendix-f--sources). Each source
  has a grade: **Official** (vendor docs), **Research** (studies with data),
  **Practitioner** (named experts writing from experience), **Community**
  (forums, blogs, vendor marketing). Treat Community sources as weak.

---

## Contents

0. [How to use this playbook](#0-how-to-use-this-playbook)
1. [Where the project is today](#1-where-the-project-is-today)
2. [Why the last round of LLM work went wrong](#2-why-the-last-round-of-llm-work-went-wrong)
3. [Operating principles](#3-operating-principles)
4. [Part A: one-time setup](#part-a--one-time-setup)
5. [Part B: standard operating procedures](#part-b--standard-operating-procedures)
6. Appendices: [CLAUDE.md](#appendix-a--draft-claudemd) ·
   [settings.json](#appendix-b--draft-claudesettingsjson) ·
   [Godot/C# rules](#appendix-c--draft-clauderulesgodot-csharpmd) ·
   [ticket template](#appendix-d--ticket-template) ·
   [fresh-project setup](#appendix-e--fresh-project-setup-checklist) ·
   [sources](#appendix-f--sources)

---

## 0. How to use this playbook

- **Part A** is a checklist you work through once, in order. Every step has an
  exit criterion. Don't start feature work with an LLM until A1 through A6 are done.
  Part A *adapts* the existing repo. [Appendix E](#appendix-e--fresh-project-setup-checklist)
  describes the *ideal* setup for a fresh project, which is the target Part A moves toward.
- **Part B** is what you do every cycle and every ticket. Reread B4 (the build
  loop) and B14 (definition of done) until they're habit.
- **Change the playbook on purpose.** Do it during cool-down (B12), not
  mid-feature. If you correct the LLM about the same thing twice, the fix goes
  into `CLAUDE.md`, a rule, or a hook, and gets noted here [15].

---

## 1. Where the project is today

What I found exploring the repo on 2026-11-02:

| Area | Finding | Consequence |
|---|---|---|
| Repo shape | The git root is `pachi/`. The Godot project is `pachi/pachi/`. The repo also commits `ref/` (Godot docs, about 5k files), `vendor/` (asset packs), and four old prototypes, `pachi.01` through `pachi.04`. `.git` is 351 MB. | Clones are slow. Agents can wander into prototype code and copy its patterns. |
| Verification gate | `scripts/verify.sh` exists and is well designed (format, then build with warnings as errors, then headless GoDotTest). **It currently fails at stage 1.** `dotnet format` flags `src/cards.old/CardUI.cs` (78 issues), `src/levels/Level.cs` (10), and `src/launcher/Launcher.cs` (1). Build passes with 0 warnings. 17/17 Godot tests pass. | A gate that's always red is a gate nobody runs. This is the clearest sign the old process wasn't being followed. |
| Gate gaps | Stage 3 skips silently if `godot-mono` isn't on PATH. Godot logs script errors and C# exceptions thrown inside node callbacks without returning a non-zero exit code. Nothing scans the logs. No test checks that every scene loads. | "Tests pass" can be true while scenes are broken. |
| Test shape | All tests are engine tests (GoDotTest in headless Godot). No game logic is testable without booting the engine. | The inner loop is slow, so TDD with an agent is expensive. |
| Dead code | `src/cards.old/` (about 1,400 LOC) still compiles, and `main_game.tscn` still loads nine of its archetype scripts. `.agents/archive/` holds 25 archived skills. `AGENTS.old.md` and a `skills-lock.json` point at the archived skills. There's commented-out code in `Hopper.cs`, `Pocket.cs`, and `Ball.cs`. TODO.md says "create legacy tag", but no tags exist locally. | Agents copy whatever patterns they see [37]. Dead code teaches dead patterns. |
| LLM cruft signals | 33 of 57 `.cs` files are `[Tool]` scripts. `Pocket._Ready` tries three ways to wire one child: `[Export]`, then `GetNodeOrNull(nameof(...))`, then `GetNodeOrNull("…")`. It also has defensive null checks on every path. There are four `static Instance` singletons. | Defensive cruft and over-tooling are both typical of unreviewed LLM output [19][20]. They also go against Godot's own guidance on global state [2]. |
| Instructions | `AGENTS.old.md` tells the agent *not* to write code and points to `CODE_STYLE.md` (about 9 KB). Much of that file restates what `.editorconfig` and the analyzers already enforce. | Long instruction files dilute attention. Rules that a tool enforces don't need to be written down [15]. |
| Process assets | `docs/plan/_package-template.md` is a strong pitch template (Shape Up mixed with game-design practice). `docs/plan/202610-beetles.md` uses it. `docs/notes/` holds playtest notes. | Keep this. It's the foundation for Part B. |
| Tooling | The `godot-ai` MCP server (v4.3.0) is configured and its editor plugin is enabled. It adds an autoload, `_mcp_game_helper`, to `project.godot`. `ref/godot-docs/_sources/` has the 4.7 docs as greppable `.rst.txt`. | Good grounding assets. Check that the MCP autoload is inert or stripped in release exports (A2). |
| Determinism | `Ball.cs` draws launch spread from the global, unseeded `GD.RandRange`. `GameConfig.Rng` is an injectable `System.Random`, and `AutoFireSamplerTests` already seeds one. | Two runs of the same scene differ, so captures and engine tests can't be compared before and after a change. |

---

## 2. Why the last round of LLM work went wrong

You described the earlier output as "crufty and not aligned with the features I
wanted." The evidence gives a consistent explanation, and each cause has a
countermeasure built into this playbook.

| Cause | Evidence | Countermeasure |
|---|---|---|
| **No written definition of done.** The agent builds the most plausible thing, not your thing. | "Claude built something different from what you had in mind... It's missing information it can't get from reading the repo." [15] | Pitch, then ticket with acceptance criteria, then a fresh session builds from the ticket (B2–B4). |
| **No check it could run.** It stops when the work *looks* done. | "If it has nothing to run that tells it whether the work actually *is* done, 'looks done' is all it has to go on." [15] | Fast, always-green verification ladder, enforced by a Stop hook (A3, A6). |
| **Over-generation.** Agents add unrequested features, fallbacks, and "robustness." | Beck: genies drift "into complexity that collapses under its own weight" [20]. Willison: tests reduce over-generation [21]. GitClear: duplicated code is up, refactoring is down, in AI-era codebases [19]. | No-gos in every ticket. Review diffs for *additions you didn't ask for*. A cool-down deletion pass (B5, B12). |
| **Reviewers always find something.** Fixing every review nit adds layers. | "A reviewer that's asked to find problems will always find *something*. If you fix every finding, you end up with extra abstraction layers." [15] | Review against the spec and skip style (B5). |
| **Stale engine knowledge.** Models confuse Godot 3 and 4 APIs. | Community reports of Godot 3 idioms in Godot 4 code [22]. Godogen ships its own reference for 850+ Godot classes because models haven't mastered the API [35]. Leak dropped Bevy partly for thin training data [33]; Rapier2D is Pachi's thin-data dependency. Official C# signal semantics have non-obvious edge cases [6]. | Ground the agent in the local 4.7 docs and the Rapier2D addon's own source. Path-scoped Godot rules (Appendix C). |
| **Feeling faster than you are.** | METR's 2025 RCT: experienced developers were 19% *slower* with AI on familiar codebases, yet believed they were about 20% faster [17]. DORA 2025: AI "amplifies" whatever practices a team already has. Throughput rose, but stability suffered where foundations were weak [18]. | Measure per cycle (B12). Invest in foundations (Part A) before throughput. |

---

## 3. Operating principles

1. **Foundations first, then speed.** AI amplifies what's already there, good or bad [18]. Part A is the work that makes it amplify the good.
2. **Every change has a check that comes back pass or fail.** If you can't name the check, the ticket isn't ready [15].
3. **Logic lives where it's cheap to test.** Game rules go in plain C# with millisecond-scale tests. Nodes stay thin and wire the rules to the engine [9][10][11]. Agents edit typed logic outside scenes confidently, and stumble on deep scene trees and implicit node paths [36].
4. **You own the *what*, the LLM drafts the *how*, and you review the diff.** Beck's framing: augmented coding de-emphasises language expertise and amplifies "vision, strategy, task breakdown, and feedback loops" [20]. Those last four are your job.
5. **Two modes, never mixed.** *Exploration* (prototypes, toys, spikes) can be vibe-coded and is thrown away. *Production* (merged code) goes through spec, tests, and review [21]. Prototype code never gets merged.
6. **Small batches.** One ticket per session and one reviewable diff, about 400 changed lines or fewer [29][18].
   Multi-agent role pipelines (PM, architect, QA agents) are deliberately not
   adopted. The one first-hand Godot report is a self-reported one-day
   prototype whose failures were interface drift and thin cross-system tests [38].
7. **Deterministic for musts, instructions for shoulds.** If a rule has zero exceptions, make it a hook or deny rule. Everything else is a short line in `CLAUDE.md` [15].
8. **Write down only what the repo can't tell the agent.** Delete instructions that the code or a tool already enforces [15].
9. **Scenes are self-contained. Dependencies flow down, events flow up.** Parents call children. Children emit signals. Siblings never reach for each other. Use autoloads only for broad systems that manage their own data [1][2].
10. **Finish things.** Use fixed appetites with a circuit breaker [23]. Shipping rough beats polishing forever [28].

---

## Part A — One-time setup

Work top to bottom. Each step ends with an **Exit** line. Steps that need a
decision from you are marked **Decide**.

### A0. Decide the repo boundary  *(Decide)*

The git root holds much more than the game. Recommended layout:

- **Keep in git:** `pachi/` (the Godot project) and `art/` (source `.aseprite` files).
- **Move out of git, keep on disk:** `ref/godot-docs`. It's the agent's
  grounding corpus, but it doesn't need history. Add `ref/` to `.gitignore` and
  `git rm -r --cached ref`. Add a `scripts/fetch-godot-docs.sh` so the corpus can be rebuilt.
- **Archive and remove:** `pachi.01` through `pachi.04`. Tag the current commit
  (`git tag archive/prototypes-2026-11`) and delete the directories. Git history is the archive.
- **vendor/:** keep only packs you actually import. Raw zips belong outside git
  or in Git LFS [4]. (Rewriting history to shrink `.git` is optional. LFS
  migration of already-committed files is "more in-depth" [4]; only do it if clone size bothers you.)

**Exit:** `git ls-files | cut -d/ -f1 | sort -u` lists only what you chose to keep.

### A1. Get to green, then delete the dead code

1. Tag the legacy state: `git tag legacy/cards-v1` (TODO.md says this was done, but no tag exists locally. Check `git ls-remote --tags origin` first).
2. Delete `src/cards.old/`, `.agents/`, `AGENTS.old.md`, `skills-lock.json`, and commented-out code blocks.
3. Run `./scripts/verify.sh --fix`, then `./scripts/verify.sh`.
4. **`src/main_game/main_game.tscn` still has nine `ext_resource` entries
   pointing at `res://src/cards.old/archetypes/*.cs`.** Remove the nodes and
   resources that use them *in the editor* before deleting the folder.
   Otherwise the main scene breaks. Then delete the commented-out
   `CardDragController` lines in `Pocket.cs` and `Hopper.cs`.

**Exit:** `./scripts/verify.sh` exits 0 on a clean checkout. From now on, a red gate on `main` is a stop-everything bug.

### A2. Pin the toolchain

- Add `global.json` next to `Pachi.sln`. It pins the .NET SDK band and the
  Godot SDK, so every machine, worktree, and CI runner builds the same way [9]:
  ```json
  {
    "sdk": { "version": "10.0.100", "rollForward": "latestFeature" },
    "msbuild-sdks": { "Godot.NET.Sdk": "4.7.2" }
  }
  ```
- Move shared MSBuild properties (`Nullable`, `TreatWarningsAsErrors`,
  `AnalysisLevel`, `EnforceCodeStyleInBuild`) into `Directory.Build.props`, so
  the core library and test projects (A4) inherit them [12].
- Pin `AnalysisLevel` to a number (`8.0-recommended`) instead of
  `latest-recommended`. Then an SDK upgrade can't turn the build red overnight.
  Bump it on purpose during cool-down [12].
- Add `.godot-version` containing `4.7.2.stable.mono` and make the scripts
  check `godot-mono --version` against it.
- Install `csharp-ls` and the `csharp-lsp` Claude Code plugin. Claude then sees
  compiler diagnostics right after each edit, without a full build [16].
- **Release hygiene:** confirm whether the `_mcp_game_helper` autoload ships in
  exports. If it does, remove it from release builds with a feature tag or an
  export filter.

**Exit:** a fresh `git worktree add` builds with no manual steps beyond `godot-mono --headless --import`.

### A3. Build the verification ladder

One script that does everything is too slow for an agent's inner loop. No
script at all leaves the agent blind. Build three rungs:

| Rung | Script | Runs | Target time | Who runs it |
|---|---|---|---|---|
| 1. Fast | `scripts/check.sh` | `dotnet format --verify-no-changes` → **scene lint** → `dotnet build` → `dotnet test core.tests` | < 30 s | Claude after every edit batch; the Stop hook |
| 2. Full | `scripts/verify.sh` | Rung 1 + `--import` if `.godot/` is missing + headless GoDotTest + **all-scenes load test** + **log scan** | < 3 min | Claude before declaring a ticket done; you before commit; CI |
| 3. Look and play | `scripts/capture.sh <scene> <frames> [--input <file>] [--seed <n>] [--debug-collisions]` | Runs a scene with `--write-movie out/frame.png --fixed-fps 60 --quit-after N` [7] at a fixed seed, optionally replaying a scripted input sequence | — | Claude (it can read PNGs) and you, for visual, interaction, or feel changes |

Changes to make to the current `verify.sh`:

- **Fail, don't warn,** when `godot-mono` is missing.
- **Scan the log.** Run Godot with `--log-file`, then fail on `SCRIPT ERROR`,
  `ERROR:`, or unhandled-exception lines. Keep a small allowlist for known-benign
  engine noise. Godot reports many failures, including C# exceptions in callbacks,
  only as log lines, with exit code 0.
- **Import first in fresh checkouts.** `.godot/` is not committed [4], so new
  worktrees need `godot-mono --headless --import` before tests can load resources [7].
- **Add an all-scenes smoke test.** It walks `res://src/**/*.tscn`, instantiates
  each scene, adds it to the tree for one frame, frees it, and asserts the log is
  clean. This catches broken `ext_resource` paths and UIDs after any `.tscn` edit,
  which are the most common LLM scene breakages.
- `--movie` capture needs a real rendering driver, so `capture.sh` can't use
  `--headless`. On your desktop that's fine. In CI it needs a virtual display or a GPU-less renderer.

New pieces the case studies point to. Feedback loops, not prompt wording,
were the biggest lever in the three most detailed first-hand accounts [33][34][35]:

- **Scene lint (rung 1).** A text-only check over changed `.tscn` and `.tres`
  files: every `ExtResource("…")` and `SubResource("…")` use has a matching
  declaration, ids are unique, and every `res://` path exists. It takes
  milliseconds, so it belongs in the Stop hook's rung instead of waiting for
  the all-scenes test. Leak reported no mangled scene files after adding one [33].
- **Fixed seed.** `capture.sh` and the engine test runner pass a seed
  (`--seed <n>`, with a fixed default) to `GameConfig.Rng`, alongside
  `--fixed-fps`. Gameplay randomness goes through it (Appendix C), starting
  with the launch spread in `Ball.cs`. Cosmetic randomness such as audio pitch
  can stay unseeded. Without both, failures don't reproduce and before/after
  captures can't be compared [39]. Rapier2D's own determinism is unverified, so
  check it here with two identical runs.
- **Scripted input (play).** `--input tests/input/<name>.json` replays a list of
  timed actions (input actions, mouse moves, clicks at given frames) through a
  debug-only helper (E4). Claude can then launch balls, buy a card, or hover a
  beetle, read the frames, and decide what to try next. This was the largest
  single improvement in Leak's pipeline: Claude played through a game, found a
  health bar that didn't update, fixed it, and replayed to confirm [33].
  Recorded or scripted inputs also turn a playtest bug into a replayable check (B9) [39].
- **Collision overlay.** `--debug-collisions` passes Godot's flag of the same
  name, so frames show collision shapes. A screenshot with visible shapes fixed
  a collision offset in one pass where text descriptions had failed [34].
  Confirm the Rapier2D addon draws through Godot's debug shapes. If it doesn't,
  add a debug-only draw.
- **Sandboxed runs.** Under Claude Code's sandbox or in CI, Godot's default
  user-data, config, and cache directories may be unwritable, which breaks
  `--script` runs and exports [34]. When that happens, the scripts point
  `XDG_DATA_HOME`, `XDG_CONFIG_HOME`, and `XDG_CACHE_HOME` at directories under
  `out/`. Export templates then have to be installed or linked under the redirected data directory.

**Exit:** all three rungs work. Break something on purpose and confirm the
right rung goes red: a bad `ExtResource` id fails rung 1, and a bad
`ext_resource` path or a throw in `_Ready` fails rung 2. Two `capture.sh` runs
with the same seed and input script produce matching frames, or you've
recorded in an ADR that Rapier2D isn't deterministic enough and switched to comparing outcomes within ranges.

### A4. Split a testable core from the Godot shell

Godot's guidance is to build self-contained scenes whose dependencies are
injected [1]. The Chickensoft ecosystem, the most mature C#-first Godot
architecture, goes further. It separates logic (state machines, rules) from
presentation nodes so logic can be tested in isolation [10]. gdUnit4Net's v5
made "logic-only tests without the Godot runtime" its default, reporting
dramatic speedups [11]. For an agent, fast tests matter more than anything else
in the inner loop.

Recommended structure, inside `pachi/pachi/`:

```
core/                     # Pachi.Core.csproj — net8.0, NO GodotSharp reference
  .gdignore               # keep Godot from scanning it
  Economy/  Deck/  Board/  Save/
core.tests/               # Pachi.Core.Tests.csproj — xUnit + Shouldly
  .gdignore
src/                      # Godot nodes & scenes (thin: wire core ↔ engine)
tests/                    # GoDotTest engine tests (existing)
```

- `Pachi.csproj` gets `<ProjectReference Include="core/Pachi.Core.csproj" />`
  and `<Compile Remove="core/**;core.tests/**" />`. Godot.NET.Sdk globs every
  `*.cs` under the project directory, so without the `Remove` the core code
  compiles twice.
- **What goes in core:** payout rules, pocket input/output matching, deck,
  shop, card draw, prestige math, the economy simulation (B10), save-game
  serialization. Use plain C# types (`record`, `enum`, `int`/`long`/`double`).
- **What stays in nodes:** physics, tweens, audio, input, drawing, and the
  mapping from core events to signals.
- **Don't big-bang migrate.** The Card Overhaul ("card is a combo of scene +
  reward config", deck, prestige) is the natural first tenant. Move existing
  pocket logic only when you next touch it.

**Exit:** `dotnet test core.tests` runs without Godot installed and finishes in seconds. One real rule (for example, pocket input matching) lives there with tests.

### A5. Test strategy

| Layer | Tool | Write when | Don't bother |
|---|---|---|---|
| Rules and economy | xUnit (`dotnet test`) | Always, test-first (B4) | — |
| Scene wiring | GoDotTest, headless | A scene has required exports or children, or emits signals other systems rely on | Asserting every node exists (the all-scenes test covers loading) |
| Behaviour in engine | GoDotTest with simulated frames | A bug was caused by engine interaction (for example, a ball stuck in a pocket). Write a regression test. | Physics feel, tween curves |
| All scenes load | Generated smoke test | Once, in A3 | — |
| Visual | `capture.sh` frames | UI, legibility, layout tickets | — |
| Scripted play | `capture.sh --input` | A criterion involves interaction (launch, buy, hover), or a playtest bug needs reproducing | Judging whether it's fun |
| Feel and fun | You, then playtesters | Every cycle (B9) | Automating it |

Sea of Thieves (Rare) is a well-known example of a studio that made automated
gameplay tests a gate on integration [25]. The lesson for a solo project is to
gate on cheap tests and keep expensive tests for regressions.

**Test integrity rule (Beck [20]):** agents will weaken or delete tests to get
to green. A diff that changes a test *and* the code under test needs a
one-line justification in the ticket, and you read test diffs first (B5).

### A6. Configure Claude Code

Keep the deterministic set small and the instruction files short [15].

1. **`CLAUDE.md`** in `pachi/pachi/`, under about 60 lines. Draft in
   [Appendix A](#appendix-a--draft-claudemd). It holds commands, what's
   misleading, and decisions that aren't in the code. Delete `CODE_STYLE.md`
   content that `.editorconfig` or the analyzers already enforce. Move the rest
   to Appendix C's path-scoped rule. Personal preferences go in `CLAUDE.local.md` (gitignored).
2. **`.claude/rules/godot-csharp.md`** with `paths: ["src/**", "tests/**"]`.
   It holds the Godot 4 and C# gotchas from [Appendix C](#appendix-c--draft-clauderulesgodot-csharpmd)
   and loads only when the agent works in engine code [15].
3. **`.claude/settings.json`** (committed). Draft in [Appendix B](#appendix-b--draft-claudesettingsjson).
   - A **Stop hook** runs `scripts/check.sh`. Claude can't end a turn with a red fast gate [15].
   - A **PreToolUse hook** blocks writes to `*.uid`, `.godot/`, and `addons/`.
     Godot generates UIDs [5], and addons are third-party [3].
   - **Permissions:** allow the ladder scripts and `dotnet build/test/format`.
     Ask for `git push`.
4. **Consolidate config.** The repo has `pachi/.claude/settings.local.json` at
   the git root and `.mcp.json` in `pachi/pachi/`. Start sessions from
   `pachi/pachi/` and keep all Claude config there. Confirm with `/context` and `/mcp`.
5. **MCP hygiene.** The `godot-ai` server is useful for live inspection (editor
   screenshots, running scenes, reading the scene tree). Connected servers cost
   context even when idle [15], so disable it in sessions that only touch `core/`.
   Prefer scripts over MCP for anything you want repeatable or in CI. The
   exception is `project.godot` settings, which go through the editor or MCP (B6).
6. **Skills: start with two, add more only when a procedure repeats.**
   - `/shape`: interview you, then write a pitch using `docs/plan/_package-template.md` (B2).
   - `/slice`: break a pitch's scopes into tickets in the Appendix D format (B3).
   Don't reinstall the archived skill pack wholesale. Write down only what
   Claude can't infer [15].
7. **Worktrees for parallel sessions.** Add a `.worktreeinclude` if anything
   gitignored is needed. Every new worktree must run `--import` (A3) [15].

**Exit:** in a fresh session, ask Claude to "introduce a formatting error and stop". The Stop hook bounces it. `/context` shows `CLAUDE.md` loaded, and the rule file loads only when Claude opens a `src/` file.

### A7. Continuous integration

GitHub Actions on `axelmagn/pachi`:

- **On every push:** `dotnet format --verify-no-changes`, `dotnet build`, and
  `dotnet test core.tests`. No Godot is needed, so this is fast and cheap.
- **On PRs to `main`:** the full `verify.sh` using a Godot Mono setup action
  (for example, Chickensoft's `setup-godot`) on Ubuntu with software rendering.
  This is the setup Chickensoft's template uses [9].
- Add Renovate or Dependabot for NuGet, grouped, so Godot SDK bumps land in one PR [9].

**Exit:** a PR with a format error or a failing core test shows red in GitHub.

### A8. Setup exit checklist

- [ ] A0 repo boundary decided and applied
- [ ] A1 `verify.sh` green; dead code deleted; legacy tag exists
- [ ] A2 `global.json`, `Directory.Build.props`, `.godot-version`, `csharp-lsp`
- [ ] A3 three-rung ladder with scene lint, fixed seed, and input scripts; scene lint, log scan, and all-scenes test proven by breaking something
- [ ] A4 `core/` and `core.tests/` exist with at least one real rule
- [ ] A6 `CLAUDE.md`, rule file, `settings.json` hooks working; `CODE_STYLE.md` trimmed
- [ ] A7 CI red/green proven
- [ ] Old `README.md` "Getting Started" updated to point at `scripts/check.sh`

---

## Part B — Standard operating procedures

### B1. The cycle

This is Shape Up [23] scaled down for one person. Your pitch template already uses its vocabulary.

```
 ┌── Shape (cool-down) ──┐  ┌──────── Build cycle (appetite: 1–3 wks) ────────┐  ┌─ Cool-down (3–5 days) ─┐
 │ playtest notes → pitch │→ │ slice → tickets → build loop (B4) per ticket   │→ │ playtest (B9)          │
 │ bet: pick ONE pitch    │  │ hill-chart the scopes; cut nice-to-haves early │  │ refactor/delete (B12)  │
 └────────────────────────┘  └────────────────────────────────────────────────┘  │ update playbook/rules  │
                                                                                  │ shape next pitch       │
                                                                                  └────────────────────────┘
```

- **One bet at a time.** The appetite is fixed and the scope is variable.
- **Circuit breaker:** if the appetite runs out, stop. In cool-down, decide
  whether to re-shape or drop. Never extend by default [23].
- **Hill chart:** in the pitch's Scopes section, mark each scope `⬆ uphill`
  (unknowns remain) or `⬇ downhill` (execution only). Ask an LLM for help on
  uphill scopes as a *spike* (B8), not as production code.

### B2. Shaping a pitch

1. Start from evidence: a playtest note, a clip, a feeling you had while playing.
2. Run `/shape` (or ask Claude to "interview me until you could write the pitch").
   Answer its questions. It writes `docs/plan/YYYYMM-<slug>.md` from the template [15].
3. You edit the result. The **experience goal**, **appetite**, and **no-gos**
   must be in your words. They're how you'll judge the LLM's work later.
4. Fill in **Rabbit holes**. Each technical risk becomes a spike ticket that
   runs *before* production tickets.
5. Record any architectural decision as a short ADR in `docs/decisions/NNNN-title.md`
   (context, decision, consequences) [30]. These are exactly the decisions
   that aren't in the code, which agents can't see [15].

### B3. Slicing into tickets

A ticket is the unit of one Claude session. Run `/slice` on the pitch, then edit.

- **Vertical:** each ticket yields something you can see or test in the game.
  Avoid tickets like "add models" followed by "add UI".
- **Small:** one session, at most about 400 changed lines [29]. If Claude's plan
  for a ticket touches more than about 6 files, split it.
- **Format:** [Appendix D](#appendix-d--ticket-template). It has acceptance
  criteria, explicit no-gos, a named check, and the layer each criterion is
  tested at (core, engine, visual, or you).
- Tickets live in the pitch's Scopes section as a checklist, so `CLAUDE.md` can
  point to one place: "the active pitch".

### B4. Building a ticket with Claude (the core loop)

```
fresh session → orient → plan → (approve) → red → green → verify → self-review → you review & play → commit
```

1. **Fresh session per ticket.** Use `/clear` or a new session, so a clean
   context starts from the written ticket [15].
2. **Orient.** The prompt names the ticket and the files to look at. For example:
   > Implement ticket `beetles#3` in `docs/plan/202610-beetles.md`. Follow the
   > pattern in `src/pockets/Pocket.cs`. Start by running `./scripts/check.sh`
   > and reading the ticket. Stay inside its no-gos.

   Starting by running the tests is a cheap way to orient the agent [21].
3. **Plan.** For anything beyond a trivial change, have Claude propose a plan
   first (plan mode). Reject plans that add abstractions, singletons, or
   fallbacks the ticket didn't ask for.
4. **Red.** Core-layer criteria get a failing xUnit test first. Engine-layer
   criteria get a failing GoDotTest. Confirm it fails for the right reason [20][21].
5. **Green.** Implement the minimum. Claude iterates against `check.sh`, and the
   Stop hook enforces it.
6. **Verify.** Claude runs `verify.sh`. For visual or interaction tickets, it
   runs `capture.sh`, with an input script when a criterion involves
   interaction, and looks at the frames. For physics bugs, add
   `--debug-collisions` and show it the frames instead of describing the bug [34].
   Ask for **evidence**: the command and the tail of its output, not "all tests pass" [15].
7. **Self-review in a fresh context.** Run `/code-review`, or use the
   spec-review prompt in B5. Fix only correctness gaps.
8. **You review and play** (B5). Feel changes require you to run the game. Use `/verify` or F5.
9. **Commit** (B13). Tick the ticket in the pitch.

**Steering:** watch the first minute and press `Esc` the moment it drifts.
After two failed corrections, `/clear` and rewrite the prompt instead of
trying a third time. `/rewind` makes trying a bolder approach cheap [15].
**Effort:** medium for routine tickets, high for bugs in code you didn't write
or changes that ripple across many callers [15].

### B5. Reviewing LLM output

Review against the ticket, not against perfection.

**Spec-review prompt** (run in a fresh subagent) [15]:
> Use a subagent to review the current diff against ticket `<id>` in
> `<pitch file>`. Check every acceptance criterion is implemented and tested at
> the stated layer and every no-go is respected. Report gaps that affect
> correctness. Skip style.

**Your review checklist, in this order:**

1. **Test diffs first.** Was any test deleted, loosened, or skipped? Was an assert changed to match new behaviour? [20]
2. **Unrequested additions.** Look for new options, fallbacks, `[Tool]`
   attributes, singletons, "just in case" null paths, config knobs, and
   generalised helpers with one caller. Delete them [19][20].
3. **Duplication.** Does this copy an existing helper instead of reusing it? (GitClear's main AI-era finding [19].)
4. **Godot shape.** Dependencies flow down, signals go up, no sibling reach-ins,
   no new autoload. Required refs are `[Export]` plus an assert [1][2].
5. **Scene and resource diffs.** Were `.tscn` changes intended? Watch for
   `uid://` values that look made up, shared-resource mutation (B6), and stray
   `load_steps` churn.
6. **Size.** Over about 400 lines? Ask why, and split if possible [29].

### B6. Scenes, resources, and editor work

`.tscn` and `.tres` files are text and documented [8], so agents *can* edit
them. But they break in quiet ways: an `ext_resource` id mismatch, an invented
UID, a missing sub-resource.

- **Claude may edit `.tscn` and `.tres` text** for property tweaks, re-parenting,
  adding a few nodes, or wiring `[Export]` paths, following the TSCN format doc
  (`../ref/godot-docs/_sources/engine_details/file_formats/tscn.rst.txt`).
  The all-scenes test must pass afterwards.
- **New scenes, or edits that restructure a scene:** create them in the editor,
  or through the `godot-ai` MCP tools, which go through the editor's own
  serializer. Then open and save once in the editor to normalise the file.
- **Never hand-write `uid://` values or `.uid` files.** The PreToolUse hook
  blocks `.uid` writes. Commit `.uid` files with their source file [5].
- **Pull before opening the editor.** Opening an editor on stale files can
  regenerate UIDs and cause churn [5].
- **Shared resources:** a `Resource` assigned in a `.tscn` is shared across
  instances. Use `ResourceLocalToScene` or `Duplicate()` before mutating it.
  `Pocket.cs` already does the latter for arrays. Make it the rule.
- **Physics layers, input actions, and autoloads** change `project.godot`.
  Don't hand-edit them; agents get the input-map format wrong [33]. Make the
  change in the editor (as E6 does), or have Claude use the `godot-ai` tools
  (`input_map_manage`, `autoload_manage`, `project_manage`), which write
  through the editor. Keep such changes in their own commits.

### B7. Bugs

1. Reproduce. Write the observed and expected behaviour in a ticket.
2. **Failing test first,** at the lowest layer that can show the bug (core, then engine) [15].
3. Fix and verify. Keep the test as a regression test.
4. If the bug was caused by a misleading pattern, add a line to `CLAUDE.md` or the rule file. This is how "corrected twice" gets institutionalised [15].

### B8. Prototypes and spikes  *(exploration mode)*

Use these for uphill scopes and "is this fun?" questions.

- Branch `proto/<slug>` or a folder `prototypes/<slug>/` with `.gdignore` removed
  only on that branch. **Vibe coding is fine here** [21]: fast, ugly, greybox,
  temp audio.
- The output is *knowledge*: a clip, a note in the pitch, a decision. The
  output is **never a merge**. Production code is rewritten from the ticket in
  build mode, and the prototype is the reference.
- Timebox it in hours, not days. If the toy isn't producing the feeling, that's the answer.

### B9. Playtesting

- **Cadence:** at the end of each cycle, minimum, plus an informal self-play
  session every few days in build.
- **Who and how many:** about 5 people per round, in more rounds rather than
  more people per round. Five testers surface most of the usability problems a
  round can find, and iterating catches the rest [26]. (Your template says at
  least 7 at cycle end. That's fine as a target. The point is to test often.)
- **Facilitation:** give them the build and a single sentence of context. Then
  stay silent. Ask them to think aloud. Answer "what do I do?" with "what do
  you think you should do?" [27]. Write down what they *do*, then what they say.
- **Afterwards:** raw notes go to `docs/notes/YYYYMMDD_<name>_playtest.md`
  (as you already do). Within a day, triage them into one of three buckets:
  - a bug, which becomes a ticket now. If input can reproduce it, the ticket
    starts with an input script that shows it (A3), and the script stays as a regression check [39]
  - a legibility or friction issue, which becomes a candidate for the next pitch
  - an idea, which goes into a brainstorm list and doesn't count as a commitment
- Your 2026-08-27 notes are a good model. "Cards confusing", "icons hard to
  read", and "too many downgrades" are exactly the legibility signals to pitch against.

### B10. Economy and balancing

Incremental games run on exponential cost curves and layered resets
(prestige). Their balance is a math problem before it's a feel problem [24].

- Model the economy in `core/Economy/` as pure functions: cost curves, payout
  tables, prestige conversion.
- Add a **headless simulation** test or CLI. It plays N minutes with a simple
  policy (for example, always buy the cheapest upgrade) and reports
  time-to-next-purchase and time-to-prestige. Assert on *ranges*, for example
  "first prestige between 12 and 20 minutes". A balance change that breaks the
  pacing then fails CI instead of a playtest.
- Tune numbers in `.tres` resources or a data file, never as magic numbers in nodes.
- Pecorella's "Math of Idle Games" series and spreadsheets are the reference for the curve shapes [24].

### B11. Juice and polish

- **Concentric rule** (from your template): a mechanic's own feel is finished
  to shippable quality while its surroundings stay greybox. Polish moves outward.
- Juice (screenshake, particles, sound, squash, lights) comes *after* the
  mechanic is proven fun in a greybox playtest. It amplifies fun that's already
  there; it doesn't create fun (Jonasson & Purho, *Juice it or lose it* [31]).
- Juice tickets are visual-layer tickets. Their check is a `capture.sh`
  before/after, and you play it.
- **Performance:** in `_Process` and `_PhysicsProcess`, cache `StringName`,
  `NodePath`, and frequently read engine properties in fields to avoid
  per-frame allocations and interop calls. Profile before optimising further,
  because results vary [14]. The Godot profiler doesn't cover C#; use
  dotTrace/Rider or `dotnet-counters` [14].

### B12. Cool-down

Run these in order, in a fresh session where needed:

1. **Playtest** (B9) and triage.
2. **Deletion pass.** Ask Claude to list dead code, unused exports, single-caller
   abstractions, `[Tool]` scripts that don't need to be tools, and duplicated
   helpers. *You* pick what goes. Use `/simplify` on the cycle's diff
   (`git diff <cycle-start-tag>..HEAD`). GitClear's data says refactoring is
   what disappears in AI-assisted codebases, so schedule it [19].
3. **Instruction audit.** Trim `CLAUDE.md` and the rule files: remove what
   didn't matter, add what you corrected twice. Run `/doctor` to get suggested cuts [15].
   Also review Claude Code's auto-memory for this project (`/memory`, or
   `~/.claude/projects/<project>/memory/`). It persists across sessions and can
   lock in stale choices: Leak's memory file pushed every game toward the same
   visual style until he cleared it [33]. Delete entries that are wrong or that
   `CLAUDE.md` now covers.
4. **Measure.** Write three lines in the pitch's retro: tickets shipped vs
   planned, tickets that needed more than two correction rounds, and whether
   LLM sessions felt faster *and* were faster. METR shows those two can
   disagree [17].
5. **Bump pinned versions** (`AnalysisLevel`, NuGet, Godot patch) on purpose, then re-run the full gate.
6. Shape and bet the next pitch (B2).
7. Tag: `cycle/YYYYMM-<slug>-end`.

### B13. Git conventions

- `main` is always green. Each feature gets a branch (`f-<slug>`), and a ticket
  is one or a few commits.
- Commit messages: `<area>: <imperative summary>`, with the ticket id in the
  body. Claude adds its co-author trailer.
- Commit `.uid` files along with their sources. `.godot/` stays ignored [4][5].
- Squash-merge feature branches, so each `main` commit corresponds to a reviewed ticket.
- Claude may commit on a feature branch when asked. **`git push` and merges to `main` are yours.**

### B14. Definition of done (per ticket)

- [ ] Every acceptance criterion is met and tested at its stated layer
- [ ] No-gos respected; no unrequested additions (B5 #2)
- [ ] `./scripts/verify.sh` green, and you saw its output (not just a claim)
- [ ] Visual or feel tickets: frames captured or the game played by you
- [ ] Test changes justified (A5 integrity rule)
- [ ] Ticket ticked in the pitch; new decisions recorded as ADRs; `CLAUDE.md` updated if you corrected something twice

---

## Appendix A — Draft `CLAUDE.md`

This is a starting point. Keep it under about 60 lines and prune it every cool-down [15].

```markdown
# Pachi

Incremental pachinko deckbuilder. Godot 4.7.2 Mono, C# (.NET 8), Rapier2D physics.
Process: docs/process/playbook.md (B4 is the per-ticket loop).
Current work: the active pitch in docs/plan/ (latest YYYYMM-*.md). Read its ticket before starting.

## Commands
- Fast check (format, build, core tests): ./scripts/check.sh   (--fix to auto-format)
- Full gate (+ headless Godot tests, all-scenes load, log scan): ./scripts/verify.sh
- One core test: dotnet test core.tests --filter "FullyQualifiedName~<TestClass>"
- Godot tests only: godot-mono --headless -s tests/TestRunner.cs
- Fresh worktree: run `godot-mono --headless --import` first (.godot/ isn't committed)
- See or play a scene: ./scripts/capture.sh res://src/<path>.tscn <frames> [--input tests/input/<name>.json] [--seed <n>] [--debug-collisions]  → PNG frames you can read

## Where things go
- Game rules with no engine dependency → core/ (Pachi.Core, no Godot types). New logic starts here, test-first.
- Nodes and scenes → src/<domain>/. Nodes stay thin: they map core state/events to the engine.
- Decisions → docs/decisions/. Playtest notes → docs/notes/.

## Conventions the code won't tell you
- Required node refs: [Export] assigned in the scene + Debug.Assert in _Ready. No GetNode fallbacks or auto-resolution.
- Allowed globals: GameConfig, GlobalEvents. Don't add autoloads or static Instance singletons.
- [Tool] only when the script must run or draw in the editor.
- Never write .uid files or uid:// values; Godot generates them.
- Input actions, physics layers, autoloads: change them via the editor or godot-ai tools, never by editing project.godot text.
- No commented-out code; git history is the archive.
- Don't add options, fallbacks, or abstractions the ticket didn't ask for. If you think one is needed, say so instead.
- Changing a test and the code it tests in one diff needs a one-line reason in your summary.

## Gotchas
- Godot API knowledge: check ../ref/godot-docs/_sources/ (4.7 docs; classes/class_*.rst.txt). Training data skews Godot 3.
- Physics engine is Rapier2D (addons/godot-rapier2d), not Godot Physics. Models know it poorly: read the addon's docs and source before relying on any Rapier2D behaviour or API.
- Godot logs errors (including C# exceptions in callbacks) without a failing exit code; trust verify.sh, not "no crash".
- src/pockets/Pocket.cs predates these rules (multi-path wiring, defensive nulls). Don't copy those parts.
```

## Appendix B — Draft `.claude/settings.json`

```json
{
  "permissions": {
    "allow": [
      "Bash(./scripts/check.sh*)",
      "Bash(./scripts/verify.sh*)",
      "Bash(./scripts/capture.sh*)",
      "Bash(dotnet build*)",
      "Bash(dotnet test*)",
      "Bash(dotnet format*)",
      "Bash(godot-mono --headless*)"
    ],
    "ask": ["Bash(git push*)"],
    "deny": ["Read(./.env)", "Read(./.env.*)"]
  },
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "f=$(jq -r '.tool_input.file_path'); case \"$f\" in *.uid|*/.godot/*|*/addons/*) echo \"Blocked: $f is generated by Godot or third-party. Use the editor/--import instead.\" >&2; exit 2;; esac; exit 0"
          }
        ]
      }
    ],
    "Stop": [
      {
        "hooks": [
          { "type": "command", "command": "cd \"$CLAUDE_PROJECT_DIR\" && ./scripts/check.sh --quiet 1>&2 || exit 2" }
        ]
      }
    ]
  }
}
```

Notes:

- Make `check.sh --quiet` exit 0 immediately when `git status --porcelain`
  shows no changed `*.cs`, `*.csproj`, `*.tscn`, or `*.tres` files. That way Q&A
  turns don't pay for a build.
- The Stop hook enforces only the *fast* rung. The full gate is a B14 checklist
  item and a CI check. Putting a 3-minute gate on every turn would make
  interactive work painful.
- After eight consecutive blocks, Claude Code lets the turn end anyway [15]. If
  that happens, it's a signal the ticket is too big or the prompt is wrong.

## Appendix C — Draft `.claude/rules/godot-csharp.md`

```markdown
---
paths:
  - "src/**"
  - "tests/**"
---
- Godot 4.7 APIs only. Verify unfamiliar members in ../ref/godot-docs/_sources/classes/class_<name>.rst.txt.
  (CharacterBody2D not KinematicBody2D; `await ToSignal(...)` not yield; signals are C# events.)
- Custom signals: `[Signal] public delegate void BallCaughtEventHandler(...)`; raise with `EmitSignal(SignalName.BallCaught, ...)`, never Invoke.
- Signals connected to lambdas that capture, and custom signals, may NOT auto-disconnect when the receiver is freed: unsubscribe (`-=`) in _ExitTree.
- Physics work in _PhysicsProcess; visuals in _Process.
- Rapier2D is not Godot Physics: check addons/godot-rapier2d docs/source before assuming body, joint, or contact behaviour.
- Physics or collision bug: capture with ./scripts/capture.sh <scene> <frames> --debug-collisions and read the frames before guessing at offsets.
- Gameplay randomness goes through GameConfig.Rng so runs can be seeded. GD.Randf/GD.RandRange only for cosmetic variation (audio pitch, particles).
- Call down, signal up: parents call child methods, children emit past-tense signals. No sibling GetNode("../X").
- Resources assigned in scenes are shared between instances: Duplicate() or ResourceLocalToScene before mutating.
- Use generated PropertyName/SignalName/MethodName and cached StringName fields, not string literals, especially in per-frame code.
- Value-type math structs: `Position = Position with { X = 10 }`, not `Position.X = 10`.
- In [Tool] scripts: guard runtime logic with `Engine.IsEditorHint()`; set `Owner = GetTree().EditedSceneRoot` on spawned children.
- Editing .tscn/.tres text: follow ../ref/godot-docs/_sources/engine_details/file_formats/tscn.rst.txt; keep ext_resource ids consistent; then run ./scripts/verify.sh (all-scenes test).
```

Most of the current `CODE_STYLE.md` (naming, bracing, aliases) is already
enforced by `.editorconfig` with `EnforceCodeStyleInBuild`. Keep that file only
for human reference, or delete it. Agents learn those rules from the build errors [15].

## Appendix D — Ticket template

Lives inside the pitch's Scopes section.

```markdown
### <pitch-slug>#<n> — <verb phrase, player-visible>
**Scope:** <scope name> (⬆ uphill | ⬇ downhill)  **Size:** S | M  (L → split it)

**Acceptance criteria** (layer in brackets: core | engine | visual | you)
- [ ] [core]   Given a beetle needing {red, red}, catching red then red pays out its outputs once
- [ ] [engine] beetle.tscn emits PaidOut with the output list when its catch hole receives matching balls
- [ ] [visual] Hovering a beetle opens its wings within 0.2 s; outputs are legible at 960×540
- [ ] [you]    Opening wings feels responsive, not floaty

**No-gos:** no dynamic placement; no new autoloads; don't touch launcher/
**Pattern to follow:** src/pockets/PocketArmsController.cs (tween usage)
**Check:** ./scripts/verify.sh + capture.sh res://src/pockets/beetle_blue.tscn 120
**Notes / test-change justifications:**
```

## Appendix E — Fresh-project setup checklist

This describes the **ideal setup for a brand-new project** that follows this
playbook. It is not adapted to Pachi's current repo; Part A handles that.
Use it as the reference point when deciding how far to move the existing
project toward it.

**How to read it.**

- Steps are in dependency order. Each step is roughly ticket-sized and ends with
  an **Exit** check, so you can hand a step to Claude as a ticket once E5 exists.
- **⚖ Judgement call** marks a real trade-off. That's where you're most likely to
  deviate, and the note says what you'd give up.
- `<Game>` stands for the project or assembly name.

**Why this order:**

- The gate exists before any game code, so no code is ever written without a check (principle 2).
- Claude is configured only after the scripts exist, because `CLAUDE.md` is mostly those commands.
- A walking skeleton comes last. It proves every layer and the whole B4 loop
  end to end before real features start [32].

### Target layout

```
<repo>/                         # git root == Godot project root (one root for Godot, dotnet, Claude, CI)
  project.godot  <Game>.csproj  <Game>.sln  export_presets.cfg
  global.json  Directory.Build.props  .editorconfig  .godot-version
  .gitignore  .gitattributes  .worktreeinclude
  CLAUDE.md  .mcp.json  .claude/{settings.json, rules/, skills/}
  README.md
  src/<domain>/                 # nodes + scenes + their scripts, grouped by feature [3]
  assets/                       # shared art/audio/fonts (feature-specific assets live beside their scene)
  addons/                       # third-party only [3]
  tests/                        # GoDotTest engine tests (excluded from release exports)
  core/          .gdignore      # <Game>.Core.csproj — plain C#, no Godot reference
  core.tests/    .gdignore      # <Game>.Core.Tests.csproj — xUnit
  scripts/       .gdignore      # check.sh, verify.sh, capture.sh, log-allowlist.txt
  docs/          .gdignore      # process/, plan/, notes/, decisions/
  out/                          # gitignored: logs, captures, coverage
```

⚖ **Single root vs. nested project.** With a single root, the Godot project,
the dotnet solution, Claude's config, and CI all start in the same directory.
That's the source of the split-root friction in §1. The alternative is
`game/` next to `core/`. That keeps `Compile Remove` out of the Godot
`.csproj`, but you have to configure where the solution lives and run every
tool from the right directory. Pick single root unless the repo also holds
non-Godot products.

### E0. Machine prerequisites

- [ ] Godot **Mono** build at the exact version you'll pin, plus its export templates.
- [ ] .NET SDK. It must be at least the version Godot's C# target needs; this playbook uses net8.0 with SDK 10.
- [ ] `git`, `git-lfs`, `jq` (used by hooks), and `gh`.
- [ ] `csharp-ls`: `dotnet tool install --global csharp-ls` [16].
- [ ] Claude Code, with the `csharp-lsp` plugin: `/plugin install csharp-lsp@claude-plugins-official` [16].
- [ ] Optional: `uvx`, if you'll use the `godot-ai` MCP server.

**Exit:** `godot-mono --version`, `dotnet --list-sdks`, `git lfs version`, `jq --version`, and `which csharp-ls` all succeed.

### E1. Repository and Godot skeleton

- [ ] Create the project in the Godot project manager. Choose **Version
      control metadata: Git**, which generates `.gitignore` and `.gitattributes`
      with LF enforcement [4]. Choose the renderer now; changing it later touches many settings.
- [ ] Create the C# solution: **Project → Tools → C# → Create C# solution**.
      This writes `<Game>.csproj` and `<Game>.sln`.
- [ ] `git init`, then `git lfs install`. Add LFS patterns to `.gitattributes`
      **before the first asset commit** (`*.png *.wav *.ogg *.aseprite *.ttf
      *.zip`), because migrating already-committed files later is painful [4].
- [ ] Extend `.gitignore`: `.godot/`, `out/`, `build/`, `CLAUDE.local.md`,
      `.claude/settings.local.json`, `coverage/` [4].
- [ ] Create the target layout's folders. Put an empty `.gdignore` in `core/`,
      `core.tests/`, `scripts/`, `docs/`, and `out/` so Godot doesn't scan or
      UID-tag them [3].
- [ ] Write `README.md`. It needs three sections: what the game is (one
      paragraph), prerequisites (E0), and "verify your setup" (`./scripts/check.sh`, then `./scripts/verify.sh`).
- [ ] First commit, plus a tag: `skeleton/0`.

⚖ **LFS at all?** For a small 2D game with a few MB of assets, plain git is
fine and simpler. LFS earns its keep once source art (`.aseprite`, `.psd`) or
audio stems accumulate. The cost of choosing later is a history rewrite.

**Exit:** `git status` is clean after opening and closing the editor, and `git check-ignore .godot` succeeds.

### E2. Toolchain pinning and build properties

- [ ] `global.json`: pin the .NET SDK band (`rollForward: latestFeature`).
      Godot writes its SDK version into the `.csproj` `Sdk="Godot.NET.Sdk/x.y.z"`
      attribute. If you also add `msbuild-sdks` to `global.json` [9], keep the two identical.
- [ ] `.godot-version`: the exact `godot-mono --version` string. The scripts read it (E5).
- [ ] `Directory.Build.props`: shared by all three projects [12]:
  ```xml
  <Project>
    <PropertyGroup>
      <Nullable>enable</Nullable>
      <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
      <AnalysisLevel>8.0-recommended</AnalysisLevel>   <!-- pinned; bump on purpose in cool-down -->
      <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    </PropertyGroup>
  </Project>
  ```
- [ ] `.editorconfig`: start from `dotnet new editorconfig`. Escalate only the
      style rules you want *enforced* to `warning`, which becomes an error under
      warnings-as-errors. Everything you'd otherwise write in a style guide
      should either live here or not exist [15].
- [ ] `<Game>.csproj`: keep it minimal. It should only reference core, exclude
      the nested projects from the Godot glob, and keep test code out of release exports:
  ```xml
  <Project Sdk="Godot.NET.Sdk/x.y.z">
    <PropertyGroup>
      <TargetFramework>net8.0</TargetFramework>
      <EnableDynamicLoading>true</EnableDynamicLoading>
    </PropertyGroup>
    <ItemGroup>
      <Compile Remove="core/**;core.tests/**" />   <!-- Godot.NET.Sdk globs every *.cs under the root -->
      <ProjectReference Include="core/<Game>.Core.csproj" />
    </ItemGroup>
    <ItemGroup Condition="'$(Configuration)' == 'ExportRelease'">
      <Compile Remove="tests/**" />
    </ItemGroup>
    <ItemGroup Condition="'$(Configuration)' != 'ExportRelease'">
      <PackageReference Include="Chickensoft.GoDotTest" Version="<exact>" />
      <PackageReference Include="Shouldly" Version="<exact>" />
    </ItemGroup>
  </Project>
  ```
  The release-only exclusion follows Chickensoft's template [9]. Pin exact
  package versions, and let Renovate bump them (E9).

**Exit:** `dotnet build <Game>.sln` succeeds with 0 warnings. Adding an unused
local variable to a script fails the build.

### E3. Core library and fast tests

- [ ] `dotnet new classlib -f net8.0 -n <Game>.Core -o core`
- [ ] `dotnet new xunit -f net8.0 -n <Game>.Core.Tests -o core.tests`, then add Shouldly and a `ProjectReference` to core.
- [ ] `dotnet sln add core/<Game>.Core.csproj core.tests/<Game>.Core.Tests.csproj`
- [ ] Rule: `core` has **no** `GodotSharp` reference. If you need a vector,
      define a tiny value type or pass primitives. That's what lets these tests
      run without the engine [10][11].
- [ ] One real test-first rule. For example, `Wallet.Spend` refuses an overdraft.

⚖ **xUnit vs gdUnit4Net.**
- **xUnit with GoDotTest:** standard tooling for the core, and the
  engine-test runner you already know.
- **gdUnit4Net v5:** one framework for both layers, with logic-only tests
  running without the engine and `[RequireGodotRuntime]` for engine tests [11].
  This is attractive on a fresh project. The trade-off is betting on a single,
  smaller ecosystem for everything.

**Exit:** `dotnet test core.tests` passes on a machine *without* Godot installed, in seconds.

### E4. Engine test harness

- [ ] `tests/TestRunner.cs`: a `SceneTree` script run with `-s`. It runs
      GoDotTest, yields a couple of frames so `QueueFree` completes, and exits
      `0` or `1` from the test result. Pachi's current `tests/TestRunner.cs` is a good model.
- [ ] `tests/IntegrationTestClass.cs`: base class that instantiates scenes,
      tracks them, and frees them in `[Cleanup]`. This prevents RID leak noise in logs.
- [ ] **All-scenes smoke test:** enumerate `res://src/**/*.tscn`, instantiate
      each, add it to the tree for one frame, then free it. Failures surface
      through the log scan (E5).
- [ ] **Input playback helper** for `capture.sh --input`: a debug-only node
      under `tests/` that reads `tests/input/*.json` (a list of
      `{ frame, action | mouse }` entries) and injects them with
      `Input.ParseInputEvent` at fixed frames. It also reads the seed and
      applies it to the game's RNG [33][39].
- [ ] In every export preset's **exclude filter**, add `tests/*`.
- [ ] Find and note the GoDotTest arguments for running a single suite. They go into `CLAUDE.md` (E7).

**Exit:** `godot-mono --headless -s tests/TestRunner.cs` exits 0. Temporarily
pointing a scene's `ext_resource` at a missing file makes the run (with E5's log scan) fail.

### E5. Verification ladder scripts

Write these as specs first, then implement. Each script's contract:

| Script | Contract |
|---|---|
| `scripts/check.sh` | Runs `dotnet format --verify-no-changes`, then the scene lint on changed `.tscn`/`.tres` files (A3), then `dotnet build`, then `dotnet test core.tests`. `--fix` formats instead of verifying. `--quiet` exits 0 immediately if `git status --porcelain` shows no `*.cs`, `*.csproj`, `*.tscn`, or `*.tres` changes; that's for the Stop hook. Fails if `.godot-version` doesn't match. Target: under 30 s. |
| `scripts/verify.sh` | Runs `check.sh`. If `.godot/` is missing, runs `godot-mono --headless --import` [7]. Runs the engine tests with `--log-file out/logs/test.log`. **Fails if Godot is missing** (never skips). **Fails if the log contains** `SCRIPT ERROR`, `ERROR:`, or an unhandled exception, unless the line matches `scripts/log-allowlist.txt`. Prints a one-screen summary with the log path. |
| `scripts/capture.sh <scene> <frames> [--input <file>] [--seed <n>] [--debug-collisions]` | Runs the scene with `--write-movie out/capture/<name>/frame.png --fixed-fps 60 --quit-after <frames>` [7] at a fixed seed (default when `--seed` is absent), replays `--input` through the E4 helper, passes `--debug-collisions` through, and prints the frame directory. Not headless: movie capture needs a real renderer. |

- [ ] Every script `cd`s to the repo root itself and uses `set -euo pipefail`. Exit codes are the interface.
- [ ] If Godot's default data, config, or cache directories aren't writable (sandbox, CI), the scripts point the `XDG_*` variables at `out/` (A3) [34].
- [ ] `log-allowlist.txt` starts empty. Each entry gets a comment explaining why it's benign.

**Exit:** prove each failure mode by breaking it on purpose, then revert:

1. A misformatted line makes `check.sh` fail.
2. A failing core assertion makes `check.sh` fail.
3. `throw` in a `_Ready` makes `verify.sh` fail through the log scan.
4. A broken `ext_resource` path makes `verify.sh` fail through the all-scenes test.
5. Renaming `.godot-version` content makes both scripts fail.
6. An `ExtResource("…")` use with no matching declaration makes `check.sh` fail through the scene lint.
7. Two `capture.sh` runs with the same seed and input script produce the same
   frames. If the physics engine isn't deterministic enough for that, record it
   in an ADR and compare outcomes (positions, wallet) within ranges instead.

### E6. Godot project baseline, recorded as ADR 0001

Decide these once, write them in `docs/decisions/0001-engine-baseline.md` [30],
and set them in the editor, not by hand-editing `project.godot`:

- [ ] Renderer, base resolution, stretch mode, and pixel snapping and texture filter if it's pixel art.
- [ ] Physics engine (built-in or Rapier2D), physics tick rate, and **named** physics layers.
- [ ] **Named** input actions. Code refers to actions, never to keys.
- [ ] Main scene path (`src/main/main.tscn`).
- [ ] Autoloads: **none** to start. Each later autoload needs its own ADR
      justifying why it's a broad, self-contained system [2].
- [ ] One desktop export preset. Commit `export_presets.cfg`; in Godot 4.1+,
      credentials are stored separately and stay out of git [4]. Do a first
      export now: `godot-mono --headless --export-release "<preset>" build/<game>` [7].
- [ ] If using editor plugins such as `godot-ai`: check whether they add
      autoloads, and make sure those don't ship in release exports.

⚖ **Physics engine.** Third-party physics (Rapier2D) changes behaviour,
determinism, and which docs apply. Choosing it on day one is cheap. Switching
mid-project means re-tuning every feel parameter, so decide during a prototype, not in production.

**Exit:** the exported build launches on your machine. ADR 0001 exists.

### E7. Claude Code configuration

- [ ] `CLAUDE.md` from [Appendix A](#appendix-a--draft-claudemd), with names and
      paths adjusted. It must contain the exact E5 commands, the single-test
      commands from E3 and E4, and the "fresh worktree: `--import` first" note. Stay under about 60 lines [15].
- [ ] `.claude/rules/godot-csharp.md` from [Appendix C](#appendix-c--draft-clauderulesgodot-csharpmd),
      scoped to `src/**` and `tests/**` [15].
- [ ] `.claude/settings.json` from [Appendix B](#appendix-b--draft-claudesettingsjson):
      allow the ladder scripts, ask for `git push`, a PreToolUse block on
      `.uid`, `.godot/`, and `addons/`, and a Stop hook running `check.sh --quiet` [15].
- [ ] Godot docs for grounding. Clone or download the docs for your *exact*
      engine version **outside the repo**. Use the `.rst` sources, since they
      grep better than HTML. Point to them from `CLAUDE.md`.
- [ ] Optional: `.mcp.json` with `godot-ai` pinned to an exact version, plus its
      editor plugin in `addons/`. Leave it off when working only in `core/` [15].
- [ ] Skills: `.claude/skills/shape/SKILL.md` (interview, then a pitch from the
      template) and `.claude/skills/slice/SKILL.md` (pitch, then Appendix D
      tickets). Two only. Add more when a procedure has repeated three times [15].
- [ ] `.worktreeinclude`: list any gitignored files a worktree needs. It's probably empty.

**Exit:**

- In a fresh session, `/context` shows `CLAUDE.md`. The rule file loads only after a `src/` file is opened.
- Asking Claude to "write a file `foo.cs.uid`" is blocked.
- Asking it to "add a misformatted line, then stop" gets bounced by the Stop hook.
- `claude -w smoke` makes a new worktree where `./scripts/verify.sh` passes with no manual steps.

### E8. Process scaffolding

- [ ] `docs/process/playbook.md`: this document.
- [ ] `docs/plan/_package-template.md`: the pitch template, copied from Pachi.
- [ ] `docs/plan/ideas.md`: an unranked idea pile. It's explicitly **not** a
      backlog or a commitment; Shape Up bets come from fresh shaping, not from
      grooming a list [23].
- [ ] `docs/decisions/0000-template.md`: Context / Decision / Consequences [30].
- [ ] `docs/notes/`: playtest and session notes, `YYYYMMDD_<topic>.md`.
- [ ] No separate `TODO.md`. Active work lives in the active pitch's tickets.

**Exit:** `/shape` produces a pitch file from the template in `docs/plan/`.

### E9. CI and dependency hygiene

- [ ] `.github/workflows/ci.yml` with two jobs:
  - **fast**: on every push. No Godot: format, build, `dotnet test core.tests`.
  - **full**: on PRs to `main`. Installs Godot Mono at the `.godot-version`
    version (for example, with a setup-godot action) and runs `verify.sh`
    headless. Uploads `out/logs/` as an artifact on failure [9].
- [ ] Branch protection on `main`: require both jobs. Allow no direct pushes,
      even for you; that makes "main is always green" real.
- [ ] Renovate or Dependabot for NuGet, grouped so Godot SDK packages and
      `.godot-version` move together [9].

⚖ **Branch protection for a solo dev.** It adds a PR step to every change.
The pay-off is that Claude-authored work can never reach `main` without both
gates and your merge. That's the "who has the last word" control from [15].
Skip it only if you'll reliably run `verify.sh` before every merge.

**Exit:** a PR with a failing core test is blocked from merging. Fixing it unblocks the PR.

### E10. Walking skeleton (first ticket, through the B4 loop)

Build the thinnest slice that touches every layer [32], **as a real ticket
using the B4 loop**. This tests the process as much as the code:

- [ ] Shape a one-paragraph pitch: "a ball drops into a pocket and the wallet increases."
- [ ] **core:** `Payout.For(pocket, ball)` plus `Wallet`, test-first.
- [ ] **engine:** a `Pocket` node calls core and emits a past-tense signal
      (`PaidOut`), with a GoDotTest that drops a ball and asserts the signal fired [1].
- [ ] **scene:** `src/main/main.tscn` with one pocket and a label bound to the wallet.
- [ ] **visual:** `capture.sh` with an input script that launches one ball, at a
      fixed seed. Claude reads the frames and confirms the label changes.
- [ ] **ship:** export the build (E6) and play it.
- [ ] Run B5's review checklist and B14's definition of done, then merge through CI.

**Exit:** you merged a ticket whose every step followed B4, and nothing about
the loop needed a workaround. If something did, fix the setup now. That's
the cheapest point at which it will ever be fixed.

### E11. Fresh-clone acceptance test

- [ ] On a different directory (ideally a different machine), `git clone`,
      follow **only** `README.md`, and get `check.sh` and `verify.sh` green.
- [ ] Time it, and note the time in the README. Anything you had to do that
      isn't in the README is a setup bug: fix the README or the scripts.
- [ ] Tag `skeleton/1`. This is the baseline every future cycle starts from.

**Exit:** a clean clone goes from nothing to a green gate using only the README.

## Appendix F — Sources

Sources marked † were read directly this session (local vendored docs or a
fetched page). The others are search-result summaries or prior knowledge and
are worth a skim before you rely on a specific number.

| # | Source | Grade | Used for |
|---|---|---|---|
| 1 | † Godot Docs, [Scene organization](https://docs.godotengine.org/en/stable/tutorials/best_practices/scene_organization.html) (local: `ref/godot-docs/_sources/tutorials/best_practices/`) | Official | Self-contained scenes, dependency injection, signals respond and methods start |
| 2 | † Godot Docs, [Autoloads versus regular nodes](https://docs.godotengine.org/en/stable/tutorials/best_practices/autoloads_versus_regular_nodes.html) | Official | Global state, global access, and global allocation problems; when autoloads are appropriate |
| 3 | † Godot Docs, [Project organization](https://docs.godotengine.org/en/stable/tutorials/best_practices/project_organization.html) | Official | snake_case files, PascalCase C#, `addons/` for third-party, `.gdignore` |
| 4 | † Godot Docs, [Version control systems](https://docs.godotengine.org/en/stable/tutorials/best_practices/version_control_systems.html) | Official | Ignore `.godot/`, LF line endings, Git LFS caveats |
| 5 | Godot Blog, [UID changes coming to Godot 4.4](https://godotengine.org/article/uid-changes-coming-to-godot-4-4/); community reports on UID churn ([Lemmy](https://lemmy.dormedas.com/post/967764)) | Official / Community | Commit `.uid` files; pull before opening the editor |
| 6 | † Godot Docs, [C# signals: disconnecting automatically](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) | Official | Lambda and custom-signal disconnect edge cases |
| 7 | † Godot Docs, [Command line tutorial](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html) | Official | `--headless`, `--import`, `--write-movie`, `--fixed-fps`, `--quit-after`, `--log-file` |
| 8 | † Godot Docs, TSCN file format (local: `ref/godot-docs/_sources/engine_details/file_formats/tscn.rst.txt`) | Official | Rules for text-editing scenes |
| 9 | † Chickensoft, [GodotGame template](https://github.com/chickensoft-games/GodotGame) | Practitioner | `global.json` pinning, GoDotTest in CI with software rendering, Renovate, coverage |
| 10 | Chickensoft, [GameDemo](https://github.com/chickensoft-games/GameDemo) | Practitioner | Logic/view separation (LogicBlocks), tree-based DI (AutoInject), testability |
| 11 | [gdUnit4Net](https://github.com/godot-gdunit-labs/gdUnit4Net) v5 release notes ([NuGet](https://www.nuget.org/packages/gdUnit4.api)) | Practitioner | Logic-only tests without the Godot runtime; `[RequireGodotRuntime]` |
| 12 | † Microsoft Learn, [Code analysis in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) | Official | `AnalysisLevel`/`AnalysisMode`, `EnforceCodeStyleInBuild`, pinning rule sets |
| 13 | [dotnet format vs CSharpier](https://levelup.gitconnected.com/dotnet-format-vs-csharpier-which-auto-formatting-tool-is-best-for-your-net-code-5376dbe9be20) | Community | Formatter trade-off. The playbook keeps `dotnet format`; consider CSharpier only if hook latency hurts |
| 14 | Godot Forum, [StringName/NodePath caching and GC hitches](https://forum.godotengine.org/t/stringname-nodepath-caching-and-garbage-collector-hitches/135268) | Community | Per-frame allocation and caching; profile first; the Godot profiler doesn't cover C# |
| 15 | † Anthropic, [Best practices for Claude Code](https://code.claude.com/docs/en/best-practices) | Official | Short `CLAUDE.md`, path rules, skills, hooks vs instructions, Stop hooks, spec-first fresh sessions, review "skip style", steering, evidence |
| 16 | † Anthropic, [Code intelligence plugins](https://code.claude.com/docs/en/plugins/code-intelligence) | Official | `csharp-lsp` plugin with `csharp-ls` binary |
| 17 | METR, [Early-2025 AI on experienced OSS developer productivity](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) (summary: [eWeek](https://eweek.com/news/news-ai-tools-slow-developer-productivity-study)); 2026 follow-up via [CoderCops](https://blog.codercops.com/blog/ai-coding-productivity-2x-not-10x-metr-2026) | Research | 19% slower vs about 20% perceived faster; follow-up flagged as unreliable by METR |
| 18 | Google, [2025 DORA report](https://blog.google/technology/developers/dora-report-2025/) ([announcement](https://cloud.google.com/blog/products/ai-machine-learning/announcing-the-2025-dora-report), [InfoQ](https://www.infoq.com/news/2025/09/dora-state-of-ai-in-dev-2025)) | Research | AI as amplifier; throughput up, stability concerns; fast feedback loops and loose coupling |
| 19 | GitClear 2025 AI code quality, via [DevClass](https://devclass.com/2025/02/20/ai-is-eroding-code-quality-states-new-in-depth-report/) and [The New Stack](https://thenewstack.io/whats-missing-with-ai-generated-code-refactoring) | Research (vendor) | Duplication up, refactoring/moved code down |
| 20 | Kent Beck, [Augmented Coding: Beyond the Vibes](https://kentbeck.com/summaries/augmented-coding-beyond-the-vibes/); [Pragmatic Engineer interview](https://newsletter.pragmaticengineer.com/p/tdd-ai-agents-and-coding-with-kent) | Practitioner | TDD as a "superpower"; agents delete tests; complexity drift; amplified skills |
| 21 | Simon Willison, [Vibe engineering](https://simonwillison.net/2025/Oct/7/vibe-engineering/) and the Agentic Engineering Patterns guide | Practitioner | Tests and QA amplify agents; red/green; start a session by running tests; exploration vs production |
| 22 | [AI mistakes in Godot 4 code](https://www.surmado.com/blog/ai-mistakes-in-godot-4-code); [DEV: 7 Godot 4 API calls](https://dev.to/ziva/7-godot-4-api-calls-your-ai-assistant-still-gets-wrong-3ep6) | Community (vendor) | Godot 3 vs 4 confusion patterns |
| 23 | † Ryan Singer, [Shape Up](https://basecamp.com/shapeup) | Practitioner | Appetite, pitches, betting, circuit breaker, scopes, hill charts, cool-down |
| 24 | Anthony Pecorella, [The Math of Idle Games](https://www.gamedeveloper.com/design/the-math-of-idle-games-part-i) (Parts I–III); [GDC Europe 2016 "Quest for Progress"](https://www.slideshare.net/slideshow/quest-for-progress-gdc-europe-2016/65405507) | Practitioner | Exponential cost and growth curves, prestige loops |
| 25 | Robert Masella, [Automated Testing of Gameplay Features in Sea of Thieves](https://gdcvault.com/play/1026366/Automated-Testing-of-Gameplay-Features) (GDC 2019) | Practitioner | Automated gameplay tests gating integration |
| 26 | Jakob Nielsen, [Why you only need to test with 5 users](https://www.nngroup.com/articles/why-you-only-need-to-test-with-5-users/) | Research | Small, frequent usability rounds |
| 27 | Playtest facilitation guides ([Bugnet](https://bugnet.io/blog/how-to-playtest-your-game-and-learn-from-it), [Wayline](https://www.wayline.io/blog/indie-dev-playtesting-usability-testing)) | Community | Silent facilitation, think-aloud |
| 28 | GameMaker, [Finish a Game by Any Means Possible: Derek Yu](https://gamemaker.io/en/blog/derek-yu-gm25) | Practitioner | Finishing as a skill; ship over endless polish |
| 29 | SmartBear/Cisco, [Best practices for peer code review](https://smartbear.com/learn/code-review/best-practices-for-peer-code-review/) | Research (vendor) | Review effectiveness drops beyond about 200–400 LOC |
| 30 | Michael Nygard, [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) | Practitioner | ADR format |
| 31 | Martin Jonasson & Petri Purho, *Juice it or lose it* (GDC Europe 2012 talk) | Practitioner | Juice amplifies a proven mechanic |
| 32 | Alistair Cockburn, "walking skeleton", *Crystal Clear* (2004) | Practitioner | Thinnest end-to-end slice through every layer before building features |
| 33 | † Caleb Leak, [I Taught My Dog to Vibe Code Games](https://calebleak.com/posts/dog-game) (Godot 4.6, C#, Claude Code) | Practitioner | Feedback loops as the main lever; screenshot and input-sequencer tools; scene linter; agents get `project.godot` input maps wrong; memory locking in one style; Bevy and Unity rejected |
| 34 | † ABA Games, [Godot is well-suited for game development with AI coding agents](https://dev.to/abagames/godot-is-well-suited-for-game-development-with-ai-coding-agents-2lcm) | Practitioner | Debug-collision screenshot fixed a bug text couldn't; headless `SceneTree` tests; `XDG_*` redirect in sandboxes |
| 35 | † htdt, [Godogen](https://github.com/htdt/godogen) (summary: [Gigazine](https://gigazine.net/gsc_news/en/20260317-godogen/)) | Practitioner | Judge the running game, not the compile; screenshot QA; custom Godot API reference; C# type-checking before running |
| 36 | † Mr Phil Games, [Why I'm switching Stellar Throne to Godot](https://mrphilgames.substack.com/p/why-im-switching-stellar-throne-to) | Community | Typed logic scripts outside scenes worked; deep scene trees and implicit node paths didn't |
| 37 | † Lucca Sanwald, [Building an RTS in Godot: what if Claude writes all code?](https://dev.to/datadeer/part-1-building-an-rts-in-godot-what-if-claude-writes-all-code-49f9) | Community | Agents copy nearby code; design docs as ground truth; abandoned after losing touch with the code |
| 38 | † maxxmini, [I built a full game in one day using AI agents](https://dev.to/maxxmini/i-built-a-full-game-in-one-day-using-ai-agents-heres-what-happened-3c3o) | Community | Multi-agent roles (not adopted); thin cross-system tests; interface drift |
| 39 | † Ed Orellana, [AI playtesting for Godot](https://dev.to/orellanaed/ai-playtesting-for-godot-an-agent-can-check-that-your-game-works-not-that-it-is-fun-58gd) | Community (vendor) | Fixed timing and seeds for repeatability; timed input; replaying human play as tests; agents only test what they think of |
