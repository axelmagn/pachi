# 0004. Upgrade dependencies by hand; no Renovate or dependency bot

- Status: accepted
- Date: 2026-11-06

## Context

The fresh-project target suggests Renovate or Dependabot so the Godot
toolchain and NuGet packages get bumped in grouped PRs. What Pachi pins:

- The Godot toolchain: `.godot-version`, `Godot.NET.Sdk`, `global.json`.
- About six NuGet packages, all test-only (xunit, Shouldly, GoDotTest and
  the test SDK); none ship in the game.
- GitHub Actions versions in `.github/workflows/`.

A Godot upgrade means a local editor install, a re-import, and possibly
re-tuning physics feel. A bot can't do any of that, and its PR fails the
local version check until the editor is upgraded by hand. The riskiest
dependency, the vendored Rapier2D addon, isn't tracked by either bot.
Renovate is a third-party GitHub app with write access to the repo.

## Decision

- No Renovate, Dependabot, or other dependency-update bot.
- Versions stay pinned exactly.
- A Godot upgrade is a ticket, done deliberately and verified like any other.
- Check NuGet packages and Actions versions by hand now and then
  (`dotnet list package --outdated`).

## Consequences

No third-party app with write access and no PR noise to triage. In return,
updates happen only when someone remembers. Old Actions versions may hit
GitHub deprecations; CI warnings will show that. Revisit if shipped
(non-test) packages are added, or a security advisory is missed.
