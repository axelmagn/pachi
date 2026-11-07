#!/usr/bin/env bash
# Fast gate, no engine run: format → scene lint → build → core tests.
#
# Usage: check.sh [--fix] [--quiet]
#   --fix    apply formatting instead of verifying it
#   --quiet  for hooks: exit 0 at once if no .cs/.csproj/.tscn/.tres changes,
#            and print output only on failure
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

FIX=false
QUIET=false
for arg in "$@"; do
    case "$arg" in
        --fix|-f) FIX=true ;;
        --quiet|-q) QUIET=true ;;
        *) die "unknown option: $arg (usage: check.sh [--fix] [--quiet])" ;;
    esac
done

if $QUIET && ! git status --porcelain --untracked-files=all . \
        | grep -qE '\.(cs|csproj|tscn|tres)$'; then
    exit 0
fi

LOG="$(mktemp)"
trap 'rm -f "$LOG"' EXIT
step() {
    local name="$1"; shift
    $QUIET || echo "▸ $name"
    if ! "$@" >"$LOG" 2>&1; then
        cat "$LOG" >&2
        die "check.sh: $name failed"
    fi
}

# .godot-version is checked here too, so a mismatched editor fails fast.
if GODOT_BIN="$(find_godot)"; then
    check_godot_version "$GODOT_BIN"
fi

if $FIX; then
    step "format (fix)" dotnet format Pachi.sln
else
    step "format" dotnet format Pachi.sln --verify-no-changes
fi
step "scene lint" ./scripts/scene-lint.sh
step "build" dotnet build Pachi.sln --nologo
step "core tests" dotnet test core.tests --nologo --no-build

$QUIET || echo "✓ check.sh passed"
