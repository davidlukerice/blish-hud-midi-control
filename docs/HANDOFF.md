# Agent Handoff — Blish HUD MIDI Control

## Where We Are

- **Platform locked**: Blish HUD (not Nexus). Rationale in `CONTEXT.md` — direct `SendInput` needed, only Blish HUD allows this.
- **Design grilling complete**: All architecture, data model, UI, threading, and error-handling decisions resolved.
- **PRD published**: `docs/MIDI-Control-v1-PRD.md` (status: `ready-for-agent`).
- **Glossary published**: `CONTEXT.md` — canonical terms. If the next agent uses different terminology, challenge them against this file.
- **Chunk 1 complete**: `NoteDefinition` and `Keymap` domain models implemented with red/green TDD.

## Current Codebase State

```
E:/dev/blish-hud-midi-control/
├── Module.cs                                        ← empty scaffold, needs rewrite
├── manifest.json                                    ← updated url and contributors
├── Blish HUD - MIDI Control.csproj                  ← net48, LangVersion 8.0, ref assemblies wired
├── Blish HUD - MIDI Control.sln                     ← includes test project
├── packages.config
├── packages/                                        ← now includes NUnit, NUnitLite, net48 ref assemblies
├── README.md
├── CONTEXT.md                                       ← glossary
├── docs/
│   ├── MIDI-Control-v1-PRD.md
│   ├── implementation-plan.md
│   ├── HANDOFF.md                                   ← this file
│   └── agent-instructions.md                        ← scope discipline rules
├── src/
│   └── Keymaps/
│       └── NoteDefinition.cs                        ← Keymap + NoteDefinition (Chunk 1 DONE)
├── tests/
│   ├── DavidRice.BlishHud.MidiControl.Tests.csproj
│   ├── Program.cs                                   ← NUnitLite entry point
│   └── Keymaps/
│       ├── NoteDefinitionTests.cs
│       └── KeymapTests.cs                           ← all 8 tests pass
```

## Decisions Already Made (Do Not Re-litigate)

| Topic | Decision |
|---|---|
| Platform | Blish HUD (not Nexus) |
| Language | C# / .NET Framework 4.8 |
| MIDI library | NAudio (`NAudio.Midi.MidiIn`) |
| Key sending | Raw P/Invoke `SendInput` with scan codes |
| Note behavior | `KeyTap` only (`noteon` → down+up back-to-back); `noteoff` deferred |
| Threading | `MidiInputManager` background thread → `ConcurrentQueue` → `Module.Update()` → `KeySender` → `KeySendThread` (`BlockingCollection`) |
| First built-in keymap | **The Minstrel (Auto)** — most complete, exercises full logic |
| Settings | Single tab: MIDI device dropdown + refresh, keymap dropdown + preview, standard toggles |
| Custom keymaps | **Follow-up** — out of scope for v1 |
| Corner icon | Active/muted states only (no note text) |
| Focus guard | Optional setting, uses `GameService.GameIntegration.Gw2Instance.IsInGame` |
| Tests | Unit tests for `KeySender` and `KeymapRegistry`; integration for `KeySendThread` |
| Namespace | `DavidRice.BlishHud.MidiControl` (was `Blish_HUD___MIDI_Control`) |
| Build | `msbuild "Blish HUD - MIDI Control.sln" -p:Configuration=Debug -p:Platform=x64` |
| Test runner | `tests/bin/x64/Debug/DavidRice.BlishHud.MidiControl.Tests.exe --noheader` |

## Source Files to Create (Remaining)

```
src/
├── Keymaps/
│   ├── NoteDefinition.cs                              DONE
│   ├── BuiltIn/
│   │   └── MinstrelAutoKeymap.cs                      (next chunk candidate)
│   └── KeymapRegistry.cs                              (next chunk candidate)
├── Input/
│   ├── MidiInputManager.cs                            (Phase 3 — needs NAudio)
│   └── SendInput.cs                                   (Phase 4 — P/Invoke wrapper)
├── Core/
│   ├── KeySender.cs                                   (Phase 5)
│   └── KeymapRegistry.cs                              (choose one location)
└── UI/
    └── SettingsView.cs                                (Phase 6)
```

> **Decision needed**: `KeymapRegistry` should live under `src/Keymaps/` (data/discovery) rather than `src/Core/`. Both the implementation plan and the handoff listed it in both places — clean this up.

## What to Change in Existing Files (Remaining)

### `manifest.json`
- `"version"` to bump when releasing

### `Module.cs`
- Complete rewrite. Must:
  - `DefineSettings()` with all 6 setting entries
  - `Initialize()` / `LoadAsync()` wiring up all subsystems
  - `Update()` draining queue
  - `Unload()` cleanup + unload safety (key-up flush)
  - Register corner icon + keybind

### `.csproj`
- Add NAudio (when MIDI code is implemented)
- Remove stale packages (`AsyncClipboardService`)
- (Optional) Migrate from `packages.config` to `PackageReference`

## Keymap Reference (The Minstrel Auto)

Port from the original TypeScript in `midi-to-game-instruments` repo at:
`/packages/main/src/modules/KeyHandler/defaultKeyMaps/gw2_minstrel_auto.ts`

Key structures:
- Notes C3–B3 → keys `1`–`7`, octave 0
- C4 → key `1`, octave 1, altOctave 0, altOctaveKey `8`
- C5 → key `8`, octave 1, altOctave 2, altOctaveKey `1`
- Notes D4–B5, D5–C6 → keys `2`–`8` across octaves 1 and 2
- Sharp notes `C#4`, `D#4` → manual octave shift keys `9`, `0`
- Sharp notes `F#4`, `G#4`, `A#4` → forceInternalOctave 0, 1, 2
- `octaveDown: { key: '9' }`, `octaveUp: { key: '0' }`

## Important Gotchas

1. **Scan codes, not virtual keys**: `SendInput` must use `KEYEVENTF_SCANCODE`. Virtual keys fail with GW2 DirectInput.
2. **Thread safety**: `MidiInputManager` event handler runs on a background thread. All data passed to the game thread must go through `ConcurrentQueue`. Never touch Blish HUD UI state from the MIDI callback.
3. **KeyTap vs KeyUp safety**: On unload, send key-up for all possible keys (`1-8`, `9`, `0`) to prevent stuck states. `KeySender` produces `KeyTap` actions with zero delay for normal notes.
4. **Copy Local = False**: All Blish HUD and MonoGame references must have `Copy Local = False`. Otherwise the module DLL bloats with assemblies Blish HUD already has loaded.

## Testing Strategy (Reiterated from PRD)

- **KeySender**: Pure unit tests. Feed `(note, currentOctave, keymap)` → assert `(SendAction[], newOctave)`.
- **KeymapRegistry**: Unit tests for lookup and built-in discovery.
- **KeySendThread**: Integration test for enqueue/dequeue/shutdown lifecycle.
- No unit tests for `SendInput` (hardware), `MidiInputManager` (requires MIDI device), or Blish HUD UI controls.

## Chunk History

| # | Description | Tests | Status |
|---|---|---|---|
| 1 | Domain model: `NoteDefinition`, `Keymap` | 8 passing | DONE |

## Next Chunk Options

1. **MinstrelAutoKeymap.cs** — port the TypeScript data into a static class that returns a populated `Keymap`. Adds no new dependencies. Low risk, high value — validates the domain model against real data.
2. **KeymapRegistry** — keys, alt-octaves, and manual shift bindings. Pure C#, unit-testable.

Ask the user which to pick up first.
