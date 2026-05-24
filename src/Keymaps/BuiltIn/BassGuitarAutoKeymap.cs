namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    public static class BassGuitarAutoKeymap
    {
        // Octave 2 (unmapped in this keymap) contains action keys on the Bass Guitar:
        //   Keys 1-8 : predefined loops
        //   Key 9    : return to octave 1
        //   Key 0    : tempo lock / unlock
        public static Keymap Instance { get; } = new Keymap("bass-guitar-auto", "Bass Guitar (Auto)")
        {
            AutoOctaveSwap = true,
            OctaveDownKey = "9",
            OctaveUpKey = "0",
            Notes =
            {
                { "C3", new NoteDefinition(key: "1", octave: 0) },
                { "D3", new NoteDefinition(key: "2", octave: 0) },
                { "E3", new NoteDefinition(key: "3", octave: 0) },
                { "F3", new NoteDefinition(key: "4", octave: 0) },
                { "G3", new NoteDefinition(key: "5", octave: 0) },
                { "A3", new NoteDefinition(key: "6", octave: 0) },
                { "B3", new NoteDefinition(key: "7", octave: 0) },

                { "C4", new NoteDefinition(key: "1", octave: 1, altOctave: 0, altOctaveKey: "8") },
                { "D4", new NoteDefinition(key: "2", octave: 1) },
                { "E4", new NoteDefinition(key: "3", octave: 1) },
                { "F4", new NoteDefinition(key: "4", octave: 1) },
                { "G4", new NoteDefinition(key: "5", octave: 1) },
                { "A4", new NoteDefinition(key: "6", octave: 1) },
                { "B4", new NoteDefinition(key: "7", octave: 1) },
                { "C5", new NoteDefinition(key: "8", octave: 1) }
            }
        };
    }
}
