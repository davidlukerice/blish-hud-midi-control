# Design Decisions

Historical record of significant design decisions for the Blish HUD MIDI Control module.

## 2026-05-31 — Custom JSON Keymaps (v0.1.0)

### JSON Shape: Dictionary-Style Notes
- **Decision**: `notes` is a JSON object (dictionary) keyed by MIDI note name, not an array.
- **Rationale**: Natural lookup by note name; matches the in-memory `Dictionary<string, NoteDefinition>` model.
- **Required fields**: `id`, `name`, `notes` (can be an empty dictionary).

### ID Resolution from JSON Field
- **Decision**: The keymap `id` comes from the JSON `id` field, not the filename.
- **Rationale**: Filenames can change; the `id` is the stable identifier used for persistence and collision detection.
- **Collision policy**: Built-in keymaps win over custom. Duplicate custom IDs are skipped with a warning (first wins).

### Error Policy: Skip Bad Files
- **Decision**: Invalid keymap files are skipped individually; the load operation continues for remaining files.
- **Rationale**: One malformed file should not break all custom keymaps. Errors are collected in `KeymapRegistry.LoadErrors` and surfaced in the UI.

### Model Mutability for Deserialization
- **Decision**: Added public setters and parameterless constructors to `Keymap` and `NoteDefinition` for Newtonsoft.Json deserialization.
- **Rationale**: Newtonsoft.Json requires mutable properties or complex constructor configuration. Public setters are the simplest path.
- **Alternative considered**: Custom `JsonConverter` with immutable models — rejected as overkill for the current scope.

### Directory Name: `midi-keymaps`
- **Decision**: The custom keymaps directory is named `midi-keymaps` (kebab-case) and registered in the module manifest via Blish HUD's `DirectoriesManager`.
- **Rationale**: Consistent with Blish HUD conventions; descriptive and unlikely to conflict with other modules.

### Registry Separation: Built-In vs Custom Lists
- **Decision**: Built-in and custom keymaps are stored in separate internal lists (`_builtInKeymaps`, `_customKeymaps`).
- **Rationale**: Enables idempotent re-scanning — `LoadCustomKeymaps()` clears and repopulates customs without touching built-ins.

### Refresh Strategy: Auto-Scan + Manual Reload
- **Decision**: Custom keymaps are auto-scanned when the settings panel opens, with a manual "Reload Keymaps" button for on-demand refresh.
- **Rationale**: Scan is sub-millisecond; auto-scan ensures fresh state without user action, while the button provides explicit control.

### Stale Keymap Fallback
- **Decision**: If the selected custom keymap disappears on reload, the module auto-fallbacks to `minstrel-auto`.
- **Rationale**: Prevents a dangling selection. A logged warning informs the user. The `KeySender` octave tracker is reset to avoid stale state.

### Error UI: Status Label
- **Decision**: A status label below the keymap dropdown shows *"X custom keymaps loaded, Y error(s)"*.
- **Rationale**: Immediate visibility into load state. Gray when clean, orange when errors. Tooltip shows first 10 error lines. Hidden when 0 customs and 0 errors to reduce visual noise.

### Preview Panel Dynamic Layout
- **Decision**: The preview panel expands to 114px when no status text is shown (reclaiming 24px), and shrinks to 90px when status text appears.
- **Rationale**: Maximizes usable space for the preview when there's nothing to report. Fixed positions for everything below (toggles, slider, log) to avoid cascading layout shifts.
