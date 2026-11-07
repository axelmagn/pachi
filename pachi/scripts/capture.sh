#!/usr/bin/env bash
# Records a scene to PNG frames for visual review and determinism checks.
# Not headless: movie capture needs a real renderer.
#
# Usage: capture.sh <scene> <frames> [--input <file>] [--seed <n>] [--debug-collisions]
#   <scene>   res:// path or project-relative path, e.g. src/main_game/main_game.tscn
#   --input   tests/input/*.json script replayed by tests/capture/InputPlayback.cs
#   --seed    RNG seed (default: HarnessArgs.DefaultSeed, i.e. 1)
# Prints the frame directory (out/capture/<scene>-s<seed>[-<input>]/).
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

usage() { die "usage: capture.sh <scene> <frames> [--input <file>] [--seed <n>] [--debug-collisions]"; }
[ $# -ge 2 ] || usage
SCENE="$1"; FRAMES="$2"; shift 2
INPUT=""; SEED="1"; EXTRA=()
while [ $# -gt 0 ]; do
    case "$1" in
        --input) INPUT="${2:?}"; shift ;;
        --seed) SEED="${2:?}"; shift ;;
        --debug-collisions) EXTRA+=(--debug-collisions) ;;
        *) usage ;;
    esac
    shift
done
[[ "$FRAMES" =~ ^[0-9]+$ ]] || usage

to_res() { case "$1" in res://*) echo "$1" ;; *) echo "res://${1#./}" ;; esac; }
SCENE_RES="$(to_res "$SCENE")"
[ -f "${SCENE_RES#res://}" ] || die "scene not found: $SCENE"
NAME="$(basename "${SCENE_RES%.tscn}")-s${SEED}"
ARGS=(--scene "$SCENE_RES" --seed "$SEED")
if [ -n "$INPUT" ]; then
    INPUT_RES="$(to_res "$INPUT")"
    [ -f "${INPUT_RES#res://}" ] || die "input script not found: $INPUT"
    NAME="${NAME}-$(basename "${INPUT_RES%.json}")"
    ARGS+=(--input "$INPUT_RES")
fi

GODOT_BIN="$(find_godot)" || die "Godot not found (set GODOT, or put godot-mono on PATH)"
check_godot_version "$GODOT_BIN"
redirect_xdg_if_needed
dotnet build Pachi.sln --nologo -v q >/dev/null || die "build failed; run ./scripts/check.sh"

OUT="out/capture/${NAME}"
rm -rf "$OUT"; mkdir -p "$OUT"
"$GODOT_BIN" --write-movie "$OUT/frame.png" --fixed-fps 60 --quit-after "$FRAMES" \
    "${EXTRA[@]}" --log-file "$OUT/capture.log" -s tests/capture/CaptureRunner.cs -- "${ARGS[@]}" \
    >/dev/null 2>&1 || die "capture failed, see $OUT/capture.log"
echo "$OUT"
