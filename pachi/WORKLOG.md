# Work Log

## 2026-09-05

Working on hex grid prototype

Self-contained scene to figure out how the hex grid should work

### Design

- players can place pockets and pegs in the hex grid
    - this can just be like a hotkey and a label for now
- I need to be able to paint the valid regions the pockets and pegs can be placed in from the editor
    - in-editor tooling?
    - shape overlaps?
    - the ideal solution would be some sort of in-editor tooling which allows me to paint different "placement groups"
- does data live on hex tile, or in hex grid?
    - probably easier and more idiomatic if it lives in the hex tile.
- hex grid can maintain a list of hex tiles if it needs to (but it might not need to if we push the logic down to the tile level)

