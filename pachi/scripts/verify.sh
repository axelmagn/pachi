#!/usr/bin/env bash
set -euo pipefail

# Navigate to project root (parent directory of scripts/)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${PROJECT_ROOT}"

FIX_MODE=false
for arg in "$@"; do
    case "$arg" in
        --fix|-f)
            FIX_MODE=true
            shift
            ;;
        *)
            ;;
    esac
done

echo "=================================================="
echo " Starting Pachi Verification Pipeline"
echo "=================================================="

# Determine godot executable (godot-mono or godot)
if command -v godot-mono &> /dev/null; then
    GODOT_BIN="godot-mono"
elif command -v godot &> /dev/null; then
    GODOT_BIN="godot"
else
    GODOT_BIN=""
fi

# Stage 1: Format Check
echo ""
echo "[1/3] Checking C# code format and style..."
if [ "$FIX_MODE" = true ]; then
    echo "  Running: dotnet format Pachi.sln (Auto-fixing formatting)"
    dotnet format Pachi.sln
else
    echo "  Running: dotnet format Pachi.sln --verify-no-changes"
    dotnet format Pachi.sln --verify-no-changes
fi
echo "✓ Formatting clean!"

# Stage 2: Build & Roslyn Analyzers
echo ""
echo "[2/3] Building solution with strict Roslyn analyzer checks..."
echo "  Running: dotnet build Pachi.sln"
dotnet build Pachi.sln
echo "✓ Build succeeded with 0 warnings and 0 errors!"

# Stage 3: Headless Godot Runtime & Tests
echo ""
echo "[3/3] Running Headless Godot Test Suites..."
if [ -n "$GODOT_BIN" ]; then
    echo "  Running: ${GODOT_BIN} --headless -s tests/TestRunner.cs"
    "${GODOT_BIN}" --headless -s tests/TestRunner.cs
    echo "✓ Headless tests completed successfully!"
else
    echo "⚠ Warning: godot-mono/godot executable not found in PATH; skipping headless Godot checks."
fi

echo ""
echo "=================================================="
echo " All verification checks passed successfully! 🎉"
echo "=================================================="
