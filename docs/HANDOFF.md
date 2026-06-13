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
