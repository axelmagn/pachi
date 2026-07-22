# Pachi Project Guidelines

Guidelines and technical context for AI coding agents working on **Pachi**.

## 1. Tech Stack & Environment
- **Engine**: Godot 4.7 (C# / Mobile target)
- **SDK & Runtime**: .NET 8.0 (`Godot.NET.Sdk/4.7.0`), Android build target `.NET 9.0`
- **Physics Engines**: 
  - 2D: **Rapier2D** (`addons/godot-rapier2d`)
- **Solution File**: `Pachi.sln` / `Pachi.csproj`

## 2. Repository Structure & Naming Conventions
- **Code Directory**: All source code is organized by domain in `src/`
- **C# Scripts**: `PascalCase.cs` (e.g., `PocketBallsIndicator.cs`).
- **Godot Scenes**: `snake_case.tscn` (e.g., `pocket.tscn`, `main_game.tscn`).
- **UID Sidecar Files**: Do not modify or delete `.cs.uid` files; these are managed by Godot 4.7's UID system.

## 3. Build & Verification Commands
- **Compile Solution**:
  ```bash
  dotnet build Pachi.sln
  ```
- **Rule**: Always execute `dotnet build Pachi.sln` after modifying C# code to verify 0 errors and 0 warnings before declaring a task complete.

## 4. Coding Standards & Patterns
- **Partial Classes**: Godot C# scripts must be partial classes inheriting from Godot node types (e.g., `public partial class Pocket : Node2D`).
- **Attributes**:
  - `[GlobalClass]`: Apply to custom node classes globally registered in Godot.
  - `[Tool]`: Apply to scripts meant to execute in the Godot editor viewport.
  - `[Export]`: Apply to inspector-exposed properties.
- **Redraw Property Setters**: For `[Tool]` scripts with custom `_Draw()` rendering, use backing fields and call `QueueRedraw()` inside the property setter. Reference: [PocketBallsIndicator.cs](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/src/pockets/PocketBallsIndicator.cs).
- **Node Validation**: Use `Debug.Assert(...)` in `_Ready()` to validate required exported node references. Reference: [Pocket.cs](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/src/pockets/Pocket.cs).
- **Type-Safe API References**: Avoid magic strings in engine calls (e.g. `TweenProperty`). Use source-generated `PropertyName` (e.g. `PropertyName.Scale`), `SignalName`, `nameof(...)`, or static `StringName` constants for group/physics tags.
- **Signal & Event Lifecycles**: Prevent memory leaks and duplicate callbacks. Wiring handlers with `+=` in `_Ready()` is safe for same-scene sibling/child nodes. When subscribing to long-lived Autoloads/singletons in `_Ready()`, always unsubscribe (`-=`) in `_ExitTree()`. For dynamic entities or single-use events triggered inside helper methods, prefer `await ToSignal(...)` or `Connect(..., (uint)ConnectFlags.OneShot)`.
- **Asynchronous Signatures**: Use `async Task` (or `async ValueTask`) for asynchronous methods. Avoid `async void` to ensure proper exception propagation.

## 5. Engine Configurations & Guardrails
- **Physics Layer 2**: Designated for `"Ball"`.
- **Input Action**: `launcher_charge` (mapped to Spacebar).
- **Language Boundary**: Write all logic in C#. Do not introduce GDScript files unless explicitly instructed.
