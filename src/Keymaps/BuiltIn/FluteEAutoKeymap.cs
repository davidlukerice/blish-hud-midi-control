namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    public static class FluteEAutoKeymap
    {
        public static Keymap Instance { get; } = new Keymap("flute-e-auto", "Flute (E) (Auto)")
        {
            AutoOctaveSwap = true,
            OctaveDownKey = "9",
            OctaveUpKey = "9",
            Notes =
            {
                { "E4",  new NoteDefinition(key: "1", octave: 1) },
                { "F#4", new NoteDefinition(key: "2", octave: 1) },
                { "G#4", new NoteDefinition(key: "3", octave: 1) },
                { "A4",  new NoteDefinition(key: "4", octave: 1) },
                { "B4",  new NoteDefinition(key: "5", octave: 1) },
                { "C#5", new NoteDefinition(key: "6", octave: 1) },
                { "D#5", new NoteDefinition(key: "7", octave: 1) },
                { "E5",  new NoteDefinition(key: "8", octave: 1, altOctave: 2, altOctaveKey: "1") },

                { "F#5", new NoteDefinition(key: "2", octave: 2) },
                { "G#5", new NoteDefinition(key: "3", octave: 2) },
                { "A5",  new NoteDefinition(key: "4", octave: 2) },
                { "B5",  new NoteDefinition(key: "5", octave: 2) },
                { "C#6", new NoteDefinition(key: "6", octave: 2) },
                { "D#6", new NoteDefinition(key: "7", octave: 2) },
                { "E6",  new NoteDefinition(key: "8", octave: 2) },

                { "F4",  new NoteDefinition(key: "9") },
                { "G4",  new NoteDefinition(key: "0") }
            }
        };
    }
}
