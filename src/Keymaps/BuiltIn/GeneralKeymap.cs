namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    /// <summary>
    /// A general-purpose manual keymap covering the default C4 instrument octave.
    /// D4 shifts octave down, E4 shifts octave up. The user must manage octaves manually.
    /// </summary>
    public static class GeneralKeymap
    {
        public static Keymap Instance { get; } = new Keymap("general", "General (Manual)")
        {
            AutoOctaveSwap = false,
            OctaveDownKey = "9",
            OctaveUpKey = "0",
            Notes =
            {
                { "C4",  new NoteDefinition(key: "1") },
                { "C#4", new NoteDefinition(key: "F1") },
                { "D4",  new NoteDefinition(key: "2") },
                { "D#4", new NoteDefinition(key: "F2") },
                { "E4",  new NoteDefinition(key: "3") },
                { "F4",  new NoteDefinition(key: "4") },
                { "F#4", new NoteDefinition(key: "F3") },
                { "G4",  new NoteDefinition(key: "5") },
                { "G#4", new NoteDefinition(key: "F4") },
                { "A4",  new NoteDefinition(key: "6") },
                { "A#4", new NoteDefinition(key: "F5") },
                { "B4",  new NoteDefinition(key: "7") },
                { "C5",  new NoteDefinition(key: "8") },
                { "D5",  new NoteDefinition(key: "9") }, // octave down
                { "E5",  new NoteDefinition(key: "0") }, // octave up
            }
        };
    }
}
