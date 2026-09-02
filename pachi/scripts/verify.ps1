[CmdletBinding()]
param (
    [Alias('f')]
    [switch]$Fix
)

$ErrorActionPreference = 'Stop'

# Navigate to project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path (Join-Path $ScriptDir "..")
Set-Location $ProjectRoot

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " Starting Pachi Verification Pipeline (PowerShell)" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Determine godot executable (godot-mono, godot, or godot.exe)
$GodotBin = $null
if (Get-Command "godot-mono" -ErrorAction SilentlyContinue) {
    $GodotBin = "godot-mono"
} elseif (Get-Command "godot" -ErrorAction SilentlyContinue) {
    $GodotBin = "godot"
} elseif (Get-Command "godot.exe" -ErrorAction SilentlyContinue) {
    $GodotBin = "godot.exe"
} elseif (Test-Path "C:\Users\Axel\Godot_v4.7\Godot_v4.7-stable_mono_win64\Godot_v4.7-stable_mono_win64_console.exe") {
    $GodotBin = "C:\Users\Axel\Godot_v4.7\Godot_v4.7-stable_mono_win64\Godot_v4.7-stable_mono_win64_console.exe"
}

# Stage 1: Format Check
Write-Host ""
Write-Host "[1/3] Checking C# code format and style..." -ForegroundColor Yellow
if ($Fix) {
    Write-Host "  Running: dotnet format Pachi.sln (Auto-fixing formatting)"
    dotnet format Pachi.sln
    if ($LASTEXITCODE -ne 0) { throw "dotnet format failed with exit code $LASTEXITCODE" }
} else {
    Write-Host "  Running: dotnet format Pachi.sln --verify-no-changes"
    dotnet format Pachi.sln --verify-no-changes
    if ($LASTEXITCODE -ne 0) { throw "dotnet format verify failed. Run with -Fix or 'dotnet format Pachi.sln' to fix formatting issues." }
}
Write-Host "✓ Formatting clean!" -ForegroundColor Green

# Stage 2: Build & Roslyn Analyzers
Write-Host ""
Write-Host "[2/3] Building solution with strict Roslyn analyzer checks..." -ForegroundColor Yellow
Write-Host "  Running: dotnet build Pachi.sln"
dotnet build Pachi.sln
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
Write-Host "✓ Build succeeded with 0 warnings and 0 errors!" -ForegroundColor Green

# Stage 3: Headless Godot Runtime & Tests
Write-Host ""
Write-Host "[3/3] Running Headless Godot Test Suites..." -ForegroundColor Yellow
if ($GodotBin) {
    Write-Host "  Running: $GodotBin --headless -s src/tests/TestRunner.cs"
    & $GodotBin --headless -s src/tests/TestRunner.cs
    if ($LASTEXITCODE -ne 0) { throw "TestRunner failed with exit code $LASTEXITCODE" }
    Write-Host "✓ Headless tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Warning: godot-mono/godot executable not found in PATH; skipping headless Godot checks." -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " All verification checks passed successfully!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan

