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
- Rendering is a custom-painted Blish HUD `Control` (`KeybedPreviewControl`).
- Fixed key sizes (24×80 px white, 16×50 px black), hosted in a horizontally scrollable `Panel`.
- Layout math lives in a testable `KeybedLayoutCalculator` in `src/Keymaps/Visualization/`.
- Colors are centralized in `KeybedTheme`.
- `KeymapPreviewFormatter` is left as-is; unifying note-name parsers is a future cleanup.

## Sub-chunks

Each chunk must build and be meaningfully verified before moving to the next. The user must review, accept, and commit each chunk before the next begins.

### Chunk A — Domain model and calculator

**Goal**: Add reverse note-name parsing and a testable calculator that turns a `Keymap` into a `KeybedLayout`.

**Files to create**:

- `src/Core/MidiNote.cs` — add `TryParseNoteName(string noteName, out int noteNumber)`.
- `src/Keymaps/Visualization/KeybedKey.cs` — data shape for one piano key.
- `src/Keymaps/Visualization/KeybedLayout.cs` — container for the full layout.
- `src/Keymaps/Visualization/KeybedLayoutCalculator.cs` — pure calculator.

**Files to modify**:

- `src/Core/MidiNote.cs`
- `Blish HUD - MIDI Control.csproj` — add new `.cs` files to `<Compile Include>`.

**Tests to create**:

- `tests/Keymaps/Visualization/KeybedLayoutCalculatorTests.cs`
- `tests/Core/MidiNoteTests.cs` — add tests for `TryParseNoteName`.

**Acceptance criteria**:

- Project builds.
- All new tests pass.
- Calculator correctly handles:
  - Natural, sharp, and flat note names.
  - Single and multi-octave keymaps.
  - Empty keymaps (returns no keys).
  - Non-contiguous octave spans (includes intermediate empty octaves).
  - Key switch detection (octave-shift keys).

**Dependencies**: None.

---

### Chunk B — KeybedPreviewControl skeleton

**Goal**: Create the custom-painted control and theme, and render a static `KeybedLayout` into it.

**Files to create**:

- `src/UI/KeybedTheme.cs` — color and dimension constants.
- `src/UI/KeybedPreviewControl.cs` — custom Blish HUD `Control` that paints white keys, black keys, and labels.

**Files to modify**:

- `Blish HUD - MIDI Control.csproj` — add new `.cs` files.

**Tests to create**:

- None for rendering itself; verified by build and manual inspection. Optional: a small test that constructs the control with a layout and asserts on `Size`.

**Acceptance criteria**:

- Project builds.
- The control renders a sample keymap correctly:
  - White and black keys are drawn at the right positions.
  - Mapped keys show the GW2 key.
  - Key switches are drawn in the key-switch color.
  - Unmapped keys are dimmed.
- Theme dimensions and colors are centralized and easy to tweak.

**Dependencies**: Chunk A must be complete.

---

### Chunk C — Keymap Layout tab and shared selection

**Goal**: Add the second settings tab, wire the keymap dropdown to the shared selection, and keep both tabs synchronized.

**Files to create**:

- `src/UI/KeymapLayoutView.cs` — view for the new tab.

**Files to modify**:

- `Module.cs`:
  - Add `public event Action<string>? SelectedKeymapChanged;`.
  - Raise the event from `SelectKeymap` and from the fallback path in `ReloadKeymaps`.
  - Add a second `Tab` for the Keymap Layout tab.
- `src/UI/MidiSettingsView.cs` — subscribe to `SelectedKeymapChanged` and update the dropdown without recursion.
- `Blish HUD - MIDI Control.csproj` — add `KeymapLayoutView.cs`.

**Tests to create**:

- `tests/ModuleKeymapSelectionTests.cs` (or similar) — verify the event is raised when `SelectKeymap` is called.

**Acceptance criteria**:

- Project builds.
- Opening the settings window shows two tabs: "Settings" and "Keymap Layout".
- Changing the keymap on either tab updates the other tab and the active keymap.
- The Keymap Layout tab renders the selected keymap in the keybed control.
- `Unload` unsubscribes from the shared event.

**Dependencies**: Chunk B must be complete.

---

### Chunk D — Tooltips, C markers, edge cases, and polish

**Goal**: Add interactive details and handle edge cases.

**Files to create**:

- None expected; modifications to existing files.

**Files to modify**:

- `src/UI/KeybedPreviewControl.cs`:
  - Dynamic tooltip based on key under mouse.
  - C-octave markers (small labels under C keys).
  - Empty/unmapped keymap placeholder message.
- `src/UI/KeymapLayoutView.cs` — adjust layout if needed.

**Tests to create**:

- Optional unit tests for tooltip text generation if extracted to a helper.

**Acceptance criteria**:

- Project builds.
- Hovering a key shows the agreed tooltip content (mapped note, key switch, alt octave, unmapped).
- Empty keymaps show a placeholder.
- C keys are labeled by octave.
- Manual end-to-end check in Blish HUD looks acceptable.

**Dependencies**: Chunk C must be complete.

---

## Follow-ups (out of scope for this feature chunk)

- Window width auto-sizing to fit the full keybed without scrolling.
- Visual legend for mapped/key-switch/unmapped colors.
- Custom keymap generation from the keybed.
- Clicking a key in the visualization to send it to the game.
- Unify note-name parsing between `KeybedLayoutCalculator` and `KeymapPreviewFormatter`.
