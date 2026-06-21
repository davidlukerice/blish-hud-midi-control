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

## Current Session — Keybed Hover Highlight + Tooltip — Chunk E

### What was done

- `KeybedControl` — Added **hover highlighting** and **native tooltip** support:
  - Tracks `_hoveredKey` by overriding `OnMouseMoved`, `OnMouseEntered`, `OnMouseLeft`.
  - Uses `RelativeMousePosition` (Blish HUD scroll-aware) to detect which key rect the cursor is over.
  - `_keyRects` dictionary rebuilt on `Layout` change for fast hit-testing.
  - **Hover overlay**: semi-transparent orange tint (`Color(255,165,0,77)`) for mapped keys, blue-gray (`Color(176,196,222,77)`) for unmapped — drawn in a third pass *after* keys.
  - **Native tooltip** (`BasicTooltipText`) updated dynamically per key, floats above all controls, follows cursor via Blish HUD's tooltip system.
  - **Tooltip content**:
    - Unmapped: `C#4 (unmapped)`
    - Mapped: `C4\r\nGW2 Key: 1\r\nOctave: 1`
    - Alt octave: appends `Also plays as 8 on octave 0`
    - Key switch: `C4\r\nOctave shift (Key: 9)`
  - Added `using Blish_HUD.Input;` for `MouseEventArgs`.

### Files changed
- `src/UI/KeybedControl.cs`

### Build / Tests
- Build: success (0 errors, 1 pre-existing warning).
- No new regressions.

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures** — `Newtonsoft.Json` `FileNotFoundException` (copy-local issue).
- **Window width auto-sizing** to fit the full keybed without scrolling (alternative to TrackBar approach).
