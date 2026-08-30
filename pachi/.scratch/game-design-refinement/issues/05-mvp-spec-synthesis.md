# 05: MVP Design Spec Synthesis

Type: task
Status: resolved
Blocked by: 02, 03, 04

## Question

How should the resolved decisions for the core pachinko loop, board-building card shop, prestige progression, and baseline balance be compiled into a lean, actionable MVP design spec in `docs/design/mvp-spec.md`?

## Answer

Compiled and synthesized all resolved decisions into [`docs/design/mvp-spec.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md):

1. **Core Loop & Launch Dynamics**: Defined starter hopper reserve (50 Tier-1 balls), manual flipper charging versus motorized auto-launching, and ball tier progression ($1\times$ to $50\times$).
2. **Board Architecture & Designated Sockets**: Specified the single-board layout with 5 Beetle Pocket Sockets, 2 Pin Block Sockets, 2 Spinner Sockets, and 3 Global Passive Slots ([ADR 0003](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0003-designated-board-sockets.md)).
3. **Pockets, Yakumono & Fever Mode**: Documented tulip wing mechanics (24 px closed to 54 px open), base payouts, Yakumono jackpot entry, 10-second Fever state, and automatic shop refresh.
4. **In-Run Card Shop**: Documented the 3-card persistent sidebar, ball purchasing mechanics, manual reroll cost scaling, and component socket installation rules.
5. **Prize Meter & Prestige Reset Loop**: Defined Prize Meter exponential scaling (`BaseTarget * 1.50^L`), Prize Token awards, Prestige Reset triggers, and Prestige Shop progression tree frontier drafting ([ADR 0004](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0004-prestige-progression-tree-frontier.md)).
6. **Baseline Economy Numbers & Initial Card Catalog**: Established starting parameter numbers and initial 7-card draft catalog.
7. **Telemetry Logging Specification**: Structured JSON schema for session telemetry tracking ball survival ratios, pocket capture distributions, and run pacing.
8. **Verification & Acceptance Criteria**: Set explicit testable criteria for gameplay verification and code quality standards.
