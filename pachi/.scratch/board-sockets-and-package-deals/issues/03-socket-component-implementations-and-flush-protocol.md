# 03: Component ISocketComponent Integration & Safe Ball Flush Protocol

**What to build:** Implement `ISocketComponent` across all modular component scenes (`Pocket`, `Spinner`, `Yakumono`), package deflection pins directly inside component scene hierarchies as integrated obstacles, and implement `FlushActiveBalls(Action<BallVariant>)` to safely evacuate trapped/queued ball variants without state loss.

**Blocked by:** 01: Socket2D Node Contract, Category Validation & Lifecycle Signals

**Status:** ready-for-agent

- [ ] `Pocket` implements `ISocketComponent`, reporting category `BeetlePocket`, local spatial bounds, and `FlushActiveBalls` callback that extracts held/in-transit balls.
- [ ] `Spinner` implements `ISocketComponent`, reporting category `Spinner`, local spatial bounds, and `FlushActiveBalls` callback.
- [ ] `Yakumono` implements `ISocketComponent`, reporting category `Yakumono`, local spatial bounds, and `FlushActiveBalls` callback that extracts any queued or held balls.
- [ ] Modular component scenes bundle integrated deflection pins and funnels within their own scene hierarchies rather than relying on external board pins.
- [ ] `BallAwardIndicator` and `PocketBallsIndicator` display discrete pip clusters accurately on modular component instances.
- [ ] Headless unit tests verify component category reporting, bounds queries, and exact ball variant recovery during flush callbacks.
