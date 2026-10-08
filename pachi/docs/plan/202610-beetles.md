# Beetles and Cards

**Idea:** Pockets should be beetles that grow over time.

## 1. Experience goal

- The player can play "cards" to place or replace pockets on the board.
- The player experiences the pockets as dynamic, living beetles with their own
legible characteristics and behaviors.

## 2. Appetite
- 2 weeks

## 3. Solution sketch (fat-marker level)

- verbs
    - hover over beetle
        - beetle opens its wings to show its payout
    - start dragging card
    - release card over beetle slot
        - beetle card: places or replaces beetle
        - growth card: grows beetle to next size
    - release card not over beetle slot
        - tweens back to card area

- nouns
    - beetle: the pocket, represented as a beetle
        - input gems on its wings
        - payout on its body beneath the wings
        - "grows" by scaling up and down
    - beetle slot: a place on the board where a beetle can go
        - temporary - does not need signifier.  will be replaced with dynamic
        placement at some point.
    - card: a UI affordance that can be dragged around
        - simple 9-patch rect
        - shows beetle (scaled)

- controls
    - mkb: click and drag
    - controller: select card, select location

- greybox: TODO
- signifiers: TODO

## 4. Rabbit holes

- beetle placement: for now, 3 pre-determined slots along the bottom
- peg coupling: for now, completely decoupled from obstacle pegs
- card generation: static deck of cards for now, dealt once at startup

## 5. No-gos

- no dynamic placement (yet)
- can only place beetles which appear on cards
- beetle cards are static for now (not dynamically generated)

## 6. Playtest & "done" criteria

- Player goal / design goal / emotional beat for this feature
- Landing & legibility checks; who plays it, how many, when (≥7 at cycle end)
- Quality bar at handoff: greybox-playable vs. vertical-slice polish
  (concentric rule: the mechanic's own feel should be finished to
  shippable even when its surroundings stay greybox)

## 7. Scopes

- Running TODO list
