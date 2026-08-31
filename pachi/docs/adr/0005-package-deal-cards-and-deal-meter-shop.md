# 5. Package-Deal Cards, Deal Meter Shop, and Discrete Tier Ball Economy

We structure in-run card upgrades as self-contained package-deal scenes that overwrite designated board sockets, governed by a dynamic score-boosted Deal Meter shop and discrete ball tier purchasing costs.

## Context & Trade-offs

In-run deckbuilding requires clear card application rules, intuitive shop pacing, and a tight economic link to pachinko ball physics without micromanagement friction.

- **Package-Deal Scene Overwrite vs. Sub-Property Mutation**:
  - *Rejected*: Incremental mutation of existing board node parameters (e.g., adding +5% bounce or +1 payout to an active pocket). Mutating properties creates messy hidden state, visual inconsistencies, and complex undo/downgrade logic.
  - *Chosen*: Package-deal cards holding a self-contained `PackedScene`. Installing a card cleanly flushes and frees the previous component instance and mounts the new archetype into the designated socket.
- **Deal Meter & Cursor Lifecycle vs. Manual Rerolls / Static Timers**:
  - *Rejected*: Manual coin/ball-cost rerolls or static instant shop refreshes. Manual rerolls encourage compulsive spamming and break the relaxing rhythm of watching balls fall.
  - *Chosen*: A 3-row (3x3) shop layout advanced by a top-down sequential Deal Cursor driven by a passive 20s Deal Meter that accelerates with pocket (+10%, +0.5x) and Yakumono (+35%, +2.0x) scoring hits. Buying a card discards remaining cards in its row; drawing overwrites unpurchased cards. Runs conclude with finite Master Deck exhaustion without mid-run reshuffling.
- **Discrete Exact-Tier Ball Costs vs. Generic Currency Value**:
  - *Rejected*: Generic numeric ball prices or automatic currency conversion/downgrading.
  - *Chosen*: Discrete ball tier costs ($1\text{--}4$ balls of Tier $1\text{--}4$). Players must produce and hold the exact tier in their strict FIFO Hopper Queue, making pocket upgrades and Yakumono transmutations mechanically distinct and rewarding.
