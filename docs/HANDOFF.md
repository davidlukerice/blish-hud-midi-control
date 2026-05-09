# Agent Handoff — Blish HUD MIDI Control

## Where We Are

- **Platform locked**: Blish HUD (not Nexus). Rationale in `CONTEXT.md` — direct `SendInput` needed, only Blish HUD allows this.
- **Design grilling complete**: All architecture, data model, UI, threading, and error-handling decisions resolved.
- **PRD published**: `docs/MIDI-Control-v1-PRD.md` (status: `ready-for-agent`).
- **Glossary published**: `CONTEXT.md` — canonical terms. If the next agent uses different terminology, challenge them against this file.

## Current Codebase State

```
E:/dev/blish-hud-midi-control/
├── Module.cs                       ← old empty scaffold, needs full rewrite
├── manifest.json                   ← needs updates (version, url, contributors)
├── Blish HUD - MIDI Control.csproj ← old, needs NAudio + cleanup
├── Blish HUD - MIDI Control.sln    ← existing
├── packages.config                 ← old packages, needs cleanup
├── packages/                       ← stale packages committed, can be restored via NuGet
├── README.md                       ← updated with project overview
├── CONTEXT.md                      ← glossary and decisions
├── docs/
│   ├── MIDI-Control-v1-PRD.md      ← full PRD
│   ├── implementation-plan.md      ← 7-phase implementation plan
│   └── HANDOFF.md                  ← this file
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

## Source Files to Create

```
src/
├── Keymaps/
│   ├── Keymap.cs                   (data model: keymap + notedef)
│   ├── NoteDefinition.cs           (single note mapping)
│   └── BuiltIn/
│       └── MinstrelAutoKeymap.cs   (ported from original TS)
├── Input/
│   ├── MidiInputManager.cs         (NAudio lifecycle, device enum, queue)
│   └── SendInput.cs                (P/Invoke wrapper: KeyTap, KeyUp)
├── Core/
│   ├── KeySender.cs                (octave logic, alt-octave, produces SendActions)
│   └── KeymapRegistry.cs           (built-in + future custom json discovery)
└── UI/
    └── SettingsView.cs             (custom settings panel with dropdowns + preview)
```

## What to Change in Existing Files

### `manifest.json`
- Fix `"url"` (currently wrong)
- Fix `"contributors"` name/url
- Bump `"version"` when releasing

### `Module.cs`
- Complete rewrite. Must:
  - `DefineSettings()` with all 6 setting entries
  - `Initialize()` / `LoadAsync()` wiring up all subsystems
  - `Update()` draining queue
  - `Unload()` cleanup + unload safety (key-up flush)
  - Register corner icon + keybind

### `.csproj`
- Add NAudio
- Remove stale packages
- Set `Copy Local = False` on Blish HUD deps
- Consider migrating from `packages.config` to `PackageReference`

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
5. **Namespace**: The project uses `davidlukerice.Blish_HUD___MIDI_Control` (with triple underscores) from the generated scaffold. Consider updating to something cleaner like `DavidRice.BlishHud.MidiControl`.

## Testing Strategy (Reiterated from PRD)

- **KeySender**: Pure unit tests. Feed `(note, currentOctave, keymap)` → assert `(SendAction[], newOctave)`.
- **KeymapRegistry**: Unit tests for lookup and built-in discovery.
- **KeySendThread**: Integration test for enqueue/dequeue/shutdown lifecycle.
- No unit tests for `SendInput` (hardware), `MidiInputManager` (requires MIDI device), or Blish HUD UI controls.

## Unresolved Open Questions

- None at architecture level. All major decisions resolved. Next session should start with Phase 1 of `docs/implementation-plan.md`.
