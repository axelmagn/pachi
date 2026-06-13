# Design Brainstorm (2026-06-10)

I had a good design discussion with Jacob today.  We did a lot of loose
brainstorming that resolved some long-standing design questions I was having.
We landed on a rough picture of what the vertical slice should look like, which
this document attempts to capture in broad strokes.


- balls come in different "tiers" that are color coded
- for simplicity, balls have a "base price" that is invisible to the user but
  can be used to balance conversions.
- jackpots can be upgraded to either pay out more balls of a certain type or
  upgrade from one
- there is a shop where you can buy upgrades and ball conversions, which are
  represented as cards dealt from a "shop deck".
- dragging a card from the shop onto a valid subject "buys" the card and
  applies the card's effect to the subject.
- the balls in the hopper are used as currency.
- the centerpiece jackpot triggers dealing more shop items from the shop deck,
  and awards prestige XP.
- when prestige XP passes a threshold, a prestige point is awarded and the next
  threshold increases exponentially.
- At any time (after a cooldown at the start of the game) the player may
  "prestige", which resets the board and allows them to spend prestige points to
  buy from the prestige upgrade tree.
- Prestige upgrades either manipulate the deck somehow (adding / removing /
  modifying cards), or upgrade parts of the board such as lever automation,
  cooldown timers, etc.
- in the first round, the player doesn't have enough cards in their deck to do
  the full upgrade path.  They have to build that gradually through prestige
  iterations.
- Prestige upgrades are presented as "card packs" that allow them to buy from a
  randomly drawn pool (rather than presenting an upgrade tree)
- UI is presented as a continuous single screen, rather than some sort of
  phased approach. (ISEPS over Balatro)

