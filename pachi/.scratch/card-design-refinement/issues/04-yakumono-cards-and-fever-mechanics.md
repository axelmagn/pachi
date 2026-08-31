# 04: Yakumono Card Archetypes and Fever Reward System

Type: grilling
Status: resolved
Blocked by: 03

## Question

How should Yakumono cards be defined as distinct archetypes from standard Pocket cards, what rewarding/dynamic center mechanics and state changes (multi-ball launches, board-wide multipliers, shop surges) should they trigger, and how do Yakumono entries synergize with the card shop deal meter?

## Answer

1. **Yakumono Entry & Physical Catch Flow**:
   - Entering balls are captured by the Yakumono funnel/mouth, triggering a 0.5-second "chewing/celebration" animation sequence before the ball is cleanly despawned.
   - Entry immediately triggers audio/visual fireworks, awards designated tier payouts/eruptions, and activates or refreshes the 10.0-second Fever state.

2. **Fever Mode Mechanics & Board Effects**:
   - **Duration & Refresh**: Standard 10.0-second timed Fever. Subsequent ball entries while Fever is active reset the timer to 10.0 seconds and award full entry payouts cumulatively.
   - **Tulip Wing Lock**: All Beetle Pocket tulip wings on the board transition to and remain locked in their wide-open state for the entire 10-second duration, catching incoming balls without closing.
   - **Card Shop Synergy**: Applies an instant flat +35% boost to the Deal Meter (hard clamped at 100%) and grants a temporary 2.0x Deal Meter fill speed multiplier for 5.0 seconds.

3. **Distinct Yakumono Package-Deal Archetypes**:
   - Yakumono cards are dedicated centerpiece components (`YakumonoCardData`) mounting into the single central Yakumono Socket:
     - **Gatling Erupter**: Erupts rapid bursts of physical bonus balls directly into play from the centerpiece nozzles during Fever.
     - **Alchemist Transmuter**: Concentrates payouts into scarce Tier 3 (Gold) and Tier 4 (Ruby) balls deposited into the hopper queue for high-tier card purchasing.
     - **Market Dynamo**: Triggers instant free deals / shop surges and temporarily discounts card purchase costs.
     - **Tulip Overdrive**: Extends base Fever duration and applies board-wide payout multipliers to all beetle pockets.
