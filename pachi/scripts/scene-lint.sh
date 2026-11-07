#!/usr/bin/env bash
# Lints Godot text scenes/resources for references the editor would silently
# drop: every ExtResource("id") / SubResource("id") must have a matching
# [ext_resource ... id="id"] / [sub_resource ... id="id"] declaration.
#
# Usage: scene-lint.sh [--all | <file>...]
#   no args  lint .tscn/.tres files changed vs HEAD, plus untracked ones
#   --all    lint every tracked .tscn/.tres
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

files=()
if [ "${1:-}" = "--all" ]; then
    mapfile -t files < <(git ls-files -- '*.tscn' '*.tres')
elif [ $# -gt 0 ]; then
    files=("$@")
else
    mapfile -t files < <({ git diff --name-only --relative HEAD -- '*.tscn' '*.tres'; \
        git ls-files --others --exclude-standard -- '*.tscn' '*.tres'; } | sort -u)
fi

status=0
for f in "${files[@]}"; do
    [ -f "$f" ] || continue   # deleted files
    awk -v file="$f" '
        /^\[ext_resource / { if (match($0, / id="[^"]*"/)) ext[substr($0, RSTART+5, RLENGTH-6)] = 1 }
        /^\[sub_resource / { if (match($0, / id="[^"]*"/)) sub_[substr($0, RSTART+5, RLENGTH-6)] = 1 }
        {
            line = $0
            while (match(line, /(Ext|Sub)Resource\("[^"]*"\)/)) {
                ref = substr(line, RSTART, RLENGTH); line = substr(line, RSTART + RLENGTH)
                kind = substr(ref, 1, 3); id = ref; sub(/^[A-Za-z]+\("/, "", id); sub(/"\)$/, "", id)
                uses[NR SUBSEP kind SUBSEP id] = 1
            }
        }
        END {
            bad = 0
            for (k in uses) {
                split(k, p, SUBSEP)
                if ((p[2] == "Ext" && !(p[3] in ext)) || (p[2] == "Sub" && !(p[3] in sub_))) {
                    printf "%s:%s: %sResource(\"%s\") has no matching declaration\n", file, p[1], p[2], p[3]
                    bad = 1
                }
            }
            exit bad
        }' "$f" || status=1
done
exit $status
