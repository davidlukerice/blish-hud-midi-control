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

## Current Session — Keybed Layout Preview — Chunk C & Key Rendering Fixes

### What was done

#### Chunk C — Shared keymap selection (Settings ↔ Layout tabs)
- `Module.cs` — Added `SelectedKeymapChanged` event (raised from `SelectKeymap` and `ReloadKeymaps` fallback)
- `MidiSettingsView` — Subscribes to `SelectedKeymapChanged`, syncs dropdown when the active keymap changes externally (with recursion guard)
- `KeymapLayoutTabView` — Selecting a keymap now calls `_module.SelectKeymap()`, syncing the active keymap. Also subscribes to `SelectedKeymapChanged` to reflect external changes. Refactored `UpdatePreview()` to avoid re-setting the active keymap on initial load.

#### Keybed rendering fixes
- **`KeybedControl` invisible / clipped bug** — `Paint` receives local bounds (origin 0,0), but `Control.Draw`'s `SpriteBatch` is in screen-space with no implicit offset. Added `bounds.Offset(this.AbsoluteBounds.X, this.AbsoluteBounds.Y)` at the top of `Paint` so drawing uses screen coordinates matching the scissor rectangle.
- **`Layout` setter missing `Invalidate()`** — Added `Invalidate()` so the control re-renders when the keymap changes.

#### Fixed key widths with horizontal scrolling
- `KeybedControl` — Constant 24px white-key width (no dynamic squeezing). Control width computed as `whiteKeyCount * WhiteKeyWidth + padding`.
- Height fixed at 96px (compact, roughly half the previous 180px).
- `KeymapLayoutTabView` — Wraps `KeybedControl` in a `Panel` (420×104 viewport). Added a `TrackBar` underneath that drives `Panel.HorizontalScrollOffset` so wide keymaps are panned left/right rather than squeezed.
- Scrollbar range auto-adjusts per keymap: `max(0, keybedWidth - 420)`.

#### Layout overlap fix
- Info label in `KeymapLayoutTabView` changed from `AutoSizeHeight = true` to fixed `Height = 20` to prevent it from growing downward into the keybed panel.

### Files changed this session
- `Module.cs`
- `src/UI/MidiSettingsView.cs`
- `src/UI/KeymapLayoutTabView.cs`
- `src/UI/KeybedControl.cs`

### Tests
- 253 passed, 5 failed (all pre-existing `KeymapRegistryTests`: count mismatches + `Newtonsoft.Json FileNotFoundException`). No new regressions.

### Outstanding / Follow-up
- **25 pre-existing `KeymapRegistryTests` failures** — `Newtonsoft.Json` `FileNotFoundException` because the assembly reference in the main `.csproj` has `<Private>False</Private>`, so Blish HUD dependencies are not copied to the test output directory. Fixing this copy-local behavior is out of scope for the keybed feature but should be addressed so the full test suite is green.
- **Visual refinements**: static note-name labels (e.g. "C3" beneath keys), key hover highlighting, click-to-tooltip with note details.
- **Key labels on narrow black keys** may overflow — consider reducing font size or not showing key on black keys.
- **Window width auto-sizing** to fit the full keybed without scrolling (alternative to the TrackBar approach).
