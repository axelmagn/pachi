# Pachi Domain Context

Pachi is a physics-based pachinko / roguelike deckbuilder game developed in Godot 4.7 with C#.

## Language

**VisualConfig**:
A centralized data resource located at `res://src/art/visual_config.tres` containing the game's visual customization settings, including color palettes, sprite textures, and environmental aesthetics.
_Avoid_: Theme (conflicts with Godot's UI `Theme` system), Skin

**Visual Showcase**:
A static in-editor scene (`res://src/art/visual_showcase.tscn`) displaying side-by-side instances of all visual game components (pins, pockets, boundaries, cards, UI) used to preview live `VisualConfig` property edits in the Godot 2D editor viewport.
_Avoid_: Design Sandbox, Theme Editor, Test Level, Dev Playground

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
The recovery process that despawns a persistently stuck ball after failed nudges and re-queues an equivalent ball variant in the hopper.
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

