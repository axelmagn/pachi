# Hex Grid System Design Brief

Board elements need to be placeable on a hex grid.  The HexGrid class will hold
all data stored by that grid, such as which placeables are allowed in which region.

## Objective

When playing cards, I want to give the player a sense of customization over the
board.  Currently everything is in a fixed slot, but it would give a better
sense of control if they could carefully place each board element.

## Requirements

- players can place pockets at bottom of level
- players can place spinners and pin groups anywhere in the level
- players cannot place anything outside of the level

## Design

### HexGrid

`HexGrid` is responsible for storing a scene's grid information, as well as
drawing.  It tracks things like the placement permissions for each tile, and
which tiles map to which placement groups.

Placement permissions are tracked using a bit-packed byte.

### HexFootprint

`HexFootprint` is a resource defining the hex footprint of a placeable element,
as well as its needed placement permissions.

### HexEditorPlugin

`HexEditorPlugin` facilitates tooling around easily editing the hex grid.
