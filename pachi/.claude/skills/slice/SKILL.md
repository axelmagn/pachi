---
name: slice
description: Break a pitch card on the "Pachi Agent" Trello board into vertical ticket cards in the Appendix D format, each with the B14 definition-of-done checklist (playbook B3). Use when the user asks to slice a pitch into tickets.
argument-hint: "<pitch card URL or name>"
---

Slice a pitch into tickets (playbook B3). Pitch: $ARGUMENTS

1. **Read** the pitch card: description, comments, checklists. Read the
   ticket template card (https://trello.com/c/JRP9I694) and the B14 checklist
   in docs/process/playbook.md. Skim docs/design/ and the code the scopes touch.
2. **Propose** a ticket list before creating anything. Each ticket:
   - is **vertical**: yields something seen or tested in the game;
   - is **small**: one session, ≤ ~400 changed lines, ≤ ~6 files (else split);
   - has acceptance criteria tagged `[core] | [engine] | [visual] | [you]`,
     no-gos (inherit the pitch's), a pattern to follow, and a named check;
   - is ordered uphill-first (unknowns before known work).
   Show the list as a table (title, scope, hill, size, layers) and wait for
   approval.
3. **Create** each approved ticket in the active `Cycle: <slug>` list (ask if
   there isn't one): title `<pitch-slug>#<n> — <verb phrase>`, description in
   the template format with `**Pitch:**` linking the pitch card. Attach labels
   for layer, hill (⬆ uphill / ⬇ downhill), and size (S / M). Add a checklist
   "Definition of done" with the B14 items.
4. **Link back:** add a checklist "Tickets" on the pitch card with one item
   per ticket card URL.

Don't change the pitch's appetite, no-gos, or experience goal. If a scope
doesn't fit the appetite, say so instead of slicing it.
