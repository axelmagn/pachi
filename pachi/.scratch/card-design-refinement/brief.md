Before moving to implementation, I need to overhaul the card system design.  The
current MVP design doc is sparse on details for the card system, and incorrect
in some places.

Some of the things I want in the cards system:

- rather than modifying board elements, cards should be a package deal - they
  overwrite the pocket / pin block / spinner entirely.
- yakumono cards should be separate from pocket cards, and generally scoring a
  yakumono should have a more rewarding and more interesting effect.
- there is a "card shop" meter that fills up over time.  When it fills up, a new
row of cards is dealt in the shop.
- scoring in the yakumo or pockets boosts the fill speed of this meter until the next time it triggers
- each card in the shop costs 1-4 ball of a specific tier
- cards should appear in the shop in rows of three
- there is a cursor in the shop that shows the next row that will be dealt to
- when the player buys a card, the other cards in that row are discarded
- when a row of cards is dealt to, any existing cards in that are discarded
- when all cards have been dealt from the deck, there is no reset.  That's just
  the board for this run.

The objective of this session is to create a standalone design doc for the
card system, and update the design MVP doc to reflect these changes and link to
it.
