# 02: Prize Meter UI and Top Bar Component

**Type:** task  
**Status:** resolved  
**Blocked by:** 01  

## Description
Implement `PrizeMeterUI` in `src/prestige/ui/PrizeMeterUI.cs` and `prize_meter_ui.tscn`:
- Progress bar displaying current score / target score and percentage.
- Token count indicator label/badge.
- "Prestige Reset" button that is enabled when `TotalTokens >= 1` (or `TokensEarnedInRun >= 1`) and disabled when 0.
- Connects to `PrizeMeter` events and emits `ResetRequested` when clicked.
- Unit / UI tests in `src/prestige/tests/PrizeMeterUITests.cs`.
