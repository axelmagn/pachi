# Pachi

Incremental pachinko deckbuilder. Godot 4.7.2 Mono, C# (.NET 8), Rapier2D physics.
Process: docs/process/playbook.md (B4 is the per-ticket loop; §3 "state seam" says what goes where).
Claude runs from this directory (pachi/pachi/); the git root is one level up and holds old prototypes.

## Work tracking (Trello board "Pachi Agent", via the Trello MCP)
- Board: https://trello.com/b/bCqFNbKV/pachi-agent
- Every ticket is a card. Read its description, comments, and linked pitch card before starting (/ticket <card>).
- The description is the spec. Never change acceptance criteria, appetite, or no-gos unless asked; when you edit a description, comment what changed and why.
- Post your approved plan, then your evidence (command + output tail, key frames attached), as comments. Move the card: In progress → Review / Play.
- Don't archive or delete cards, or change lists or labels, unless asked.
- Anything that must outlive the card (decisions, design facts) goes in docs/decisions/ or docs/design/, never only in a comment. Repo files never reference cards.

## Commands
- Fast check (format, scene lint, build, core tests): ./scripts/check.sh   (--fix to auto-format)
- Full gate (+ headless Godot tests, all-scenes load, log scan): ./scripts/verify.sh
- One core test: dotnet test core.tests --filter "FullyQualifiedName~<TestClass>"
- One Godot suite/test: ./scripts/verify.sh -- --run-tests=<Suite>[.<Method>]
- See or play a scene: ./scripts/capture.sh res://src/<path>.tscn <frames> [--input tests/input/<name>.json] [--seed <n>] [--debug-collisions]  → PNG frames you can read
- Worktrees (`claude -w`) check out the whole repo: cd into `pachi/` inside the worktree, and run `godot-mono --headless --import` once (.godot/ isn't committed).

## Where things go
- Game rules with no engine dependency → core/ (Pachi.Core, no Godot types), tests in core.tests/ (xUnit). New logic starts here, test-first.
- Nodes and scenes → src/<domain>/. Nodes stay thin: they map core state/events to the engine. Main scene: src/main_game/main_game.tscn.
- Engine tests → tests/ (GoDotTest). Input scripts → tests/input/*.json.
- Decisions → docs/decisions/. Settled design → docs/design/. Plans, tickets, playtest notes → Trello.

## Conventions the code won't tell you
- Required node refs: [Export] assigned in the scene + Debug.Assert in _Ready. No GetNode fallbacks or auto-resolution.
- Allowed globals: GameConfig, GlobalEvents, frozen (docs/decisions/0003-globals.md). Don't add autoloads, static Instance singletons, or members to either.
- [Tool] only when the script must run or draw in the editor.
- Never write .uid files or uid:// values; Godot generates them.
- Input actions, physics layers, autoloads: change them via the editor or godot-ai tools, never by editing project.godot text.
- No commented-out code; git history is the archive.
- Don't add options, fallbacks, or abstractions the ticket didn't ask for. If you think one is needed, say so instead.
- Changing a test and the code it tests in one diff needs a one-line reason in a card comment.
- Docs: open with "For: <reader>. Lets them: <task>." Hard-wrap at 80. Budget ≤40 lines (ticket/ADR), ≤80 (how-to), ≤150 (design); exceed only with a stated reason. Research goes in docs/research/, linked.

## Gotchas
- Godot API knowledge: check ../ref/godot-docs/_sources/ (4.7 docs; classes/class_*.rst.txt). Training data skews Godot 3.
- Physics engine is Rapier2D (addons/godot-rapier2d), not Godot Physics. Models know it poorly: read the addon's docs and source before relying on any Rapier2D behaviour or API. Captures aren't bit-exact across runs (ADR 0002).
- Godot logs errors (including C# exceptions in callbacks) without a failing exit code; trust verify.sh, not "no crash".
- A Stop hook runs ./scripts/check.sh --quiet; a red fast gate blocks ending the turn. Fix the cause, don't work around it.
