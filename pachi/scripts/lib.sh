# Shared helpers for the verification ladder. Source it; don't run it.
# Callers set -euo pipefail themselves.

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${PROJECT_ROOT}"

die() { echo "✗ $*" >&2; exit 1; }

# Godot writes user data, config and caches under XDG dirs. In a sandbox or
# CI they may be read-only, so fall back to out/xdg/.
redirect_xdg_if_needed() {
    local var dir default
    for var in XDG_DATA_HOME XDG_CONFIG_HOME XDG_CACHE_HOME; do
        case "$var" in
            XDG_DATA_HOME) default="${HOME:-/nonexistent}/.local/share" ;;
            XDG_CONFIG_HOME) default="${HOME:-/nonexistent}/.config" ;;
            XDG_CACHE_HOME) default="${HOME:-/nonexistent}/.cache" ;;
        esac
        dir="${!var:-$default}"
        mkdir -p "$dir" 2>/dev/null || true
        if [ ! -w "$dir" ]; then
            dir="${PROJECT_ROOT}/out/xdg/${var,,}"
            mkdir -p "$dir"
            export "$var=$dir"
            echo "  (note: $var not writable, using $dir)"
        fi
    done
}

# Finds the Godot binary (GODOT env, then godot-mono, then godot).
# Prints nothing and returns 1 if none is found.
find_godot() {
    if [ -n "${GODOT:-}" ]; then echo "$GODOT"; return 0; fi
    command -v godot-mono 2>/dev/null && return 0
    command -v godot 2>/dev/null && return 0
    return 1
}

# Fails unless `<godot> --version` starts with the contents of .godot-version.
check_godot_version() {
    local godot="$1" want have
    [ -f .godot-version ] || die ".godot-version is missing"
    want="$(tr -d '[:space:]' < .godot-version)"
    have="$("$godot" --headless --version 2>/dev/null | tail -n1 | tr -d '[:space:]')"
    case "$have" in
        "$want"|"$want".*) ;;
        *) die "Godot version mismatch: .godot-version says '$want', $godot is '$have'" ;;
    esac
}
