# Implementation Plan: Blish HUD MIDI Control

## v0.0.3 (Released)

All v1 PRD user stories implemented. See `docs/MIDI-Control-v1-PRD.md` for details.

Additional features delivered in v0.0.3:
- Additional built-in keymaps: Grand Piano, Flute (C), Flute (E), Choir Bell, Minstrel (non-auto)
- Live Recent Sends log in settings panel

## v0.0.4 — Additional Built-in Keymaps

### Chunk 1 — Lute / Harp / Horn Auto

Three 3-octave C Major natural instruments with identical structure:
- **Musical Lute (Auto)** — `lute-auto`
- **Musical Harp (Auto)** — `harp-auto`
- **Marriner's Horn (Auto)** — `horn-auto`

Each maps C3–B3 (octave 0, keys 1–7), C4–B4 + C5 alt (octave 1), D5–C6 (octave 2).
Octave shifts on `9`/`0`. No sharp notes, no force-internal-octave bindings.

### Chunk 2 — Verdarach Auto

- **Musical Verdarach (Auto)** — `verdarach-auto`

3-octave C Major natural, same structure as chunk 1. Separate chunk because it requires
the legendary Verdarach skin unlock.

### Chunk 3 — Bass Guitar Auto

- **Musical Bass Guitar (Auto)** — `bass-guitar-auto`

2-octave instrument. Starting octave and exact note range to be verified in-game.
Likely C Major natural with 9/0 shifts.

### Chunk 4 — Drum Kit

Research typical MIDI finger-drumming layouts (e.g., General MIDI drum map, Ableton Drum Rack,
common pad controller assignments). Design a keymap that maps MIDI drum notes to the
Musical Frame Drum's 5 sounds:
- Drum #1 (low-pitched, larger drum)
- Drum #2 (high-pitched, larger drum)
- Drum #3 (low-pitched, smaller drum)
- Drum #4 (high-pitched, smaller drum)
- Rim Shot

## Follow-up Items (Not in v0.0.4)

- [ ] Custom JSON keymap loading from data directory
- [ ] `noteoff` support / true key-down key-up hold behavior
- [ ] Floating overlay indicator showing last played note (outside settings tab)
- [ ] Chord support for instruments with multi-key bindings
- [ ] Configuration validation and error UI for malformed custom keymaps
