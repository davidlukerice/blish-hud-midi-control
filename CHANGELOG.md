# Changelog

## [Unreleased]

## [0.2.0] – 2026-06-20

### Added
- **Keybed Layout Preview** — New "Keymap Layout" tab in the settings window shows a visual piano-key rendering of any selected keymap.
- **Key Hold mode** — New "Enable Key Hold" toggle in settings. When on, MIDI note-on sends a key-down and note-off sends a key-up, so GW2 keys are held for the duration the MIDI note is held. Key Tap mode (down+up per note) remains the default. Includes a "Release All Keys" panic button to recover stuck keys.
- **Black Lion Drum built-in keymap** — `black-lion-drum`. Maps 15 MIDI notes (`C4`–`E5`) to the Black Lion Drum set Chair's 15 sounds. See in-game keymap layout for full mapping.
- **Frame Drum (Auto) built-in keymap** — `frame-drum-auto`. Maps MIDI notes `C4`–`E4` to the Frame Drum's five percussion sounds on keys `1`–`5`. No octave swapping (the instrument has no octaves).
- **GW2 gameplay sample keymaps** — Three custom keymaps for non-musical GW2 gameplay: `gw2-gameplay-25key`, `gw2-gameplay-37key`, and `gw2-gameplay-49key`. See `sample-keymaps/README.md` for details.

### Fixed
- **Settings ↔ Layout tab keymap sync** — Changing the keymap on either tab now updates the other and the active playing keymap.
- **Settings UI sync on corner icon right-click** — The settings panel now correctly reflects the `Send Notes` toggle state when it is toggled via right-click on the corner icon.

## [0.1.0] – 2026-05-31

### Added
- **Custom JSON keymaps** — Drop `.json` files into the `midi-keymaps` directory (managed by Blish HUD, typically under `%USERPROFILE%\Documents\Guild Wars 2\addons\blishhud\midi-keymaps\`). Custom keymaps appear in the keymap dropdown alongside built-ins.
- **Keymap reload** — "Reload Keymaps" button in settings rescans the directory on demand. Auto-scan also runs when the settings panel opens.
- **Keymap load status** — Status label below the keymap dropdown shows *"X custom keymaps loaded, Y error(s)"*. Gray when clean, orange when errors. Hover for error details.
- **Dynamic preview panel** — Preview panel expands to 114px when no status text is shown, reclaiming the space. Shrinks to 90px when status text appears.
- **Stale keymap fallback** — If a selected custom keymap is deleted and the user reloads, the module automatically falls back to `minstrel-auto` with a logged warning.
- **Corner icon right-click toggle** — Right-clicking the corner icon now toggles `Send Notes` on or off, in addition to the existing toggle keybind.

## [0.0.4] – 2026-05-24

### Added
- **Five new auto keymaps**
  - **Lute (Auto)** — `lute-auto`
  - **Harp (Auto)** — `harp-auto`
  - **Horn (C) Auto** — `horn-c-auto`
  - **Horn (E) Auto** — `horn-e-auto`
  - **Verdarach (Auto)** — `verdarach-auto`
  - **Bass Guitar (Auto)** — `bass-guitar-auto`
- **General (Manual) keymap** — `general`. A general-purpose manual keymap mapping the default C4 instrument octave without auto-octave-swap. Manual octave switches on D5 (key 9, down) and E5 (key 0, up). Useful for instruments not yet covered by a dedicated built-in keymap.

### Fixed
- **Keymap preview accuracy** — The keymap preview no longer labels every note without an octave property as `(oct shift)`. It now checks the note's key against the keymap's `OctaveDownKey` / `OctaveUpKey` before applying the label.

## [0.0.3] – 2026-05-24

### Added
- **Live Recent Sends log** — Settings panel now shows a real-time scrolling log with octave state, MIDI note name, and sent GW2 key(s). Entries like `oct 0→1: D4 → 0 + 2` show when an octave shift occurred.

### Changed
- **Compact settings layout** — Shifted UI elements up and left
- **Corner icon state** — Tooltip and icon now update reliably when MIDI device disconnects/reconnects.

## [0.0.2] – 2026-05-??

### Added
- **Five additional built-in keymaps** — Grand Piano, Flute C, Flute E, Choir Bell, and Minstrel (manual, non-auto).

## [0.0.1] – 2026-05-16

### Added
- **MIDI-to-keyboard mapping** — Play GW2 instruments with a real MIDI keyboard or controller.
- **Auto octave swap** — Automatically tracks the current in-game octave and shifts up/down (`9`/`0`) when a note is outside the current range. Configurable delay for multi-octave jumps.
- **Auto-reconnect** — Detects MIDI device disconnection and retries every 10 seconds until the device returns. Corner icon turns orange during retry and green on reconnect.
- **Built-in instrument keymaps** — Pre-configured mapping for The Minstrel with keymap preview in settings.
- **Toggle keybind** — Configurable global keybind to quickly enable or disable note sending.
- **Focus guard** — Option to block all keypresses when Guild Wars 2 is not in focus.
- **Settings panel** — `TabbedWindow2` settings with device selection, keymap selection with live preview, toggle checkboxes, delay slider, and recent-send log.
- **Corner icon states** — Green (active), gray (muted), orange (disconnected/retrying) with live tooltip updates.
- **93 passing unit tests** covering keymap resolution, MIDI input, connection evaluation, and key sending.

### Changed
- Upgraded to Blish HUD 1.3.0 with zero deprecation warnings.
- Cleaned stale NuGet packages and fixed NAudio references.

[0.0.1]: https://github.com/davidlukerice/blish-hud-midi-control/releases/tag/v0.0.1
