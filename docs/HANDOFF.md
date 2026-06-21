# Agent Handoff — Blish HUD MIDI Control

Latest release: **v0.2.0** (released 2026-06-20).

## Current State

### Core Implementation
- Domain model: `NoteDefinition`, `Keymap`, `KeySendResult`
- `KeymapRegistry` with built-in keymaps (17 total), `KeymapPreviewFormatter`
- `SendInput` P/Invoke, `KeyToScanCode` (forward + reverse lookup), `KeySendThread`
- `MidiInputManager` with auto-reconnect every 10s; recognizes true Note Off as well as Note On (velocity 0)
- `MidiEventConverter` pure helper for NAudio `MidiEvent` → module `MidiNoteEvent`
- `KeySender` with `NoteProcessed` event, `Resolve()` pure function, Key Hold mode with per-note scan-code tracking
- `ConnectionEvaluator` (pure, tested)
- `MidiSettingsView` with compact layout: device/keymap dropdowns, keymap preview, toggles, delay slider, live recent-send log, Key Hold checkbox, Release All Keys panic button
- `KeymapLayoutTabView` with `KeybedControl`: visual piano-keybed rendering, hover highlighting, tooltips, C-note octave labels, horizontal scroll via `TrackBar`
- `TabbedWindow2` settings panel with two tabs (Settings + Keymap Layout), shared keymap selection via `SelectedKeymapChanged` event
- Toggle keybind, corner icon states (green/gray/orange), focus guard, right-click toggle on corner icon
- Blish HUD 1.3.0, post-build xcopy to modules folder

### Design Decisions
See [`docs/design-decisions.md`](design-decisions.md) for the historical record of significant design decisions.

### Deferred / Future
- Drum Kit keymap with MIDI drum note mapping
- Floating overlay indicator showing last played note
- Chord support (multi-key bindings per note)
- Better handling of 'extra' keys that activate loops, recording, or chords
- Visual legend for mapped/key-switch/unmapped colors in the keybed preview
- Custom keymap generation from the keybed
- Clicking a key in the visualization to send it to the game
- Unify note-name parsing between `KeybedLayoutCalculator` and `KeymapPreviewFormatter`

---

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures** — `Newtonsoft.Json` `FileNotFoundException` (copy-local issue).
- **Window width auto-sizing** to fit the full keybed without scrolling (alternative to TrackBar approach).
