# Implementation Plan: Blish HUD MIDI Control

## v0.0.5 — Custom JSON Keymaps

### Goals

Allow users to create custom keymaps by dropping `.json` files into the module's data directory (a Blish HUD-managed folder, typically under `%USERPROFILE%\Documents\Guild Wars 2\addons\blishhud\`). Custom keymaps appear in the keymap settings dropdown alongside built-ins.

### Chunk 1 — JSON Schema and Model

Define the JSON contract. Keystone fields:

```json
{
  "id": "my-custom-keymap",
  "name": "My Custom Keymap",
  "autoOctaveSwap": true,
  "octaveDownKey": "9",
  "octaveUpKey": "0",
  "notes": {
    "C3": { "key": "1", "octave": 0 },
    "C4": { "key": "1", "octave": 1, "altOctave": 0, "altOctaveKey": "8" },
    "C#4": { "key": "9" },
    "F#4": { "forceInternalOctave": 0 }
  }
}
```

Rules:
- `id` and `name` are required.
- `notes` keys are MIDI note names (`C3`, `D#4`, `Bb5`, etc.).
- A note with only `key` and no `octave` is a special/manual key (e.g. octave switch).
- `autoOctaveSwap` defaults to `true` when absent.

### Chunk 2 — File Discovery and Loading

`KeymapRegistry` constructor gains an optional `string customKeymapsDirectory` parameter (provided by `MidiModule` via `DirectoriesManager`):

- Enumerate `*.json` files in the directory.
- Deserialize each with `try/catch` (Newtonsoft.Json).
- Valid keymaps are appended to `_keymaps`.
- Invalid files are logged as warnings and skipped.
- A duplicate `id` is skipped with a warning (first wins; built-ins win over custom).

### Chunk 3 — Settings UI Refresh and Error Surfacing ✅

`MidiSettingsView` changes:
- **Auto-scan on open**: `Build()` calls `RefreshKeymaps()` which reloads custom keymaps from disk.
- **Manual reload**: "Reload Keymaps" button next to the keymap dropdown (same pattern as MIDI device "Refresh").
- **Status label**: Below the dropdown, shows *"X custom keymaps loaded, Y error(s)"*. Gray when clean, orange when errors. Empty (hidden) when 0 customs and 0 errors.
- **Error tooltip**: Hover the status label to see first 10 error lines.
- **Dynamic preview panel**: When status is empty, preview panel raises up 24px and expands to 114px tall. When status shows, preview returns to normal 90px. Everything below (toggles, slider, log) stays at fixed positions.
- **Stale selection handling**: If the selected custom keymap disappears on reload, auto-fallback to `minstrel-auto` with a logged warning and fresh `KeySender` octave tracker.

`KeymapRegistry` changes:
- Built-in and custom keymaps stored in separate internal lists (`_builtInKeymaps`, `_customKeymaps`).
- `LoadCustomKeymaps()` clears customs before scanning — idempotent re-scanning.
- `CustomKeymapCount` property exposed.

`MidiModule` changes:
- `ReloadKeymaps()` method: re-scans directory, handles stale selection fallback.
- `CustomKeymapCount` and `KeymapLoadErrors` properties exposed to view.

### Chunk 4 — Documentation

Add a README / wiki section explaining the JSON keymap format with examples. Sample keymaps exist in `sample-keymaps/`:
- `beginners-harp.json` — valid single-octave example
- `bad-missing-id.json` — example of missing required field
- `bad-id-collision.json` — example of ID conflict with built-in

## Deferred / Future

- [ ] Frame Drum Auto keymap
- [ ] Drum Kit keymap with MIDI drum note mapping
- [ ] `noteoff` support / true key-down key-up hold behavior
- [ ] Floating overlay indicator showing last played note (outside settings tab)
- [ ] Chord support (multi-key bindings per note)
- [ ] Better handling of 'extra' keys that activate loops, recording, or chords
