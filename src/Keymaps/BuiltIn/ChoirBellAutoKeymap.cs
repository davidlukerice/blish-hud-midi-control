namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    public static class ChoirBellAutoKeymap
    {
        public static Keymap Instance { get; } = new Keymap("choir-bell-auto", "Magnanimous Choir Bell (Auto)")
        {
            AutoOctaveSwap = true,
            OctaveDownKey = "9",
            OctaveUpKey = "0",
            Notes =
            {
                { "C4",  new NoteDefinition(key: "1", octave: 1) },
                { "D4",  new NoteDefinition(key: "2", octave: 1) },
                { "E4",  new NoteDefinition(key: "3", octave: 1) },
                { "F4",  new NoteDefinition(key: "4", octave: 1) },
                { "G4",  new NoteDefinition(key: "5", octave: 1) },
                { "A4",  new NoteDefinition(key: "6", octave: 1) },
                { "B4",  new NoteDefinition(key: "7", octave: 1) },
                { "C5",  new NoteDefinition(key: "8", octave: 1, altOctave: 2, altOctaveKey: "1") },

                { "D5",  new NoteDefinition(key: "2", octave: 2) },
                { "E5",  new NoteDefinition(key: "3", octave: 2) },
                { "F5",  new NoteDefinition(key: "4", octave: 2) },
                { "G5",  new NoteDefinition(key: "5", octave: 2) },
                { "A5",  new NoteDefinition(key: "6", octave: 2) },
                { "B5",  new NoteDefinition(key: "7", octave: 2) },
                { "C6",  new NoteDefinition(key: "8", octave: 2) },

                { "C#4", new NoteDefinition(key: "9") },
                { "D#4", new NoteDefinition(key: "0") }
            }
        };
    }
}
