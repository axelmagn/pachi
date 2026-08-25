# 02: Yakumono Face State Machine & Ball Interaction

**What to build:** Dynamic face cycling behavior for Yakumono. On catching a ball, the character face transitions to a new random face from the configured face textures array and emits state change notifications. On completing a jackpot payout, it transitions to a dedicated Jackpot face state and emits payout notifications.

**Blocked by:** 01: VisualConfig Settings, GlobalEvents & Yakumono Core Visual Class

**Status:** ready-for-agent

- [ ] `Yakumono` maintains an internal face state machine indexing available face graphics.
- [ ] Catching a ball (`OnBallCatch`) transitions `Yakumono` to a random face state index from `FaceTextures` (excluding current index) and fires `GlobalEvents.NotifyYakumonoStateChanged`.
- [ ] Completing a payout (`NotifyCentralPocketPaidOut`) transitions `Yakumono` to the reserved Jackpot face state and fires `GlobalEvents.NotifyYakumonoPaidOut`.
- [ ] C# headless unit tests verify state transitions, random selection bounds, and signal dispatches.
