# 02: Package-Deal Card Resource Model & Discrete Tier Ball Cost

**What to build:** Implement the `PackageDealCard` custom `Resource` class (`[Tool]`, `[GlobalClass]`) containing card descriptive metadata, target `SocketCategory`, target `PackedScene` component, discrete ball cost properties (`BallCostCount` 1–4, `BallCostTier` 1–4), and shop draft weight. Create baseline card resource assets and headless unit tests.

**Blocked by:** 01: Socket2D Node Contract, Category Validation & Lifecycle Signals

**Status:** ready-for-agent

- [ ] `PackageDealCard` resource class is implemented with `CardId`, `Title`, `Description`, `Icon`, `AccentColor`, `Category`, `ComponentScene`, `BallCostCount`, `BallCostTier`, and `DraftWeight`.
- [ ] Property setters and validation logic enforce discrete ball cost constraints (count 1–4, tier 1–4).
- [ ] Baseline `.tres` package-deal card resources are created for starter Pocket, Spinner, and Yakumono archetypes.
- [ ] Automated headless test suite `PackageDealCardTests.cs` verifies resource property constraints, tier cost integrity, and scene references, registered in `TestRunner.cs`.
