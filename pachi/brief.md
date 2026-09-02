This project is at a good point to consider refactors that improve code quality
and architecture.  We have had great feature velocity recently, but I fear that
there is a fair amout of cruft and bitrot that has accumulated.

Some focus areas:

- our testing is pretty ad-hoc.  I don't think we ever really thought through
how testing should work at the macro level.
- The whole visual config / visual showcase system sort of sucks.  It
introduces a lot of complexity at the editor level, and is a frequent cause of
bugs.  The visual showcase has diverged significantly from the game, because it
duplicates effort whenever there's a visual change.  I sort of want to just
throw the whole thing out.
- the docs directory is almost entirely LLM-generated context that is itself
consumed by the LLM.  A lot of the info in there is stale or noisy, and pretty
much everything but the playtest notes should be discarded.

I'm sure there are more opportunities to improve than just this area, so make
sure you cover them, but do not over-index on them.

Do a deep audit of the project's code and architecture.

- Identify significant technical risks
- Identify areas where code can be significantly simplified or improved via rearchitecture.
- Identify opportunities to delete obsolete code or documentation.
- Identify areas that should be thrown out and reimplemented.

I am willing to sacrifice a small amount of functionality for a large amount of
code simplicity.

Your objective is to produce an artifact that contains architecture improvement
recommendations that can be converted into work tickets.

