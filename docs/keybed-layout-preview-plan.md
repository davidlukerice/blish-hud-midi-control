# Keybed Layout Preview — Implementation Plan

A read-only piano-keybed visualization of the active keymap, hosted on a new "Keymap Layout" tab in the settings window.

## Design Decisions

See `CONTEXT.md` for canonical terms:

- **Keybed Layout Preview**: read-only piano-key visualization of a keymap.
- **Keymap Layout Tab**: settings tab that hosts the preview and allows keymap selection.
- **Key Switch**: a mapped MIDI note that triggers an internal action (octave shift) rather than a GW2 note key.

Non-domain design decisions captured during this session:

- Keymap selection is **shared** between the Settings tab and the Keymap Layout tab via a module-level `SelectedKeymapChanged` event.
- The visible range is the full octave span from the lowest to highest octave that contains any GW2-mapped note, with empty intermediate octaves included.
- Rendering is a custom-painted Blish HUD `Control` (`KeybedControl`).
- Fixed key sizes (24px white, 16px black), constant regardless of keymap width. Wide keymaps are horizontally panned via a `TrackBar` that drives `Panel.HorizontalScrollOffset`.
- `KeybedControl.Paint` uses `bounds.Offset(this.AbsoluteBounds.X, this.AbsoluteBounds.Y)` because Blish HUD's `SpriteBatch` in `Control.Draw` is in screen space with no implicit local-to-screen transform.
- Layout math lives in a testable `KeybedLayoutCalculator` in `src/Keymaps/Visualization/`.
- `KeymapPreviewFormatter` is left as-is; unifying note-name parsers is a future cleanup.

## Sub-chunks

### Chunk A — Domain model and calculator ✅

- `src/Core/MidiNote.TryParseNoteName(string noteName, out int noteNumber)`.
- `src/Keymaps/Visualization/KeybedKey.cs`, `KeybedLayout.cs`, `KeybedLayoutCalculator.cs`.
- Tests: `KeybedLayoutCalculatorTests.cs`, `MidiNoteTests.cs`.

### Chunk B — KeybedControl rendering ✅

- `src/UI/KeybedControl.cs` — custom `Control` with `Paint` override.
- Renders white keys, black keys, GW2 key labels, key-switch orange borders.
- Verified in-game after fixing screen-space coordinate offset bug.

### Chunk C — Keymap Layout tab and shared selection ✅

- `src/UI/KeymapLayoutTabView.cs` — `IView` for the second settings tab.
- Keymap dropdown on Layout tab calls `_module.SelectKeymap()`, syncing active keymap.
- `MidiSettingsView` subscribes to `SelectedKeymapChanged` to sync from Layout tab.
- `KeymapLayoutTabView` subscribes to `SelectedKeymapChanged` to sync from Settings tab.
- Event unsubscribe on `DoUnload` / `Unload`.

### Chunk D — Tooltips, C markers, edge cases, and polish

**Goal**: Add interactive details and handle edge cases.

**Files to modify**:
- `src/UI/KeybedControl.cs`:
  - Dynamic tooltip based on key under mouse.
  - C-octave markers (small labels under C keys).
  - Empty/unmapped keymap placeholder message.
- `src/UI/KeymapLayoutTabView.cs` — layout adjustments if needed.

**Tests to create**:
- Optional unit tests for tooltip text generation if extracted to a helper.

**Acceptance criteria**:
- Project builds.
- Hovering a key shows the agreed tooltip content (mapped note, key switch, alt octave, unmapped).
- Empty keymaps show a placeholder.
- C keys are labeled by octave.
- Manual end-to-end check in Blish HUD looks acceptable.

**Dependencies**: Chunk C must be complete.

## Follow-ups (out of scope for this feature chunk)

- Visual legend for mapped/key-switch/unmapped colors.
- Custom keymap generation from the keybed.
- Clicking a key in the visualization to send it to the game.
- Unify note-name parsing between `KeybedLayoutCalculator` and `KeymapPreviewFormatter`.
