---
name: shape
description: Interview the user about a game feature idea, then create a pitch card in the Pitches list of the "Pachi Agent" Trello board from the pitch template card (playbook B2). Use when the user wants to shape, pitch, or write up a feature idea.
argument-hint: "[inbox/idea card URL or a short description]"
---

Shape a pitch (playbook B2). Input: $ARGUMENTS

1. **Evidence.** If given a card, read it (description, comments, attachments).
   Otherwise ask what playtest moment or feeling motivates this.
2. **Template.** Read the card "TEMPLATE: Pitch" in the Pitches list
   (https://trello.com/c/P4DdyhfQ). Its sections are the interview outline.
3. **Interview.** One or two questions at a time, until you could fill every
   section. Push for specifics: the emotion, the appetite (timebox), the
   no-gos, and the cut list. Don't invent answers; mark unknowns `TODO`.
   Read docs/design/ first so you don't ask about settled rules.
4. **Rabbit holes.** List each technical risk. Propose a spike card for each
   (don't create them yet).
5. **Draft.** Show the full card description. Experience goal, appetite, and
   no-gos must use the user's own words. Budget ≤150 lines.
6. **Create** on approval: card `<slug> — <slogan>` in Pitches, description
   from the draft; link the evidence card in a comment. Create the approved
   spike cards in Spikes, labelled `spike`.
7. **ADRs.** If the interview settled an architectural decision, offer a
   docs/decisions/NNNN-title.md (template: 0000-template.md).

Never move a pitch into a Cycle list; betting is the user's call.
