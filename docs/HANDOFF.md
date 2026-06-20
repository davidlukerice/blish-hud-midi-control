# Agent Handoff — Blish HUD MIDI Control

## Current State

Latest release: **v0.1.0** (released 2026-05-31).

### Core Implementation
- Domain model: `NoteDefinition`, `Keymap`, `KeySendResult`
- `KeymapRegistry` with built-in keymaps (15 total including new Black Lion Drum and Frame Drum), `KeymapPreviewFormatter`
- `SendInput` P/Invoke, `KeyToScanCode` (forward + reverse lookup), `KeySendThread`
- `MidiInputManager` with auto-reconnect every 10s; recognizes true Note Off as well as Note On (velocity 0)
- `MidiEventConverter` pure helper for NAudio `MidiEvent` → module `MidiNoteEvent`
- `KeySender` with `NoteProcessed` event, `Resolve()` pure function, Key Hold mode with per-note scan-code tracking
- `ConnectionEvaluator` (pure, tested)
- `MidiSettingsView` with compact layout: device/keymap dropdowns, keymap preview, toggles, delay slider, live recent-send log, Key Hold checkbox, Release All Keys panic button
- `TabbedWindow2` settings panel with event cleanup on unload
- Toggle keybind, corner icon states (green/gray/orange), focus guard
- Blish HUD 1.3.0, post-build xcopy to modules folder

### Design Decisions
See [`docs/design-decisions.md`](design-decisions.md) for the historical record of significant design decisions.

### Deferred / Future
- Better visualization of keymap previews
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Better handling of 'extra' keys that activate loops, recording, or chords

## Current Session — Keybed Layout Preview — Chunk A Complete

**Chunk A** (domain model and calculator) is implemented, built, and all new tests pass.

### What was done
- `MidiNote.TryParseNoteName(string, out int)` — parses natural, sharp, flat, and enharmonic note names; rejects invalid input.
- `KeybedKey` — immutable data shape for a single rendered piano key.
- `KeybedLayout` — container for the full layout with `StartOctave`, `EndOctave`, and `IsEmpty`.
- `KeybedLayoutCalculator.Calculate(Keymap)` — pure function that turns a `Keymap` into a `KeybedLayout`:
  - Full octave span from lowest to highest mapped octave.
  - Empty intermediate octaves included.
  - Black/white key identification.
  - Key-switch detection (`OctaveDownKey` / `OctaveUpKey`).
  - Alt-octave info preserved.
  - Original note name from the keymap preserved (e.g. `Bb3` stays `Bb3`, not normalized to `A#3`).
- Tests:
  - `MidiNoteTests.TryParseNoteName_*` (8 tests)
  - `KeybedLayoutCalculatorTests` (9 tests)
- Also added `SkipCopyBhmToModules` condition to the `.csproj` post-build target to avoid xcopy sharing-violation errors while Blish HUD is running.

### Files changed
- `src/Core/MidiNote.cs`
- `src/Keymaps/Visualization/KeybedKey.cs`
- `src/Keymaps/Visualization/KeybedLayout.cs`
- `src/Keymaps/Visualization/KeybedLayoutCalculator.cs`
- `tests/Core/MidiNoteTests.cs`
- `tests/Keymaps/Visualization/KeybedLayoutCalculatorTests.cs`
- `Blish HUD - MIDI Control.csproj`
- `tests/DavidRice.BlishHud.MidiControl.Tests.csproj`

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures**: these error with `FileNotFoundException` for `Blish HUD.dll` because the assembly reference in the main `.csproj` has `<Private>False</Private>`, so Blish HUD dependencies are not copied to the test output directory. Fixing this copy-local behavior is out of scope for the keybed feature but should be addressed so the full test suite is green.
