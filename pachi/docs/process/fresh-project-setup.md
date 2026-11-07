# Fresh-Project Setup Checklist

> Companion to the [Pachi Development Playbook](playbook.md), formerly its
> Appendix E. Section references (Part A, B4, §1, …) and numbered citations
> ([Sources](playbook.md#appendix-f--sources)) point into the playbook.

This describes the **ideal setup for a brand-new project** that follows this
playbook. It is not adapted to Pachi's current repo; [Part A](playbook.md#part-a--one-time-setup) handles that.
Use it as the reference point when deciding how far to move the existing
project toward it.

Over budget: a step-by-step checklist (E0–E11) worked through once, with
each step's exit check inline.

**How to read it.**

- Steps are in dependency order. Each step is roughly ticket-sized and ends with
  an **Exit** check, so you can hand a step to Claude as a ticket once E5 exists.
- **⚖ Judgement call** marks a real trade-off. That's where you're most likely to
  deviate, and the note says what you'd give up.
- `<Game>` stands for the project or assembly name.

**Why this order:**

- The gate exists before any game code, so no code is ever written without a check (principle 2).
- Claude is configured only after the scripts exist, because `CLAUDE.md` is mostly those commands.
- A walking skeleton comes last. It proves every layer and the whole
  [B4](playbook.md#b4-building-a-ticket-with-claude-the-core-loop) loop
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
  docs/          .gdignore      # process/, design/, decisions/ (plans, tickets, notes live in Trello)
  out/                          # gitignored: logs, captures, coverage
```

⚖ **Single root vs. nested project.** With a single root, the Godot project,
the dotnet solution, Claude's config, and CI all start in the same directory.
That's the source of the split-root friction in [§1](playbook.md#1-where-the-project-is-today). The alternative is
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
- [ ] A Trello account and the Trello MCP server connected to Claude Code.

**Exit:** `godot-mono --version`, `dotnet --list-sdks`, `git lfs version`,
`jq --version`, and `which csharp-ls` all succeed. `/mcp` in Claude Code shows
the Trello server connected.

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
  package versions.

**Exit:** `dotnet build <Game>.sln` succeeds with 0 warnings. Adding an unused
local variable to a script fails the build.

### E3. Core library and fast tests

- [ ] `dotnet new classlib -f net8.0 -n <Game>.Core -o core`
- [ ] `dotnet new xunit -f net8.0 -n <Game>.Core.Tests -o core.tests`, then add
      Shouldly and a `ProjectReference` to core.
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
| `scripts/check.sh` | Runs `dotnet format --verify-no-changes`, then the scene lint on changed `.tscn`/`.tres` files ([A3](playbook.md#a3-build-the-verification-ladder)), then `dotnet build`, then `dotnet test core.tests`. `--fix` formats instead of verifying. `--quiet` exits 0 immediately if `git status --porcelain` shows no `*.cs`, `*.csproj`, `*.tscn`, or `*.tres` changes; that's for the Stop hook. Fails if `.godot-version` doesn't match. Target: under 30 s. |
| `scripts/verify.sh` | Runs `check.sh`. If `.godot/` is missing, runs `godot-mono --headless --import` [7]. Runs the engine tests with `--log-file out/logs/test.log`. **Fails if Godot is missing** (never skips). **Fails if the log contains** `SCRIPT ERROR`, `ERROR:`, or an unhandled exception, unless the line matches `scripts/log-allowlist.txt`. Prints a one-screen summary with the log path. |
| `scripts/capture.sh <scene> <frames> [--input <file>] [--seed <n>] [--debug-collisions]` | Runs the scene with `--write-movie out/capture/<name>/frame.png --fixed-fps 60 --quit-after <frames>` [7] at a fixed seed (default when `--seed` is absent), replays `--input` through the E4 helper, passes `--debug-collisions` through, and prints the frame directory. Not headless: movie capture needs a real renderer. |

- [ ] Every script `cd`s to the repo root itself and uses `set -euo pipefail`. Exit codes are the interface.
- [ ] If Godot's default data, config, or cache directories aren't writable
      (sandbox, CI), the scripts point the `XDG_*` variables at `out/`
      ([A3](playbook.md#a3-build-the-verification-ladder)) [34].
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

- [ ] `CLAUDE.md` from [Appendix A](playbook.md#appendix-a--draft-claudemd), with names and
      paths adjusted. It must contain the exact E5 commands, the single-test
      commands from E3 and E4, and the "fresh worktree: `--import` first" note. Stay under about 60 lines [15].
- [ ] `.claude/rules/godot-csharp.md` from [Appendix C](playbook.md#appendix-c--draft-clauderulesgodot-csharpmd),
      scoped to `src/**` and `tests/**` [15].
- [ ] `.claude/settings.json` from [Appendix B](playbook.md#appendix-b--draft-claudesettingsjson):
      allow the ladder scripts and Trello card writes, ask for `git push` and
      Trello list and board writes, a PreToolUse block on `.uid`, `.godot/`,
      and `addons/`, and a Stop hook running `check.sh --quiet` [15].
- [ ] `scripts/doc-budget.sh` and a `PostToolUse` hook on `Write|Edit` that
      runs it ([B15](playbook.md#b15-writing-docs-with-claude)).
- [ ] Godot docs for grounding. Clone or download the docs for your *exact*
      engine version **outside the repo**. Use the `.rst` sources, since they
      grep better than HTML. Point to them from `CLAUDE.md`.
- [ ] Optional: `.mcp.json` with `godot-ai` pinned to an exact version, plus its
      editor plugin in `addons/`. Leave it off when working only in `core/` [15].
- [ ] Skills:
  - `.claude/skills/shape/SKILL.md`: interview, then a pitch card from the board's template card.
  - `.claude/skills/slice/SKILL.md`: pitch card, then [Appendix
    D](playbook.md#appendix-d--ticket-template) ticket cards with the B14
    checklist.
  - `.claude/skills/ticket/SKILL.md`: load a card's spec, comments, and linked pitch, then run `check.sh`.

  Three only. Add more when a procedure has repeated three times [15].
  The skills need the board from E8, so write them after it exists.
- [ ] `.worktreeinclude`: list any gitignored files a worktree needs. It's probably empty.

**Exit:**

- In a fresh session, `/context` shows `CLAUDE.md`. The rule file loads only after a `src/` file is opened.
- Asking Claude to "write a file `foo.cs.uid`" is blocked.
- Asking it to "add a misformatted line, then stop" gets bounced by the Stop hook.
- Asking it to append 50 lines to an ADR gets a doc-budget warning.
- `claude -w smoke` makes a new worktree where `./scripts/verify.sh` passes with no manual steps.

### E8. Process scaffolding

The repo holds only state that outlives a card; work state lives on the
Trello board (the playbook's §3 "state seam").

- [ ] `docs/process/playbook.md`: the playbook.
- [ ] `docs/decisions/0000-template.md`: Context / Decision / Consequences [30].
- [ ] `docs/design/`: settled design facts promoted from pitches (core loop,
      economy targets, glossary). It starts empty.
- [ ] A Trello board with the lists, labels, and template cards from
      [A6.8](playbook.md#a6-configure-claude-code):
  - A pitch template card, with the content of Pachi's `_package-template.md`.
  - A ticket template card: [Appendix D](playbook.md#appendix-d--ticket-template), plus the B14 checklist.
- [ ] The Ideas list is an unranked idea pile. It's explicitly **not** a
      backlog or a commitment; Shape Up bets come from fresh shaping, not from
      grooming a list [23].
- [ ] Playtest and session notes go on Inbox cards as attachments, not in the repo.
- [ ] No `TODO.md` and no `docs/plan/`. Active work lives in the cycle list's cards.

**Exit:** `/shape` produces a pitch card from the template card, and a repo
grep for card links outside commit messages finds nothing.

### E9. CI and dependency hygiene

- [ ] `.github/workflows/ci.yml` with two jobs:
  - **fast**: on every push. No Godot: format, build, `dotnet test core.tests`.
  - **full**: on PRs to `main`. Installs Godot Mono at the `.godot-version`
    version (for example, with a setup-godot action) and runs `verify.sh`
    headless. Uploads `out/logs/` as an artifact on failure [9].
- [ ] Branch protection on `main`: require both jobs. Allow no direct pushes,
      even for you; that makes "main is always green" real.

⚖ **Branch protection for a solo dev.** It adds a PR step to every change.
The pay-off is that Claude-authored work can never reach `main` without both
gates and your merge. That's the "who has the last word" control from [15].
Skip it only if you'll reliably run `verify.sh` before every merge.

**Exit:** a PR with a failing core test is blocked from merging. Fixing it unblocks the PR.

### E10. Walking skeleton (first ticket, through the B4 loop)

Build the thinnest slice that touches every layer [32], **as a real ticket
using the [B4](playbook.md#b4-building-a-ticket-with-claude-the-core-loop)
loop**. This tests the process as much as the code:

- [ ] Shape a one-paragraph pitch card: "a ball drops into a pocket and the wallet
      increases." `/slice` it into one ticket card.
- [ ] **core:** `Payout.For(pocket, ball)` plus `Wallet`, test-first.
- [ ] **engine:** a `Pocket` node calls core and emits a past-tense signal
      (`PaidOut`), with a GoDotTest that drops a ball and asserts the signal fired [1].
- [ ] **scene:** `src/main/main.tscn` with one pocket and a label bound to the wallet.
- [ ] **visual:** `capture.sh` with an input script that launches one ball, at a
      fixed seed. Claude reads the frames and confirms the label changes.
- [ ] **ship:** export the build (E6) and play it.
- [ ] Run [B5](playbook.md#b5-reviewing-llm-output)'s review checklist and
      [B14](playbook.md#b14-definition-of-done-per-ticket)'s definition of done,
      then merge through CI.
- [ ] The card's plan and evidence comments are there, and the card moved
      across the board without you moving it by hand.

**Exit:** you merged a ticket whose every step followed
[B4](playbook.md#b4-building-a-ticket-with-claude-the-core-loop), and nothing
about
the loop needed a workaround. If something did, fix the setup now. That's
the cheapest point at which it will ever be fixed.

### E11. Fresh-clone acceptance test

- [ ] On a different directory (ideally a different machine), `git clone`,
      follow **only** `README.md`, and get `check.sh` and `verify.sh` green.
- [ ] Time it, and note the time in the README. Anything you had to do that
      isn't in the README is a setup bug: fix the README or the scripts.
- [ ] Tag `skeleton/1`. This is the baseline every future cycle starts from.

**Exit:** a clean clone goes from nothing to a green gate using only the README.

