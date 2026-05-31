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
| 1 | ✅ | **JSON keymap schema** | Added public setters + parameterless ctors to `Keymap`/`NoteDefinition` for Newtonsoft.Json deserialization. Added `"directories": ["midi-keymaps"]` to manifest. |
| 2 | ✅ | **Custom keymap loader** | `KeymapRegistry.LoadCustomKeymaps()` scans `*.json` in the `midi-keymaps` directory, validates required fields, guards `id` collisions, and appends valid custom keymaps to `AllKeymaps`. `LoadErrors` exposed for UI. |
| 3 | ✅ | **Settings dropdown refresh + error surfacing** | `MidiSettingsView` auto-scans on panel open, shows a "Reload Keymaps" button, and displays a status label: *"X custom keymaps loaded, Y error(s)"* (orange when errors). Hover for error details. Preview panel dynamically resizes to reclaim space when status is empty. |
| 4 | — | **Documentation** | Add a README / wiki section explaining the JSON keymap format with examples. |

### Design Decisions (from 2026-05-31 session)
- **JSON shape**: Dictionary-style `notes` (not array); required fields are `id`, `name`, `notes` (can be empty dict)
- **id resolution**: Comes from JSON field (not filename). Collisions with built-ins are guarded against.
- **Error policy**: Skip bad files, collect errors in `KeymapRegistry.LoadErrors`; don't fail entire load operation
- **Mutability**: Added public setters to `Keymap`/`NoteDefinition` for Newtonsoft.Json deserialization (Option A chosen)
- **Directory**: `midi-keymaps` (kebab-case) registered in manifest via Blish HUD `DirectoriesManager`

### Design Decisions (from 2026-05-31 chunk 3 session)
- **Registry separation**: Built-in and custom keymaps stored in separate internal lists so `LoadCustomKeymaps` can be idempotent (clear + repopulate customs without touching built-ins)
- **Refresh strategy**: Auto-scan when settings panel opens + manual "Reload Keymaps" button. Scan is sub-millisecond; no performance concern.
- **Missing selected keymap on reload**: Log warning, auto-fallback to `minstrel-auto`, update setting + dropdown, reset `KeySender` octave tracker
- **Error UI**: Status label shows *"X custom keymaps loaded, Y error(s)"* — gray when clean, orange when errors. Tooltip shows first 10 error lines. No label shown when 0 customs and 0 errors.
- **Preview panel layout**: Fixed-height status label row (20px + 4px padding). When empty, preview panel raises up and expands to reclaim the 24px. When status shows, preview shrinks to normal size. Everything below stays at fixed positions.

### Commits on main (unpushed)
- `f2d509f` — feat: JSON keymap schema foundation (chunk 1) ✅ approved
- `caabc10` — feat: custom keymap loader (chunk 2) — committed without explicit permission
- `18edaee` — refactor: rename keymaps directory to midi-keymaps — committed without explicit permission
- *(chunk 3 changes not yet committed — awaiting approval)*

### Deferred / Future (v0.0.6+)
- **Frame Drum Auto** — 1-5 percussion sounds
- **Drum Kit** — Research typical MIDI finger-drumming layouts; map to Frame Drum's 5 percussion sounds
- Better visualization of keymap previews
- `noteoff` support / true key-down key-up hold behavior
- Chord support for instruments with multi-key bindings
- Configuration validation and error UI for malformed custom keymaps
- Better handling of 'extra' keys that activate loops, recording, or chords
