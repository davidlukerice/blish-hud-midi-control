namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    /// <summary>
    /// The Musical Frame Drum has no octaves or traditional notes.
    /// Keys 1-5 play five distinct percussion sounds. This keymap maps
    /// a contiguous block of MIDI notes (C4-E4) to those drum sounds.
    /// </summary>
    public static class FrameDrumAutoKeymap
    {
        public static Keymap Instance { get; } = new Keymap("frame-drum-auto", "Frame Drum (Auto)")
        {
            AutoOctaveSwap = false,
            Notes =
            {
                { "C4",  new NoteDefinition(key: "1", octave: 0) },
                { "C#4", new NoteDefinition(key: "2", octave: 0) },
                { "D4",  new NoteDefinition(key: "3", octave: 0) },
                { "D#4", new NoteDefinition(key: "4", octave: 0) },
                { "E4",  new NoteDefinition(key: "5", octave: 0) },
            }
        };
    }
}
