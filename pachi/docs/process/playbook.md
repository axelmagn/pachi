# Pachi Development Playbook

How Pachi is set up, and how each feature goes from idea to shipped. It is
written for the human developer. It decides where the LLM fits; it is not the
LLM's instruction file (that's `CLAUDE.md`, drafted in [Appendix A](#appendix-a--draft-claudemd)).

Over budget: the reference for the whole process (setup, SOPs, appendices,
sources); each section is read on its own via links.

- **Last reviewed:** 2026-11-02
- **Applies to:** Godot 4.7.2 Mono, .NET 8 target (SDK 10 installed), Rapier2D, Claude Code,
  Trello ("Pachi Agent" board, through the Trello MCP server)
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
   [fresh-project setup](fresh-project-setup.md) ·
   [sources](#appendix-f--sources)

---

## 0. How to use this playbook

- **Part A** is a checklist you work through once, in order. Every step has an
  exit criterion. Don't start feature work with an LLM until A1 through A6 are done.
  Part A *adapts* the existing repo. [the fresh-project setup checklist](fresh-project-setup.md)
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
| Process assets | `docs/plan/_package-template.md` is a strong pitch template (Shape Up mixed with game-design practice). `docs/plan/202610-beetles.md` uses it. `docs/notes/` holds playtest notes. | Keep the template; it's the foundation for Part B. It and the active pitch move to Trello (A6.8). |
| Tooling | The `godot-ai` MCP server (v4.3.0) is configured and its editor plugin is enabled. It adds an autoload, `_mcp_game_helper`, to `project.godot`. `ref/godot-docs/_sources/` has the 4.7 docs as greppable `.rst.txt`. | Good grounding assets. Check that the MCP autoload is inert or stripped in release exports (A2). |
| Determinism | `Ball.cs` draws launch spread from the global, unseeded `GD.RandRange`. `GameConfig.Rng` is an injectable `System.Random`, and `AutoFireSamplerTests` already seeds one. | Two runs of the same scene differ, so captures and engine tests can't be compared before and after a change. |

---

## 2. Why the last round of LLM work went wrong

You described the earlier output as "crufty and not aligned with the features I
wanted." The evidence gives a consistent explanation, and each cause has a
countermeasure built into this playbook.

| Cause | Evidence | Countermeasure |
|---|---|---|
| **No written definition of done.** The agent builds the most plausible thing, not your thing. | "Claude built something different from what you had in mind... It's missing information it can't get from reading the repo." [15] | Pitch card, then ticket card with acceptance criteria, then a fresh session builds from the card (B2–B4). |
| **No check it could run.** It stops when the work *looks* done. | "If it has nothing to run that tells it whether the work actually *is* done, 'looks done' is all it has to go on." [15] | Fast, always-green verification ladder, enforced by a Stop hook (A3, A6). |
| **Over-generation.** Agents add unrequested features, fallbacks, and "robustness." | Beck: genies drift "into complexity that collapses under its own weight" [20]. Willison: tests reduce over-generation [21]. GitClear: duplicated code is up, refactoring is down, in AI-era codebases [19]. | No-gos in every ticket. Review diffs for *additions you didn't ask for*. A cool-down deletion pass (B5, B12). |
| **Reviewers always find something.** Fixing every review nit adds layers. | "A reviewer that's asked to find problems will always find *something*. If you fix every finding, you end up with extra abstraction layers." [15] | Review against the spec and skip style (B5). |
| **Stale engine knowledge.** Models confuse Godot 3 and 4 APIs. | Community reports of Godot 3 idioms in Godot 4 code [22]. Godogen ships its own reference for 850+ Godot classes because models haven't mastered the API [35]. Leak dropped Bevy partly for thin training data [33]; Rapier2D is Pachi's thin-data dependency. Official C# signal semantics have non-obvious edge cases [6]. | Ground the agent in the local 4.7 docs and the Rapier2D addon's own source. Path-scoped Godot rules (Appendix C). |
| **Feeling faster than you are.** | METR's 2025 RCT: experienced developers were 19% *slower* with AI on familiar codebases, yet believed they were about 20% faster [17]. DORA 2025: AI "amplifies" whatever practices a team already has. Throughput rose, but stability suffered where foundations were weak [18]. | Measure per cycle (B12). Invest in foundations (Part A) before throughput. |

---

## 3. Operating principles

1. **Foundations first, then speed.** AI amplifies what's already there, good or
   bad [18]. Part A is the work that makes it amplify the good.
2. **Every change has a check that comes back pass or fail.** If you can't name the check, the ticket isn't ready [15].
3. **Logic lives where it's cheap to test.** Game rules go in plain C# with
   millisecond-scale tests. Nodes stay thin and wire the rules to the engine
   [9][10][11]. Agents edit typed logic outside scenes confidently, and stumble
   on deep scene trees and implicit node paths [36].
4. **You own the *what*, the LLM drafts the *how*, and you review the diff.**
   Beck's framing: augmented coding de-emphasises language expertise and
   amplifies "vision, strategy, task breakdown, and feedback loops" [20]. Those
   last four are your job.
   Trello is the shared channel: Claude may create, edit, comment on, and move
   cards, but a card's acceptance criteria, appetite, and no-gos change only
   when you ask, and every such change gets a comment.
5. **Two modes, never mixed.** *Exploration* (prototypes, toys, spikes) can be
   vibe-coded and is thrown away. *Production* (merged code) goes through spec,
   tests, and review [21]. Prototype code never gets merged.
6. **Small batches.** One ticket per session and one reviewable diff, about 400 changed lines or fewer [29][18].
   Multi-agent role pipelines (PM, architect, QA agents) are deliberately not
   adopted. The one first-hand Godot report is a self-reported one-day
   prototype whose failures were interface drift and thin cross-system tests [38].
7. **Deterministic for musts, instructions for shoulds.** If a rule has zero
   exceptions, make it a hook or deny rule. Everything else is a short line in
   `CLAUDE.md` [15].
8. **Write down only what the repo can't tell the agent.** Delete instructions
   that the code or a tool already enforces [15].
9. **Scenes are self-contained. Dependencies flow down, events flow up.**
   Parents call children. Children emit signals. Siblings never reach for each
   other. Use autoloads only for broad systems that manage their own data
   [1][2].
10. **Finish things.** Use fixed appetites with a circuit breaker [23]. Shipping rough beats polishing forever [28].

### The state seam: repo vs. Trello

Project-management state lives in Trello. Project state lives in the repo.
The test for any piece of state: **does it still need to be true after the
card is archived?** If yes, it goes in the repo.

| Kind of state | Where it lives | Examples |
|---|---|---|
| The product and its permanent rules | Repo | Code, tests, input scripts, `CLAUDE.md`, rule files, ADRs, this playbook |
| Design facts that outlive a cycle | Repo, `docs/design/` | The core loop, economy targets ("first prestige at 12–20 min"), glossary. Promoted from a pitch once settled. |
| The plan and the work | Card description | Pitch, ticket, acceptance criteria, no-gos, Check line, files and pattern to follow |
| The conversation | Card comments | Your context, Claude's plan, questions, why the spec changed, evidence (command and output tail), test-change justifications |
| Progress | List, labels, checklists | Status, layer, hill position, size, the B14 checklist |
| Evidence and source material | Card attachments | Capture frames, clips, logs, playtest notes and recordings, reference images, long spike write-ups |
| Scratch | `out/` (gitignored) | Capture output before it's attached, sim runs |

Two rules hold the seam:

1. **The description is the current spec; comments are its history.** Trello
   doesn't version descriptions, so whoever edits one (you or Claude) posts a
   comment saying what changed and why.
2. **Nothing in the repo depends on Trello to make sense.** Code, comments, and
   ADRs never say "see card X". Commit messages may carry a card link, as a
   pointer into history. A decision made in a card's comments that a future
   agent will need becomes an ADR before the card closes.

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

1. Tag the legacy state: `git tag legacy/cards-v1` (TODO.md says this was done,
   but no tag exists locally. Check `git ls-remote --tags origin` first).
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
  debug-only helper ([E4](fresh-project-setup.md#e4-engine-test-harness)).
  Claude can then launch balls, buy a card, or hover a
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

**Exit:** `dotnet test core.tests` runs without Godot installed and finishes in
seconds. One real rule (for example, pocket input matching) lives there with
tests.

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
     Ask for `git push`. Allow Trello reads and card, comment, and checklist
     writes; ask for list and board writes (Appendix B).
4. **Consolidate config.** The repo has `pachi/.claude/settings.local.json` at
   the git root and `.mcp.json` in `pachi/pachi/`. Start sessions from
   `pachi/pachi/` and keep all Claude config there. Confirm with `/context` and `/mcp`.
5. **MCP hygiene.** The `godot-ai` server is useful for live inspection (editor
   screenshots, running scenes, reading the scene tree). Connected servers cost
   context even when idle [15], so disable it in sessions that only touch `core/`.
   Prefer scripts over MCP for anything you want repeatable or in CI. The
   exception is `project.godot` settings, which go through the editor or MCP (B6).
6. **Skills: start with three, add more only when a procedure repeats.**
   - `/shape`: interview you, then create a pitch card from the board's pitch template card (B2).
   - `/slice`: break a pitch card's scopes into ticket cards in the Appendix D format, each with the B14 checklist (B3).
   - `/ticket <card>`: load a card's description, its latest comments in full
     (older ones summarised), its attachments list, and its linked pitch, then
     run `./scripts/check.sh` (B4 orient).
   Don't reinstall the archived skill pack wholesale. Write down only what
   Claude can't infer [15].
7. **Worktrees for parallel sessions.** Add a `.worktreeinclude` if anything
   gitignored is needed. Every new worktree must run `--import` (A3) [15].
8. **Set up the Trello board** ("Pachi Agent") and connect the Trello MCP server.
   - **Lists, left to right:**
     - Inbox (untriaged playtest findings)
     - Ideas (not commitments)
     - Pitches (shaped, awaiting a bet)
     - Setup (Part A cards; archive once A8 is done)
     - Cycle: `<slug>` (the one active bet's tickets; rename each cycle)
     - Spikes
     - In progress (WIP limit 1)
     - Review / Play
     - Done (this cycle)
     - Cool-down (recurring B12 cards)
   - **Labels:**
     - Layer: `core`, `engine`, `visual`, `you`
     - Hill: `⬆ uphill`, `⬇ downhill`
     - Size: `S`, `M`
     - Type: `bug`, `spike`, `setup`, `juice`, `proto`
   - **Template cards:** a pitch template (the content of `docs/plan/_package-template.md`)
     and a ticket template (Appendix D, with the B14 checklist).
   - **Migrate:**
     - `docs/plan/202610-beetles.md` becomes a pitch card with linked ticket cards.
     - `docs/notes/` playtest notes become attachments on Inbox or pitch cards.
     - Settled design facts from either move to `docs/design/`.
     - `TODO.md` items become cards.
     - Then delete `docs/plan/`, `docs/notes/`, and `TODO.md`. Git history is the archive.

**Exit:**
- In a fresh session, ask Claude to "introduce a formatting error and stop". The Stop hook bounces it.
- `/context` shows `CLAUDE.md` loaded, and the rule file loads only when Claude opens a `src/` file.
- `/ticket <card>` prints the card's spec, and Claude can post a comment on it.

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
- [ ] A3 three-rung ladder with scene lint, fixed seed, and input scripts; scene
      lint, log scan, and all-scenes test proven by breaking something
- [ ] A4 `core/` and `core.tests/` exist with at least one real rule
- [ ] A6 `CLAUDE.md`, rule file, `settings.json` hooks working; `CODE_STYLE.md` trimmed
- [ ] A6.8 Trello board set up with lists, labels, and template cards;
      `docs/plan/`, `docs/notes/`, and `TODO.md` migrated and removed
- [ ] A7 CI red/green proven
- [ ] Old `README.md` "Getting Started" updated to point at `scripts/check.sh`

---

## Part B — Standard operating procedures

### B1. The cycle

This is Shape Up [23] scaled down for one person. Your pitch template already
uses its vocabulary. Every stage below happens on the Trello board; the lists
in A6.8 follow this cycle left to right.

```
 ┌── Shape (cool-down) ──┐  ┌──────── Build cycle (appetite: 1–3 wks) ────────┐  ┌─ Cool-down (3–5 days) ─┐
 │ playtest notes → pitch │→ │ slice → tickets → build loop (B4) per ticket   │→ │ playtest (B9)          │
 │ bet: pick ONE pitch    │  │ hill-chart the scopes; cut nice-to-haves early │  │ refactor/delete (B12)  │
 └────────────────────────┘  └────────────────────────────────────────────────┘  │ update playbook/rules  │
                                                                                  │ shape next pitch       │
                                                                                  └────────────────────────┘
```

- **One bet at a time.** The appetite is fixed and the scope is variable.
- **Circuit breaker:** the pitch card's due date is the end of the appetite.
  When it arrives, stop. In cool-down, decide whether to re-shape or drop.
  Never extend by default [23].
- **Hill chart:** label each ticket card `⬆ uphill` (unknowns remain) or
  `⬇ downhill` (execution only), and move the label as it changes. Ask an LLM
  for help on uphill scopes as a *spike* (B8), not as production code.

### B2. Shaping a pitch

1. Start from evidence: a playtest note, a clip, a feeling you had while playing.
   Usually that's a card in Inbox, or a Pitches card you've been adding to.
2. Run `/shape` (or ask Claude to "interview me until you could write the pitch").
   Answer its questions. It creates a pitch card in Pitches from the template
   card, attaching or linking the evidence it started from [15].
3. You edit the result. The **experience goal**, **appetite**, and **no-gos**
   must be in your words. They're how you'll judge the LLM's work later.
4. Fill in **Rabbit holes**. Each technical risk becomes a spike card that
   runs *before* production tickets.
5. Record any architectural decision as a short ADR in `docs/decisions/NNNN-title.md`
   (context, decision, consequences) [30]. These are exactly the decisions
   that aren't in the code, which agents can't see [15]. This matters more
   with Trello: a decision left in a card's comments is invisible to the code.
6. **Bet:** move the pitch card to the cycle list (renamed `Cycle: <slug>`) and set its due date.

### B3. Slicing into tickets

A ticket is a Trello card and the unit of one Claude session. Run `/slice` on
the pitch card, then edit the cards it creates.

- **Vertical:** each ticket yields something you can see or test in the game.
  Avoid tickets like "add models" followed by "add UI".
- **Small:** one session, at most about 400 changed lines [29]. If Claude's plan
  for a ticket touches more than about 6 files, split it.
- **Format:** [Appendix D](#appendix-d--ticket-template) is the card description.
  It has acceptance criteria, explicit no-gos, a named check, and the layer
  each criterion is tested at (core, engine, visual, or you). Labels mirror
  layer, hill, and size. The B14 checklist is attached to every card.
- **Linking:** each ticket card links to its pitch card, and the pitch card
  holds a checklist of its ticket cards. `CLAUDE.md` points to one place: the board.

### B4. Building a ticket with Claude (the core loop)

```
fresh session → orient → plan → (approve) → red → green → verify → self-review → you review & play → commit
```

1. **Fresh session per ticket.** Use `/clear` or a new session, so a clean
   context starts from the written ticket [15].
2. **Orient.** Run `/ticket <card>`, or write a prompt that names the card and
   the files to look at. For example:
   > Implement card `beetles#3` on the Pachi Agent board. Read its description
   > and comments. Follow the pattern in `src/pockets/Pocket.cs`. Start by
   > running `./scripts/check.sh`. Stay inside its no-gos.

   Starting by running the tests is a cheap way to orient the agent [21].
   Claude moves the card to In progress.
3. **Plan.** For anything beyond a trivial change, have Claude propose a plan
   first (plan mode). Reject plans that add abstractions, singletons, or
   fallbacks the ticket didn't ask for. Claude posts the approved plan as a
   card comment, so the review in B5 has something to compare against.
4. **Red.** Core-layer criteria get a failing xUnit test first. Engine-layer
   criteria get a failing GoDotTest. Confirm it fails for the right reason [20][21].
5. **Green.** Implement the minimum. Claude iterates against `check.sh`, and the
   Stop hook enforces it.
6. **Verify.** Claude runs `verify.sh`. For visual or interaction tickets, it
   runs `capture.sh`, with an input script when a criterion involves
   interaction, and looks at the frames. For physics bugs, add
   `--debug-collisions` and show it the frames instead of describing the bug [34].
   Ask for **evidence**: the command and the tail of its output, not "all tests pass" [15].
   Claude posts the evidence as a card comment and attaches the frames that matter.
7. **Self-review in a fresh context.** Run `/code-review`, or use the
   spec-review prompt in B5. Fix only correctness gaps.
   Claude then moves the card to Review / Play.
8. **You review and play** (B5). Feel changes require you to run the game. Use `/verify` or F5.
   Put feedback in card comments, so the next session sees it too.
9. **Commit** (B13). Move the card to Done and tick it on the pitch card's checklist.

**Steering:** watch the first minute and press `Esc` the moment it drifts.
After two failed corrections, `/clear` and rewrite the prompt instead of
trying a third time. `/rewind` makes trying a bolder approach cheap [15].
**Effort:** medium for routine tickets, high for bugs in code you didn't write
or changes that ripple across many callers [15].

### B5. Reviewing LLM output

Review against the ticket, not against perfection.

**Spec-review prompt** (run in a fresh subagent) [15]:
> Use a subagent to review the current diff against Trello card `<id>`: its
> description and the plan comment I approved. Check every acceptance
> criterion is implemented and tested at the stated layer and every no-go is
> respected. Report gaps that affect correctness. Skip style.

**Your review checklist, in this order:**

0. **Spec drift.** Did the card's acceptance criteria or no-gos change since you
   approved the plan, without a comment saying you asked for it? An agent
   quietly rewording its own spec is the main way this setup fails.
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
  change in the editor (as
  [E6](fresh-project-setup.md#e6-godot-project-baseline-recorded-as-adr-0001)
  does), or have Claude use the `godot-ai` tools
  (`input_map_manage`, `autoload_manage`, `project_manage`), which write
  through the editor. Keep such changes in their own commits.

### B7. Bugs

1. Reproduce. Create a `bug` card with the observed and expected behaviour.
   If input can reproduce it, the input script goes in the repo
   (`tests/input/`), because it becomes a regression check; the card links to it.
2. **Failing test first,** at the lowest layer that can show the bug (core, then engine) [15].
3. Fix and verify. Keep the test as a regression test.
4. If the bug was caused by a misleading pattern, add a line to `CLAUDE.md` or
   the rule file. This is how "corrected twice" gets institutionalised [15].

### B8. Prototypes and spikes  *(exploration mode)*

Use these for uphill scopes and "is this fun?" questions.

- Branch `proto/<slug>` or a folder `prototypes/<slug>/` with `.gdignore` removed
  only on that branch. **Vibe coding is fine here** [21]: fast, ugly, greybox,
  temp audio.
- The output is *knowledge*: a clip or write-up attached to the spike card, a
  comment on the pitch card, a decision (an ADR if it's durable). The
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
- **Afterwards:** raw notes and recordings go on a card in Inbox, named
  `Playtest YYYYMMDD <name>`, as an attachment or the description. Within a
  day, triage each finding into its own card:
  - a bug becomes a `bug` card in the cycle list now. If input can reproduce
    it, the card starts with an input script that shows it (A3), and the
    script stays in the repo as a regression check [39]
  - a legibility or friction issue becomes a card in Pitches, as a candidate for the next pitch
  - an idea becomes a card in Ideas and doesn't count as a commitment
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
   `CLAUDE.md` now covers. Also scan the cycle's card comments for corrections
   you made more than once, and promote them to `CLAUDE.md` or a rule.
4. **Promote durable state.** Read the pitch card and its tickets' comments.
   Settled design facts go to `docs/design/`, and decisions go to ADRs. After
   this, archiving the cards loses nothing the repo needs.
5. **Measure.** Write three lines in a comment on the pitch card: tickets
   shipped vs planned (cards in Done vs. still in the cycle list), tickets
   that needed more than two correction rounds (count correction comments),
   and whether LLM sessions felt faster *and* were faster. METR shows those
   two can disagree [17].
6. **Bump pinned versions** (`AnalysisLevel`, NuGet, Godot patch) on purpose, then re-run the full gate.
7. Shape and bet the next pitch (B2).
8. Tag `cycle/YYYYMM-<slug>-end`. Archive the cycle's Done cards and the pitch card.

### B13. Git conventions

- `main` is always green. Each feature gets a branch (`f-<slug>`), and a ticket
  is one or a few commits.
- Commit messages: `<area>: <imperative summary>`, with the card id and
  short link in the body. Claude adds its co-author trailer.
- Commit `.uid` files along with their sources. `.godot/` stays ignored [4][5].
- Squash-merge feature branches, so each `main` commit corresponds to a reviewed ticket.
- Claude may commit on a feature branch when asked. **`git push` and merges to `main` are yours.**

### B14. Definition of done (per ticket)

- [ ] Every acceptance criterion is met and tested at its stated layer
- [ ] No-gos respected; no unrequested additions (B5 #2)
- [ ] `./scripts/verify.sh` green, and you saw its output (not just a claim)
- [ ] Visual or feel tickets: frames captured or the game played by you
- [ ] Test changes justified in a card comment (A5 integrity rule)
- [ ] Card has an evidence comment and is in Review / Play; no unexplained spec edits (B5 #0)
- [ ] Durable decisions recorded as ADRs or in `docs/design/`, not only in
      comments; `CLAUDE.md` updated if you corrected something twice

### B15. Writing docs with Claude

Claude's docs run long by default. Adding text costs it nothing, leaving
something out feels like failure, and its research ends up in the deliverable.
Asking it to "be concise" mostly rewords the same text. To get real cuts,
give it a budget and tell it to delete.

- **Reader line first.** Every doc opens with `For: <reader>. Lets them: <decision/task>.`
  Anything that doesn't serve that line gets cut.
- **Budget in lines, hard-wrapped at ~80 columns.** Lines are the unit Claude
  sees (Read output numbers them) and the one a hook can check exactly. Defaults:

  | Doc type | Budget |
  |---|---|
  | Ticket, ADR, card comment | ≤ 40 |
  | How-to, SOP, `CLAUDE.md` | ≤ 80 |
  | Design doc, pitch | ≤ 150 |

  To go over, the doc states why in one line, or you say so in the request.
- **Keep research out of the doc.** Notes, sources and alternatives considered
  go in `docs/research/<slug>.md`, and the doc links to them.
- **Cutting pass for anything over 80 lines.** A fresh subagent reads the doc
  as the named reader and lists what to delete, what's repeated, and what
  could be a link. Apply the deletions only; don't reword.
- **Hook enforces it.** A `PostToolUse` hook on `Write|Edit` runs
  `scripts/doc-budget.sh`. That script runs `wc -l` on `docs/**/*.md`, flags
  lines over 120 columns, and exits 2 with a warning so Claude sees it and
  trims. The rule above also goes into `CLAUDE.md` (Appendix A).

---

## Appendix A — Draft `CLAUDE.md`

This is a starting point. Keep it under about 60 lines and prune it every cool-down [15].

```markdown
# Pachi

Incremental pachinko deckbuilder. Godot 4.7.2 Mono, C# (.NET 8), Rapier2D physics.
Process: docs/process/playbook.md (B4 is the per-ticket loop; §3 "state seam" says what goes where).

## Work tracking (Trello board "Pachi Agent", via the Trello MCP)
- Every ticket is a card. Read its description, comments, and linked pitch card before starting (/ticket <card>).
- The description is the spec. Never change acceptance criteria, appetite, or no-gos unless asked; when you edit a description, comment what changed and why.
- Post your approved plan, then your evidence (command + output tail, key frames attached), as comments. Move the card: In progress → Review / Play.
- Don't archive or delete cards, or change lists or labels, unless asked.
- Anything that must outlive the card (decisions, design facts) goes in docs/decisions/ or docs/design/, never only in a comment. Repo files never reference cards.

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
- Decisions → docs/decisions/. Settled design → docs/design/. Plans, tickets, playtest notes → Trello.

## Conventions the code won't tell you
- Required node refs: [Export] assigned in the scene + Debug.Assert in _Ready. No GetNode fallbacks or auto-resolution.
- Allowed globals: GameConfig, GlobalEvents. Don't add autoloads or static Instance singletons.
- [Tool] only when the script must run or draw in the editor.
- Never write .uid files or uid:// values; Godot generates them.
- Input actions, physics layers, autoloads: change them via the editor or godot-ai tools, never by editing project.godot text.
- No commented-out code; git history is the archive.
- Don't add options, fallbacks, or abstractions the ticket didn't ask for. If you think one is needed, say so instead.
- Changing a test and the code it tests in one diff needs a one-line reason in a card comment.
- Docs: open with "For: <reader>. Lets them: <task>." Hard-wrap at 80. Budget ≤40 lines (ticket/ADR), ≤80 (how-to), ≤150 (design); exceed only with a stated reason. Research goes in docs/research/, linked.

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
      "Bash(godot-mono --headless*)",
      "mcp__trello__trelloRead*",
      "mcp__trello__trelloSearch",
      "mcp__trello__trelloWriteCard",
      "mcp__trello__trelloWriteChecklist"
    ],
    "ask": [
      "Bash(git push*)",
      "mcp__trello__trelloWriteList",
      "mcp__trello__trelloWriteBoard"
    ],
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
- Trello permissions are per tool, not per action. `trelloWriteCard` covers
  creating, editing, commenting, moving, *and* archiving cards, so "don't
  archive or delete unless asked" is a `CLAUDE.md` instruction, not a rule
  the harness enforces. If Claude archives a card without being asked twice,
  move `trelloWriteCard` to `ask`. Check the exact tool names with `/mcp`;
  check whether your Claude Code version matches the `*` wildcard on MCP
  names, and list the read tools individually if it doesn't.
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

This is the ticket template card's description. The card title is
`<pitch-slug>#<n> — <verb phrase, player-visible>`. Layer, hill, and size are
also labels. The B14 checklist is attached as a card checklist.
Conversation, the plan, evidence, and test-change justifications go in
comments, not in the description.

```markdown
**Pitch:** <link to pitch card>
**Scope:** <scope name> (⬆ uphill | ⬇ downhill)  **Size:** S | M  (L → split it)

**Acceptance criteria** (layer in brackets: core | engine | visual | you)
- [ ] [core]   Given a beetle needing {red, red}, catching red then red pays out its outputs once
- [ ] [engine] beetle.tscn emits PaidOut with the output list when its catch hole receives matching balls
- [ ] [visual] Hovering a beetle opens its wings within 0.2 s; outputs are legible at 960×540
- [ ] [you]    Opening wings feels responsive, not floaty

**No-gos:** no dynamic placement; no new autoloads; don't touch launcher/
**Pattern to follow:** src/pockets/PocketArmsController.cs (tween usage)
**Check:** ./scripts/verify.sh + capture.sh res://src/pockets/beetle_blue.tscn 120
**Repo links:** <input scripts, ADRs, design docs this ticket relies on>
```

## Appendix E — Fresh-project setup checklist

The fresh-project setup checklist now lives in its own document:
[Fresh-Project Setup Checklist](fresh-project-setup.md). It describes the
ideal setup for a brand-new project following this playbook; Part A adapts
the existing repo.

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
