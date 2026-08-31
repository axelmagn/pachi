# 01: Package-Deal Socket Component Replacement Architecture

Type: grilling
Status: resolved
Blocked by: none

## Question

How should the package-deal card model structure component archetypes (Pocket cards, Pin Block cards, Spinner cards) to cleanly overwrite designated sockets on the board, how do card data resources represent complete self-contained components, and how does the replacement lifecycle clean up previous node states?

## Answer

1. **Card Architecture & Representation**:
   - Cards are self-contained `CardData` resources representing whole package-deal components holding a `PackedScene` reference for their physical node tree.
   - Distinct archetypes inherit from `CardData`: `PocketCardData`, `PinBlockCardData`, `SpinnerCardData`, and `YakumonoCardData`.
   - Each card defines a mandatory `SocketCategory` enum (`Pocket`, `PinBlock`, `Spinner`, `Yakumono`), cost tier (1–4), and ball cost quantity (1–4).

2. **Socket Lifecycle & Cleanup**:
   - All board sockets are pre-populated with starter baseline components at run initialization.
   - When a new package-deal card is installed into an eligible socket:
     - Any balls currently trapped or in the middle of processing in the old component are immediately paid out / refunded to the active hopper.
     - Active tweens, physics processes, and signal connections on the outgoing component are terminated.
     - The old component instance is removed and freed (`QueueFree()`).
     - The new component scene is instantiated from the card's `PackedScene`, added as a child of the Socket, and initialized with any card parameters.
     - An install event / visual burst is dispatched.

3. **Socket Compatibility**:
   - Strict 1:1 category matching between card `SocketCategory` and board socket types. Sockets cannot accept mismatched card archetypes.
