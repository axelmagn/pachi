- When creating or modifying code files, ALWAYS create an artifact for the 
  developer to review.
- When checking for error conditions such as null values, PREFER to use asserts
  for unexpected edge cases.  If there are valid reasons to check for an edge
  case with an `if` clause, add a comment that describes when we expect this
  edge case to occur.
- PREFER to assert simplifying assumptions rather than covering all edge cases
- ASSERT that nodes are always correctly configured prior to insertion into the
  tree.
- Use packed scenes to instantiate manually configured nodes.
- If an `if` is used for an edge case guard clause, PREFER guard clauses that
  exit early rather than nesting.
