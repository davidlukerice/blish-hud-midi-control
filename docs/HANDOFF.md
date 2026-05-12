# Agent Handoff — Blish HUD MIDI Control

## Completed Work (Ready)

Chunks 1–8 plus settings UI are fully implemented and unit-tested (76 tests passing):
- Domain model: `NoteDefinition`, `Keymap`
- Built-in keymap: `MinstrelAutoKeymap`
- `KeymapRegistry`
- `SendInput` P/Invoke wrapper
- `KeyToScanCode` mapping utility
- `KeySendThread`
- `MidiInputManager`, `MidiNote`, `MidiNoteEvent`
- `KeySender` (pure `Resolve()` + `Send()`) with `NoteProcessed` event for diagnostics
- **`KeySender` wired into `Module.cs`** — `Update()` drains queue, respects settings, keymap fallback
- **`MidiSettingsView`** — custom settings panel with device dropdown, keymap dropdown, toggles, delay slider, and recent-send log
- **`TabbedWindow` settings** — opens on corner icon click (old Blish HUD 0.5.2 doesn't support `GetSettingsView()`)

## Remaining Work

| # | Name | Files | Verifiable because |
|---|---|---|---|
| 9 | **Auto-reconnect** | `Module.cs`, `MidiInputManager.cs?` | Simulated by force-closing device in unit test or observable log output |
| 10 | **Toggle keybind** | `Module.cs` | Keybind registration compiles; integration tested at runtime |
| 13 | **Build/package** | `.csproj`, post-build | Clean build, `.bhm` post-build xcopy restored, stale packages removed |

## Important Notes

- **Blish HUD 0.5.2-alpha.648 does NOT support `GetSettingsView()`** — confirmed by reflection. The standard approach in this version is a `TabbedWindow` opened from the corner icon. The auto-rendered settings still show plain text boxes for `SelectedMidiDeviceName` and `SelectedKeymapId`.
- **Corner icon click now opens the settings window** instead of toggling `SendNotes`. The mute toggle is available as a checkbox inside the settings panel.
- **`KeySender.NoteProcessed`** fires after each note is resolved and enqueued. The handler in `Module.cs` pushes a formatted entry into `_recentSendLog` (max 10 entries).
- **Settings window content** (`MidiSettingsView`) uses absolute positioning (`Location = new Point(x, y)`) with a vertical `y` tracker. This is the safest layout approach for Blish HUD 0.5.2's older control system. `FlowPanel` is available but proved unreliable for nested layout in this version.

## Next Chunk

TBD by user.
