# 04: Prestige Reset & Economy Balance Model

Type: grilling
Status: resolved
Blocked by: 01, 03

## Question

How should the prestige reset loop (prestige currency formula, resetting board state, purchasing card packs to expand the in-run draw pool) and baseline ball/pocket economy numbers be structured for rapid, satisfying progression?

## Answer

Established the prestige reset loop, progression tree drafting, and economy balance calibration model:

1. **Prize Meter & Prestige Reset Loop**:
   - Payout balls scored from Beetle Pockets and the Yakumono fill the in-run Prize Meter at the top of the play area.
   - Filling the meter awards Prize Tokens and scales the next fill target exponentially: `Target = BaseTarget * (GrowthFactor ^ Level)`.
   - Players holding at least 1 Prize Token can trigger a Prestige Reset at any time.
   - Prestige Reset clears the in-run board state, sockets, and active hopper balls, resetting the run to starter conditions while preserving meta-progression.

2. **Prestige Shop & Progression Tree Frontier**:
   - In the Prestige Shop, players spend Prize Tokens on individual open upgrades or themed booster packs.
   - Upgrades follow an incremental progression tree structure where node levels equal purchase counts.
   - The Prestige Shop drafts candidate upgrades exclusively from the active Prestige Frontier (unlocked and adjacent nodes).
   - Node levels provide distinct archetype benefits: adding card copies to the Master Deck (modifying in-run draw odds), increasing card baseline stats directly, or unlocking higher-tier card variants in the in-run shop pool.

3. **Economy Calibration & Telemetry Specification**:
   - Economy balance will be calibrated using statistical telemetry rather than static guesswork.
   - The MVP spec defines a Telemetry Logging Specification to track launch velocity distributions, pocket capture rates, Yakumono Fever frequency, meter fill rates, and run durations across test sessions.

4. **Decisions & ADRs**:
   - Documented architectural decision in [ADR 0004](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0004-prestige-progression-tree-frontier.md).
   - Added canonical domain terms (`Prize Meter`, `Prize Token`, `Prestige Reset`, `Progression Node`, `Prestige Frontier`) to [CONTEXT.md](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md).
