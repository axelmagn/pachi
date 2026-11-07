#!/usr/bin/env bash
# Line budget for docs (playbook B15).
#
# Usage: doc-budget.sh [file.md ...]
#   No args: read a Claude Code hook payload on stdin and check its file_path.
#   Checks only files under docs/. Budgets by folder:
#     decisions/ ≤40, process/ ≤80, design/ ≤150, research/ no limit.
#   A doc may go over by stating why on a line starting "Over budget:".
#   Lines over 120 columns are flagged, except table rows and code blocks.
# Exits 2 with warnings on stderr (a PostToolUse hook shows them to Claude).
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."

files=("$@")
if [ ${#files[@]} -eq 0 ]; then
    f="$(jq -r '.tool_input.file_path // empty')"
    [ -n "$f" ] && files=("$f")
fi

status=0
for f in "${files[@]}"; do
    f="${f#"$PWD"/}"
    case "$f" in
        docs/decisions/*.md) budget=40 ;;
        docs/process/*.md) budget=80 ;;
        docs/design/*.md) budget=150 ;;
        *) continue ;;
    esac
    [ -f "$f" ] || continue

    lines=$(wc -l <"$f")
    if [ "$lines" -gt "$budget" ] && ! grep -q '^Over budget:' "$f"; then
        echo "doc-budget: $f is $lines lines (budget $budget). Cut it, or add an 'Over budget: <why>' line." >&2
        status=2
    fi

    wide=$(awk '/^```/ { code = !code; next }
                !code && !/^\|/ && length > 120 { printf "%d ", FNR }' "$f")
    if [ -n "$wide" ]; then
        echo "doc-budget: $f has lines over 120 columns (hard-wrap at 80): ${wide% }" >&2
        status=2
    fi
done
exit $status
