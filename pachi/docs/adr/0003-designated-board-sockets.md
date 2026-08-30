# 3. Designated Board Sockets for Board Building

We structure board building around fixed, designated board sockets rather than freeform grid placement or coordinate-free spawning.

## Context & Trade-offs
Players modify their board by buying cards that place or upgrade components (Beetle Pockets, Pin Blocks, and Spinners).
- We rejected freeform grid tile placement across the entire playfield. Freeform placement allows players to cluster scoring pockets immediately below the launcher entrance, bypassing obstacle pins and breaking pachinko physics dynamics.
- We rejected purely static boards where cards only modify global stats. Static boards lack visual spatial customization and tangible tactile upgrades.
- We chose designated board sockets: fixed positions on the single board engineered for specific component categories (Beetle Pocket Sockets, Pin Block Sockets, Spinner Sockets). Players swap and upgrade archetypes within balanced lanes, preserving deflection dynamics while enabling modular build variety.
