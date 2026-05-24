# Agent Handoff — Blish HUD MIDI Control

## Current State

Latest release: **v0.0.3** (released 2026-05-24).

All v1 PRD user stories are implemented. The module has gained additional built-in keymaps, a live Recent Sends debug log, and richer diagnostics. **147 tests passing.**

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

| Chunk | Instrument(s) | Notes |
|---|---|---|
| 1 | **Lute Auto**, **Harp Auto**, **Horn Auto** | 3-octave C Major natural, 9/0 shifts. Structurally identical; copy Minstrel Auto minus sharps/force-octave. |
| 2 | **Verdarach Auto** | 3-octave C Major natural. Same structure as chunk 1, but separate for legendary novelty skin gating. |
| 3 | **Bass Guitar Auto** | 2-octave C Major natural (starting octave TBD). |
| 4 | **Drum Kit** | Research typical MIDI finger-drumming layouts; map to Frame Drum's 5 percussion sounds. |

### Deferred / Future
- Custom JSON keymap loading from data directory
- `noteoff` support / true key-down key-up hold behavior
- Floating overlay indicator showing last played note (outside settings tab)
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Settings persistence for log panel scroll position or log size
