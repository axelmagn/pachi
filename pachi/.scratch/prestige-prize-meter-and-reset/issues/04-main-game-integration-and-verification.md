# 04: Main Game Integration and Test Suite Verification

**Type:** task  
**Status:** closed  
**Blocked by:** 02, 03  

## Description
- Integrate `PrizeMeterUI` into `main_game.tscn` in the top bar (`HBoxContainer2`).
- Wire `MainGameController` to handle `PrizeMeter` events, reset requests, and telemetry.
- Register all new test suites in `src/tests/TestRunner.cs`.
- Run `./scripts/verify.sh` to verify formatting, 0 build warnings/errors, and test passes.
