# 03: Discrete Ball Tier Cost and Hopper Economy Model

Type: grilling
Status: resolved
Blocked by: 02

## Question

How does the hopper track and expose discrete inventories across ball tiers (1 to 4), how are card costs (1–4 balls of a specific tier) validated and deducted, and how do ball conversions, payouts, and purchase validations interact without ambiguity?

## Answer

1. **Hopper FIFO Queue Structure**:
   - The hopper maintains a strict sequential First-In-First-Out (`Queue<BallVariant>` / `List<BallVariant>`) collection of individual ball instances awaiting launch.
   - Launching always draws and fires the ball at the head (front) of the queue.
   - All newly earned balls (from pocket payouts, Yakumono rewards, fever bonuses) and refunded stuck balls are appended to the back (tail) of the FIFO queue.
   - Alternate launch modes (e.g. tier selectors or bucket auto-prioritization) are ruled out of scope for MVP as potential future upgrade cards.

2. **Card Purchase Validation & Exact-Tier Deduction**:
   - Each Package-Deal Card has a discrete cost: $N$ balls of Tier $T$ ($N \in [1, 4]$, $T \in [1, 4]$).
   - **Validation**: A card is purchasable if and only if the hopper contains at least $N$ balls of Tier $T$ (`Hopper.CountTier(T) >= N`).
   - **Hopper-Only Scope**: In-flight / airborne balls cannot be committed or spent; only balls currently residing in the hopper queue count toward purchase validation.
   - **No Auto-Substitution**: Higher tier balls do not automatically downgrade or substitute for lower tier ball costs.
   - **Deduction Execution**: Upon confirming a purchase, the hopper performs a front-to-back scan and removes the earliest $N$ instances of Tier $T$ balls from the queue, preserving the relative ordering of all other remaining balls.

3. **Hopper Capacity & HUD Presentation**:
   - **Capacity**: Unlimited queue capacity (soft cap 999) to prevent lost payout balls during large multi-ball cascades or fever events.
   - **HUD Visualization**:
     - A physical launcher trough / rail displays the next 5–8 incoming balls at the head of the queue.
     - Compact badge counters display total counts per tier (`[T1: x12] [T2: x4] [T3: x1] [T4: x0]`) for at-a-glance card purchasing feasibility.

4. **Zero-Ball Bankruptcy Recovery (Emergency Drip)**:
   - If total hopper count reaches 0 AND active balls in flight reach 0, an **Emergency Drip** triggers after a 2.0-second grace delay.
   - Grants a minimal starter batch of 3–5 Tier 1 balls (or a steady 1 ball/sec drip) into the hopper tail so the run never softlocks and players can continue playing until choosing a voluntary Prestige Reset.
