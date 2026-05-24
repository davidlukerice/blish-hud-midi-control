# Agent Handoff — Blish HUD MIDI Control

## Current State

Latest release: **v0.0.3** (released 2026-05-24).

### Core Implementation
- Domain model: `NoteDefinition`, `Keymap`, `KeySendResult`
- `KeymapRegistry` with built-in keymaps, `KeymapPreviewFormatter`
- `SendInput` P/Invoke, `KeyToScanCode` (forward + reverse lookup), `KeySendThread`
- `MidiInputManager` with auto-reconnect every 10s
- `KeySender` with `NoteProcessed` event, `Resolve()` pure function
- `ConnectionEvaluator` (pure, tested)
- `MidiSettingsView` with compact layout: device/keymap dropdowns, keymap preview, toggles, delay slider, live recent-send log
- `TabbedWindow2` settings panel with event cleanup on unload
- Toggle keybind, corner icon states (green/gray/orange), focus guard
- Blish HUD 1.3.0, post-build xcopy to modules folder

### v0.0.4 Plan — Additional Built-in Keymaps

| Chunk | Status | Instrument(s) | Notes |
|---|---|---|---|
| 1 | ✅ **Complete** | **Lute Auto**, **Harp Auto**, **Horn (C) Auto**, **Horn (E) Auto** | Lute/Harp/Horn(C) are 3-octave C Major natural (9/0 shifts). Horn(E) is 2-octave E Major (both shifts on 9), discovered during manual testing that the in-game Horn plays in E. |
| 2 | ✅ **Complete** | **Verdarach Auto** | 3-octave C Major natural. Same structure as chunk 1, but separate for legendary novelty skin gating. |
| 3 | ✅ **Complete** | **Bass Guitar Auto** | 2-octave C Major natural starting on C3 (15 notes: C3–C5). Octave 2 is unmapped; a comment documents the action keys (loops 1–8, return-to-octave-1 on 9, tempo lock on 0). |
| 4 | — | **Frame Drum Auto** | 1-5 percussion sounds |
| 5 | — | **Drum Kit** | Research typical MIDI finger-drumming layouts; map to Frame Drum's 5 percussion sounds. |

### Deferred / Future
- Custom JSON keymap loading from data directory
- `noteoff` support / true key-down key-up hold behavior
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Better handling of 'extra' keys that activate loops, recording, or chords
