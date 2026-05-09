# MIDI Control

A [Blish HUD](https://blishhud.com/) module that maps MIDI controller input to keyboard keypresses for playing instruments in Guild Wars 2.

Originally developed as a standalone Electron app ([midi-to-game-instruments](https://github.com/davidlukerice/midi-to-game-instruments)), this is a native Blish HUD port that runs directly inside the GW2 overlay.

## Features

- **MIDI to keyboard mapping** — Play GW2 instruments with a real MIDI keyboard or controller.
- **Auto octave swap** — Automatically tracks the current in-game octave and shifts up/down (`9`/`0`) when a note is outside the current range. Configurable delay for multi-octave jumps.
- **Built-in instrument keymaps** — Pre-configured mappings for GW2 instruments (starting with The Minstrel).
- **Toggle keybind** — A configurable global keybind to quickly enable or disable note sending.
- **Focus guard** — Optionally block all keypresses when Guild Wars 2 is not in focus.

## Requirements

- Guild Wars 2
- [Blish HUD](https://blishhud.com/)
- A MIDI input device

## GW2 Policy

Per [ArenaNet's macro policy](https://en-forum.guildwars2.com/discussion/65554/policy-macros-and-macro-use):

> You may use music macros to compose or perform in-game music. As long as the macro is used solely for the composition or performance of in-game music and the account is actively attended by a player, we do not place restrictions on its use.

Use at your own risk. This tool is for music performance only and has no affiliation with Guild Wars 2 or ArenaNet.
