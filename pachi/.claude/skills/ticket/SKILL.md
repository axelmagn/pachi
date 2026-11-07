---
name: ticket
description: Orient on a ticket card from the "Pachi Agent" Trello board before building it (playbook B4 orient). Loads the card spec, comments, attachments, and linked pitch, then runs the fast check. Use when the user names a card to work on.
argument-hint: "<card URL, short link, or name>"
---

Orient on a ticket (playbook B4 step 2). Card: $ARGUMENTS

1. **Find the card.** URL/short link → read it directly. A name like
   `beetles#3` → search cards on the "Pachi Agent" board.
2. **Load:**
   - the description in full (it's the spec);
   - comments: the latest 5 in full, older ones summarised in one line each
     (paginate if there are more);
   - the attachments list (names + URLs);
   - checklists, labels, and current list;
   - the linked pitch card (the `**Pitch:**` line): its experience goal,
     appetite, and no-gos only.
3. **Print the spec** for the user: title, list, labels, acceptance criteria
   with layers, no-gos, pattern to follow, check, repo links, then the
   comment digest and anything unresolved in it.
4. **Run** `./scripts/check.sh` and report the tail of its output. Red means
   stop and report; don't start the ticket on a red gate.
5. **Move** the card to "In progress (WIP 1)" only if the user says to start.
   If another card is already there, say so instead of moving.

Then wait. The next step is a plan (B4 step 3), posted as a card comment
once the user approves it.
