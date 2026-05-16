namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    public static class GrandPianoAutoKeymap
    {
        public static Keymap Instance { get; } = new Keymap("grand-piano-auto", "Ornate Grand Piano (Auto)")
        {
            AutoOctaveSwap = true,
            OctaveDownKey = "9",
            OctaveUpKey = "0",
            Notes =
            {
                { "C3",  new NoteDefinition(key: "1", octave: 0) },
                { "C#3", new NoteDefinition(key: "F1", octave: 0) },
                { "D3",  new NoteDefinition(key: "2", octave: 0) },
                { "D#3", new NoteDefinition(key: "F2", octave: 0) },
                { "E3",  new NoteDefinition(key: "3", octave: 0) },
                { "F3",  new NoteDefinition(key: "4", octave: 0) },
                { "F#3", new NoteDefinition(key: "F3", octave: 0) },
                { "G3",  new NoteDefinition(key: "5", octave: 0) },
                { "G#3", new NoteDefinition(key: "F4", octave: 0) },
                { "A3",  new NoteDefinition(key: "6", octave: 0) },
                { "A#3", new NoteDefinition(key: "F5", octave: 0) },
                { "B3",  new NoteDefinition(key: "7", octave: 0) },

                { "C4",  new NoteDefinition(key: "1", octave: 1, altOctave: 0, altOctaveKey: "8") },
                { "C#4", new NoteDefinition(key: "F1", octave: 1) },
                { "D4",  new NoteDefinition(key: "2", octave: 1) },
                { "D#4", new NoteDefinition(key: "F2", octave: 1) },
                { "E4",  new NoteDefinition(key: "3", octave: 1) },
                { "F4",  new NoteDefinition(key: "4", octave: 1) },
                { "F#4", new NoteDefinition(key: "F3", octave: 1) },
                { "G4",  new NoteDefinition(key: "5", octave: 1) },
                { "G#4", new NoteDefinition(key: "F4", octave: 1) },
                { "A4",  new NoteDefinition(key: "6", octave: 1) },
                { "A#4", new NoteDefinition(key: "F5", octave: 1) },
                { "B4",  new NoteDefinition(key: "7", octave: 1) },

                { "C5",  new NoteDefinition(key: "8", octave: 1, altOctave: 2, altOctaveKey: "1") },
                { "C#5", new NoteDefinition(key: "F1", octave: 2) },
                { "D5",  new NoteDefinition(key: "2", octave: 2) },
                { "D#5", new NoteDefinition(key: "F2", octave: 2) },
                { "E5",  new NoteDefinition(key: "3", octave: 2) },
                { "F5",  new NoteDefinition(key: "4", octave: 2) },
                { "F#5", new NoteDefinition(key: "F3", octave: 2) },
                { "G5",  new NoteDefinition(key: "5", octave: 2) },
                { "G#5", new NoteDefinition(key: "F4", octave: 2) },
                { "A5",  new NoteDefinition(key: "6", octave: 2) },
                { "A#5", new NoteDefinition(key: "F5", octave: 2) },
                { "B5",  new NoteDefinition(key: "7", octave: 2) },
                { "C6",  new NoteDefinition(key: "8", octave: 2) }
            }
        };
    }
}
