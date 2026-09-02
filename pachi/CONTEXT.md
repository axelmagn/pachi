# Pachi Domain Context

Pachi is a physics-based pachinko / roguelike deckbuilder game developed in Godot 4.7 with C#.

## Core Vision

1. **It's Pachinko**: Physical balls serve as both play resource and currency. Tactile deflection off pins, pocket entries, and jackpot triggers drive the core sensory experience.
2. **It's Incremental**: Layered progression curves scale ball values, pocket payouts, and board capabilities.
3. **It's a Deckbuilder**: Upgrades are purchased from a shop drawn from a deck of cards. Prestige resets allow players to shape and expand their deck.
4. **It's Simple**: Simple and legible systems designed for a relaxing, low-cognitive-load experience without micromanagement friction.

## Language


**Ball Variant**:
A configuration resource defining the gameplay attributes (tier price) and color representation for a specific ball tier.
_Avoid_: Ball Type, Ball Model

**Shape Sprite**:
A procedural vector drawing component used as a placeholder before final sprite textures are assigned.
_Avoid_: Dummy Sprite, Placeholder Graphic

**Stuck Ball**:
An active ball in play whose displacement remains within a minimum spatial threshold for longer than the detection window.
_Avoid_: Jammed Ball, Dead Ball

**Nudge**:
An automatic upward and lateral impulse applied to a stuck ball to dislodge it from pins, pockets, or bottlenecks.
_Avoid_: Shake, Tilt, Jolt

**Ball Refund**:
The recovery process that despawns a persistently stuck ball (after failed nudges) or recovers held balls during a socket component flush, re-queuing equivalent ball variants in the hopper queue.
_Avoid_: Despawn Penalty, Void Ball, Ball Recycle

**Pip**:
A rendered circular or iconographic visual element representing a single Ball Variant within a Pocket indicator or Card UI indicator.
_Avoid_: Dot, Ball Marker, Chip, Bead

**Yakumono**:
A specialized centerpiece feature on the board with dynamic mechanics, dedicated graphics/animations, and enhanced payout or state-altering behaviors.
_Avoid_: Center Pocket, Gimmick, Central Feature

**Beetle Pocket**:
The thematic visual and mechanical representation of a scoring pocket on the board, equipped with animated tulip wings and distinct behavioral archetypes.
_Avoid_: Goal, Target, Scoring Hole, Cup

**Socket**:
A designated, fixed mounting position on the board that accepts specific modular component cards.
_Avoid_: Slot, Tile, Grid Cell, Attachment Point

**Package-Deal Card**:
A self-contained component card that completely replaces the existing node instance inside a designated socket with a fresh `PackedScene` component, rather than mutating individual sub-properties.
_Avoid_: Upgrade Card, Modifier Card, Mutation Card, Component Patch

**Pin Block**:
A modular cluster of deflection pins occupying a designated board socket that players can swap or upgrade to alter ball trajectories.
_Avoid_: Pin Group, Peg Array, Peg Cluster

**Fever**:
A heightened reward state triggered by entering the Yakumono, opening all tulip wings, paying bonus balls, and drafting fresh shop cards.
_Avoid_: Jackpot Phase, Bonus Round, Super Mode

**Prize Meter**:
A persistent UI progress meter at the top of the play area that accumulates progress from ball payouts, awarding Prize Tokens upon filling.
_Avoid_: XP Bar, Level Bar, Score Gauge

**Prize Token**:
The primary prestige currency awarded by filling the Prize Meter, spent between runs in the Prestige Shop to unlock and level progression tree nodes.
_Avoid_: Prestige Point, Meta Coin, Gem, Star

**Prestige Reset**:
The voluntary meta-reset action that clears the in-run board state, sockets, and active hopper balls in exchange for spending earned Prize Tokens in the Prestige Shop.
_Avoid_: Soft Reset, Cash Out, Wipe, Rebirth

**Progression Node**:
An unlockable element on the meta progression tree representing a card or upgrade, whose level dictates Master Deck copies, stat boosts, or higher-tier variant availability.
_Avoid_: Tech Tree Node, Skill, Talent

**Prestige Frontier**:
The subset of progression nodes currently eligible to appear in the Prestige Shop drafts based on prerequisite unlocks.
_Avoid_: Available Pool, Unlocked Upgrades

**Deal Meter**:
The in-run timer and progress meter that accumulates passively over time and accelerates from Pocket and Yakumono scores, triggering a fresh 3-card row deal in the Card Shop upon reaching 100%.
_Avoid_: Shop Gauge, Card Timer, Deal Bar, Shop Progress

**Deal Cursor**:
The visual indicator in the Card Shop UI marking the next row index scheduled to receive newly dealt cards and have its previous contents discarded upon meter completion.
_Avoid_: Shop Pointer, Target Marker, Deal Index

**Master Deck**:
The finite collection of Package-Deal Cards configured for the current run, drawn into the Card Shop until exhausted with no mid-run reshuffling.
_Avoid_: Draw Pile, Run Deck, Card Pool

**Discard Pile**:
The inactive collection where unpurchased cards are sent when another card in their row is bought or when their row is overwritten by the Deal Cursor.
_Avoid_: Burn Pile, Scrap, Graveyard

**Hopper Queue**:
The strict First-In-First-Out sequential collection of Ball Variants awaiting launch, where payouts and refunds append to the tail, and launching draws from the head.
_Avoid_: Ball Bin, Ammo Pool, Ball Inventory, Magazine

**Discrete Ball Cost**:
The exact quantity ($1\text{--}4$) and specific tier ($1\text{--}4$) of Ball Variants required to purchase a Package-Deal Card from the Card Shop without substitution.
_Avoid_: Card Price, Currency Cost, Tier Cost

**Emergency Drip**:
The automatic low-rate trickle of Tier 1 Ball Variants granted into the hopper queue when both hopper inventory and active balls in flight reach zero, preventing softlocks.
_Avoid_: Bankruptcy Bailout, Free Balls, Pity Drip

**Board Eruption**:
The dynamic multi-ball ejection mechanic performed by specialized Yakumono cards that dispenses physical bonus balls directly into active board play during Fever mode.
_Avoid_: Ball Fountain, Multi-Spawn, Ball Explosion, Bonus Drop



