# Custom Keymaps

Create your own instrument mappings by dropping `.json` files into the module's `midi-keymaps` directory (typically under `%USERPROFILE%\Documents\Guild Wars 2\addons\blishhud\midi-keymaps\`). Custom keymaps appear in the settings dropdown alongside built-ins.

## Quick Start

1. Create a new `.json` file in the `midi-keymaps` directory.
2. Copy the structure from one of the examples below.
3. Open the MIDI Control settings panel in Blish HUD — your keymap appears automatically.
4. Click **Reload Keymaps** if you edit the file while the panel is open.

## Field Reference

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `id` | string | **Yes** | — | Unique identifier. Lowercase letters, numbers, and hyphens only. |
| `name` | string | **Yes** | — | Display name shown in the settings dropdown. |
| `autoOctaveSwap` | boolean | No | `true` | Automatically track and shift octaves. |
| `octaveDownKey` | string | No | — | Key that shifts the instrument down one octave (e.g. `"9"`). |
| `octaveUpKey` | string | No | — | Key that shifts the instrument up one octave (e.g. `"0"`). |
| `notes` | object | **Yes** | — | Map of MIDI note names to note definitions. Must have at least one entry. |

### Note Definition Fields

Each entry in `notes` uses a MIDI note name as the key (e.g. `C4`, `D#4`, `Bb5`).

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `key` | string | *Conditional* | Keyboard key to send. Required unless `forceInternalOctave` is set. |
| `octave` | integer (0–5) | No | Relative octave this note belongs to. Omit for special/manual keys. |
| `altOctave` | integer (0–5) | No | Alternative octave where this note can be played without shifting. |
| `altOctaveKey` | string | No | Key to use when playing on `altOctave`. Only meaningful with `altOctave`. |
| `forceInternalOctave` | integer (0–5) | *Conditional* | Update the internal octave tracker without sending a keypress. Required unless `key` is set. |

A note must have at least one of `key` or `forceInternalOctave`.

### Supported Keys

`1` `2` `3` `4` `5` `6` `7` `8` `9` `0` `A`–`Z` `F1`–`F12` `SPACE` `ENTER` `ESC` `ESCAPE` `TAB` `BACKSPACE`

## Examples

### Single-Octave Manual Keymap

A simple 8-note harp with no auto-octave swapping:

```json
{
  "$schema": "./schema.json",
  "id": "beginners-harp",
  "name": "Beginner's Harp (1 Octave)",
  "autoOctaveSwap": false,
  "octaveDownKey": "9",
  "octaveUpKey": "0",
  "notes": {
    "C4": { "key": "1", "octave": 0 },
    "D4": { "key": "2", "octave": 0 },
    "E4": { "key": "3", "octave": 0 },
    "F4": { "key": "4", "octave": 0 },
    "G4": { "key": "5", "octave": 0 },
    "A4": { "key": "6", "octave": 0 },
    "B4": { "key": "7", "octave": 0 },
    "C5": { "key": "8", "octave": 0 }
  }
}
```

### Auto-Octave with Alt-Octave

A note playable on two octaves with different keys, avoiding unnecessary shifts:

```json
{
  "$schema": "./schema.json",
  "id": "alt-octave-example",
  "name": "Alt Octave Example",
  "autoOctaveSwap": true,
  "octaveDownKey": "9",
  "octaveUpKey": "0",
  "notes": {
    "C4": { "key": "1", "octave": 0 },
    "C5": { "key": "1", "octave": 1, "altOctave": 0, "altOctaveKey": "8" }
  }
}
```

When `C5` is played while the module thinks it's in octave 0, it sends `8` instead of shifting up.

### Force Internal Octave

Used for notes that implicitly change the active octave without sending a keypress. The Minstrel's black keys work this way:

```json
{
  "$schema": "./schema.json",
  "id": "minstrel-black-keys",
  "name": "Minstrel Black Keys",
  "autoOctaveSwap": true,
  "octaveDownKey": "9",
  "octaveUpKey": "0",
  "notes": {
    "F#4": { "forceInternalOctave": 0 },
    "G#4": { "forceInternalOctave": 1 },
    "A#4": { "forceInternalOctave": 2 }
  }
}
```

When `F#4` arrives, the module updates its internal tracker to octave 0. No key is sent — the black key sound is produced by the white key below plus an octave shift in-game.

## Common Errors

### Missing Required Field

```json
{
  "$schema": "./schema.json",
  "name": "Missing ID Keymap",
  "notes": {}
}
```

**Error:** `bad-missing-id.json: missing required field 'id'`

Every keymap must have an `id` and a `name`.

### ID Collision

```json
{
  "$schema": "./schema.json",
  "id": "minstrel-auto",
  "name": "ID Collision Test",
  "notes": {}
}
```

**Error:** `bad-id-collision.json: id 'minstrel-auto' conflicts with existing keymap`

The `id` must be unique across all keymaps, including built-ins. `minstrel-auto` is already taken.

### Empty Notes

```json
{
  "$schema": "./schema.json",
  "id": "empty-notes",
  "name": "Empty Notes",
  "notes": {}
}
```

**Error:** Schema validation fails — `notes` must contain at least one entry.

### Note Without Key or ForceInternalOctave

```json
{
  "$schema": "./schema.json",
  "id": "bad-note",
  "name": "Bad Note",
  "notes": {
    "C4": { "octave": 0 }
  }
}
```

**Error:** Schema validation fails — each note must have at least one of `key` or `forceInternalOctave`.

## Using the JSON Schema

The `schema.json` file in this directory validates your keymap as you edit. Add the `$schema` field at the top of your file:

```json
{
  "$schema": "./schema.json",
  "id": "my-keymap",
  ...
}
```

If your editor supports JSON Schema (VS Code does by default), you'll get:

- Autocomplete for field names and enum values (e.g. supported keys)
- Inline errors for missing required fields, invalid note names, or out-of-range values
- Hover tooltips showing field descriptions

The schema is permissive at the root level — you can add extra metadata fields like `author` or `description` without breaking validation.
