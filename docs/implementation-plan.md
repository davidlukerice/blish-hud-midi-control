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

### Chunk 3 — Settings UI Refresh

When the settings panel is opened or a custom JSON is added/removed, the keymap dropdown should reflect the current set. Blish HUD does not provide file-watching baked into its settings; simplest approach is to reload the registry each time the settings panel is built (or add a "Reload" button).

### Chunk 4 — Validation and Error UI

- Minimum: log to Blish HUD's log file; malformed files are silently skipped.
- Stretch: surface a label in the settings panel: *"2 custom keymaps loaded, 1 file failed to parse."*

## Deferred / Future

- [ ] Frame Drum Auto keymap
- [ ] Drum Kit keymap with MIDI drum note mapping
- [ ] `noteoff` support / true key-down key-up hold behavior
- [ ] Floating overlay indicator showing last played note (outside settings tab)
- [ ] Chord support (multi-key bindings per note)
- [ ] Better handling of 'extra' keys that activate loops, recording, or chords
