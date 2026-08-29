# 01: VisualConfig distinct input/output indicator colors

**What to build:** Configurable background colors for input and output pocket indicators in `VisualConfig`, replacing the single generic background color property while preserving existing theme binding functionality.

**Blocked by:** None (can start immediately)

**Status:** resolved

- [x] `InputIndicatorBackgroundColor` defaults to `#1A2433` (slate/dark blue) in `VisualConfig.cs`.
- [x] `OutputIndicatorBackgroundColor` defaults to `#33221A` (amber/dark bronze) in `VisualConfig.cs`.
- [x] `IndicatorBackgroundColor` is deprecated or replaced without breaking existing tests.
- [x] Changing `InputIndicatorBackgroundColor` or `OutputIndicatorBackgroundColor` emits the `Changed` signal.
- [x] `VisualConfigTests.cs` validates new defaults and change events.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
