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

## Current Session — Keybed Layout Preview — Chunk B Complete

**Chunk B** (UI tab and rendering) is implemented, built, and visually verified in-game.

### What was done
- `KeybedControl` — custom `Control` that overrides `Paint(SpriteBatch, Rectangle)` to render a `KeybedLayout` as piano keys:
  - White keys drawn first, black keys overlaid between them (65% width, 60% height)
  - Mapped keys show their GW2 key label centered; unmapped keys shown muted
  - Key-switches (`OctaveDownKey` / `OctaveUpKey`) get an orange border
  - 1×1 pixel texture created via `GraphicsDeviceManager` for drawing filled rects
- `KeymapLayoutTabView` — new `IView` tab in `TabbedWindow2`:
  - Keymap dropdown for preview selection (does not affect active playing keymap)
  - Info label: `Octaves N–M  |  X mapped notes`
  - `KeybedControl` at 420×180
- `Module.cs` — second tab `"Keymap Layout"` added with dedicated `layout.png` icon loaded from `ref/layout.png` via `ContentsManager`
- `Blish HUD - MIDI Control.csproj` — added `KeybedControl.cs` and `KeymapLayoutTabView.cs`

### Files changed
- `src/UI/KeybedControl.cs` (new)
- `src/UI/KeymapLayoutTabView.cs` (new)
- `src/UI/MidiSettingsView.cs` (unchanged — still only on Settings tab)
- `Module.cs`
- `Blish HUD - MIDI Control.csproj`

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures**: these error with `FileNotFoundException` for `Blish HUD.dll` because the assembly reference in the main `.csproj` has `<Private>False</Private>`, so Blish HUD dependencies are not copied to the test output directory. Fixing this copy-local behavior is out of scope for the keybed feature but should be addressed so the full test suite is green.
- **Potential visual refinements**: key labels on very narrow keys may overflow; consider reducing font size or showing labels only on white keys. Consider adding note name labels beneath keys (e.g., "C3"). Consider a scrollbar or horizontal overflow for keymaps spanning many octaves.
- **Mouse interaction**: hover/select feedback on keys, click-to-preview-sound (future), or click-to-see-note-details tooltip.
- **Settings ↔ Layout tab sync**: currently independent. Consider whether selecting a keymap on the Layout tab should optionally sync back to the active keymap.
