namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    public static class MinstrelKeymap
    {
        public static Keymap Instance { get; } = new Keymap("minstrel", "The Minstrel")
        {
            AutoOctaveSwap = false,
            Notes =
            {
                { "C4",  new NoteDefinition(key: "1") },
                { "D4",  new NoteDefinition(key: "2") },
                { "E4",  new NoteDefinition(key: "3") },
                { "F4",  new NoteDefinition(key: "4") },
                { "G4",  new NoteDefinition(key: "5") },
                { "A4",  new NoteDefinition(key: "6") },
                { "B4",  new NoteDefinition(key: "7") },
                { "C5",  new NoteDefinition(key: "8") },

                { "C#4", new NoteDefinition(key: "9") },
                { "D#4", new NoteDefinition(key: "0") }
            }
        };
    }
}
