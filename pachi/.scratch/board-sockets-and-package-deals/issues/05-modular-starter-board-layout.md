# 05: 2x3 Modular Starter Board Layout in Level.tscn

**What to build:** Refactor `Level.tscn` to use the engineered 2-row $\times$ 3-column modular grid with 6 `Socket2D` mounting positions (1 center Yakumono, 2 flanking Spinners, 3 bottom Beetle Pockets) pre-populated with WYSIWYG starter component child instances. Verify playfield boundary containment, launcher drop trajectories, and runtime starter component adoption.

**Blocked by:** 01: Socket2D Node Contract, Category Validation & Lifecycle Signals, 03: Component ISocketComponent Integration & Safe Ball Flush Protocol

**Status:** resolved

- [x] Top socket row in `Level.tscn` is positioned with `SocketSpinnerLeft` (-110, -40), `SocketYakumonoCenter` (0, -40), and `SocketSpinnerRight` (110, -40).
- [x] Bottom socket row in `Level.tscn` is positioned with `SocketPocketLeft` (-115, 130), `SocketPocketCenter` (0, 130), and `SocketPocketRight` (115, 130).
- [x] Sockets contain pre-configured starter child component scenes (`Yakumono`, `Spinner`, `Pocket`) for in-editor WYSIWYG preview and automatic runtime adoption.
- [x] Playfield boundary, launcher hole positioning, and drain zone seamlessly integrate with the new 6-socket layout without ball traps or physics deadzones.
- [x] Level startup tests verify all 6 sockets initialize with active `CurrentComponent` instances and valid collision geometry.
