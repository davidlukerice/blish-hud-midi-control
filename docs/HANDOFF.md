# Agent Handoff — Blish HUD MIDI Control

Latest release: **v0.1.0** (released 2026-05-31).

## Current State

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
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Better handling of 'extra' keys that activate loops, recording, or chords

---

## Current Session — Keybed Note Labels — Chunk D

### What was done

- `KeybedControl` — Only **C notes** show octave reference labels (e.g. "C3", "C4"), rendered **below** the keybed rather than inside keys.
  - Added `NoteLabelHeight = 14` and increased `DefaultHeight` from 96 → 110 to accommodate the label row.
  - Key drawing area is now `bounds.Height - KeyPadding * 2 - NoteLabelHeight`.
  - GW2 key label moved back to the **lower 45%** of the white key (as originally designed).
  - C note labels are rendered in a third pass at `y + keyAreaHeight + 2`, centered under each C key in `Color.LightGray` (was `Color.Gray`).
  - Black keys remain unchanged (GW2 key centered, no note name).
- `KeymapLayoutTabView` — Increased keybed panel height from 104 → 118 to fit the taller `KeybedControl`. Info label (`"Octaves n–m | x mapped notes"`) color changed from `Color.Gray` to `Color.LightGray`. Scrollbar auto-adjusts via `y + _keybedPanel.Height + 4`.

### Files changed
- `src/UI/KeybedControl.cs`
- `src/UI/KeymapLayoutTabView.cs`

### Build / Tests
- Build: success (0 errors, 1 pre-existing warning).
- No new regressions.

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures** — `Newtonsoft.Json` `FileNotFoundException` because the assembly reference in the main `.csproj` has `<Private>False</Private>`, so Blish HUD dependencies are not copied to the test output directory.
- **Key hover highlighting** and **click-to-tooltip** with note details.
- **Window width auto-sizing** to fit the full keybed without scrolling (alternative to the TrackBar approach).
