# Agent Handoff — Blish HUD MIDI Control

## Current State

Latest release: **v0.0.4** (released 2026-05-24).

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

### v0.0.5 Plan — Custom JSON Keymaps

| Chunk | Status | Feature | Notes |
|---|---|---|---|
| 1 | — | **JSON keymap schema** | Define contract: `id`, `name`, `autoOctaveSwap`, `octaveDownKey`, `octaveUpKey`, `notes` array or dict. |
| 2 | — | **Custom keymap loader** | `KeymapRegistry` discovers `.json` files in module data directory, deserializes via Newtonsoft.Json, appends to `AllKeymaps`. |
| 3 | — | **Error handling** | Catch parse failures, log warnings, skip malformed files. Surface a count of load failures in UI if feasible. |
| 4 | — | **Settings dropdown refresh** | `MidiSettingsView` or `MidiModule` should refresh keymap dropdown when custom files appear/disappear. |
| 5 | — | **Documentation** | Add a README / wiki section explaining the JSON keymap format with examples. |

### Deferred / Future (v0.0.6+)
- **Frame Drum Auto** — 1-5 percussion sounds
- **Drum Kit** — Research typical MIDI finger-drumming layouts; map to Frame Drum's 5 percussion sounds
- Better visualization of keymap previews
- `noteoff` support / true key-down key-up hold behavior
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Better handling of 'extra' keys that activate loops, recording, or chords
