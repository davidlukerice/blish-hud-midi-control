# Changelog

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
