# Brief: <REQUIRED_STRING: Feature Name, max 60 chars>

Status: <ENUM: needs-triage | needs-info | ready-for-agent | ready-for-human | wontfix>

## 1. Intent & Player Experience
<!-- 
INSTRUCTION:
- Write 1-2 concise paragraphs explaining what this feature adds to the game loop from the player's perspective.
- Focus on gameplay feel, player agency, and core motivation. Avoid implementation jargon.
EXAMPLE:
Players can preview the trajectory of their launched ball while holding down the launch button. This gives players strategic control over pin rebounds, making shots feel intentional rather than purely random.
-->
<REQUIRED_TEXT: 1-2 paragraphs describing player-facing intent and game feel>

## 2. Rough Scope & Key Mechanics
<!-- 
INSTRUCTION:
- Identify affected game domains and subsystems (e.g., Hopper, Launcher, Cards, Pockets, Pins, VisualConfig).
- State explicitly what is within scope and what is explicitly deferred or out of scope.
EXAMPLE:
- **In Scope**:
  - Trajectory line drawing using a RayCast2D prediction loop in Launcher.
  - Visual styling tied to `VisualConfig.tres` trajectory color property.
- **Out of Scope**:
  - Spin and curve ball physics calculations (deferred to a later milestone).
-->
### In Scope
<REQUIRED_LIST: Bulleted list of components and systems touched>

### Out of Scope
<!-- If nothing is explicitly excluded, write "None." Do not delete this subsection. -->
<REQUIRED_TEXT: Excluded items or "None.">

## 3. Open Questions & Uncertainties
<!-- 
INSTRUCTION:
- List unresolved questions regarding mechanics, edge cases, state transitions, or visual UI details.
- These questions serve as the starting frontier for `/grill-with-docs`.
- If no open questions remain, write "None identified." Do not delete this section.
EXAMPLE:
- Should the trajectory line fade out after a certain bounce count?
- How does the prediction handle dynamic obstacles like moving pocket arms?
-->
<REQUIRED_LIST: Bulleted questions or "None identified.">

<!-- 
AGENT CHECKLIST:
- [ ] Did you replace all `<...>` placeholders with concrete project details?
- [ ] Is the Status set to a valid triage label from docs/agents/triage-labels.md (defaulting to needs-triage)?
- [ ] Are in-scope and out-of-scope boundaries clearly distinguished?
- [ ] Did you preserve all headers and fallback text for empty optional sections?
-->
