# Agent Instructions

## Scope Discipline

Implementation proceeds **one logical chunk at a time**. Never implement ahead of the current chunk. Each chunk must be reviewed, committed, and accepted by the user before starting the next.

## Chunk Boundaries

A chunk is complete when it can be **built and meaningfully verified**. Chunks are the smallest unit of work trust, not the smallest set of files.

| Anti-pattern | Correct approach |
|---|---|
| Adding NAudio to `.csproj` before we have code that uses it | Create models first, wire dependencies at the chunk that needs them |
| Creating empty scaffold classes for all planned files | Only create files justified by the current test or compilation need |
| Implementing a dependency because you know a future chunk will need it | Wait until that future chunk's tests demand it |

## TDD Policy

Where feasible, implement with red/green TDD:
1. Write a failing test that describes the desired behavior.
2. Write the minimal code to make it pass.
3. Refactor if needed.
4. Repeat.

## Handoff and Planning

When resuming work, always:
1. Read the handoff file first (`docs/HANDOFF.md`).
2. Read `CONTEXT.md` for canonical domain terminology.
3. Ask the user what the next chunk should be rather than assuming.
4. Never "pick up from where we left off" by silently continuing — get explicit confirmation.
