#!/usr/bin/env bash
# Full gate: check.sh, then the engine tests headless, then a log scan.
# Fails if Godot is missing; it never skips the engine stage.
#
# Usage: verify.sh [--fix] [-- <GoDotTest args, e.g. --run-tests=AllScenesTests>]
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

CHECK_ARGS=()
TEST_ARGS=()
while [ $# -gt 0 ]; do
    case "$1" in
        --fix|-f) CHECK_ARGS+=(--fix) ;;
        --) shift; TEST_ARGS=("$@"); break ;;
        *) die "unknown option: $1 (usage: verify.sh [--fix] [-- <test args>])" ;;
    esac
    shift
done

GODOT_BIN="$(find_godot)" || die "Godot not found (set GODOT, or put godot-mono on PATH)"
check_godot_version "$GODOT_BIN"

./scripts/check.sh "${CHECK_ARGS[@]}"

redirect_xdg_if_needed
mkdir -p out/logs
LOG_FILE="out/logs/test.log"
ALLOWLIST="scripts/log-allowlist.txt"

# Not just .godot/: check.sh's dotnet build creates .godot/mono/ first.
if [ ! -d .godot/imported ]; then
    echo "▸ import (first run in this checkout)"
    "$GODOT_BIN" --headless --import >out/logs/import.log 2>&1 \
        || { tail -n 40 out/logs/import.log >&2; die "import failed, see out/logs/import.log"; }
fi

echo "▸ engine tests"
set +e
"$GODOT_BIN" --headless --log-file "$LOG_FILE" -s tests/TestRunner.cs -- "${TEST_ARGS[@]}" >/dev/null 2>&1
TEST_STATUS=$?
set -e

echo "▸ log scan"
PATTERN='SCRIPT ERROR|ERROR:|Unhandled exception|Fatal error'
mapfile -t ALLOWED < <(grep -vE '^[[:space:]]*(#|$)' "$ALLOWLIST" 2>/dev/null || true)
FLAGGED="$(grep -nE "$PATTERN" "$LOG_FILE" || true)"
for allowed in "${ALLOWED[@]}"; do
    FLAGGED="$(printf '%s\n' "$FLAGGED" | grep -vE -- "$allowed" || true)"
done
FLAGGED="$(printf '%s\n' "$FLAGGED" | sed '/^$/d')"

RESULTS="$(grep -E 'Test results:' "$LOG_FILE" | tail -n1 | sed 's/.*Test results: //' || true)"
echo ""
echo "=================================================="
echo " engine tests: ${RESULTS:-no results} (exit $TEST_STATUS)"
echo " log:          $LOG_FILE"
if [ -n "$FLAGGED" ]; then
    echo " log scan:     $(printf '%s\n' "$FLAGGED" | wc -l) flagged line(s):"
    printf '%s\n' "$FLAGGED" | head -n 15 | sed 's/^/   /'
fi
echo "=================================================="

[ "$TEST_STATUS" -eq 0 ] || die "verify.sh: engine tests failed"
[ -z "$FLAGGED" ] || die "verify.sh: log scan found errors (allowlist: $ALLOWLIST)"
echo "✓ verify.sh passed"
