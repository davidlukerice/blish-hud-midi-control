namespace DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn
{
    /// <summary>
    /// Black Lion Drum set Chair — a 15-note drum kit with no octaves.
    /// Cymbals are on the black keys, drums on the white keys.
    /// </summary>
    public static class BlackLionDrumKeymap
    {
        public static Keymap Instance { get; } = new Keymap("black-lion-drum", "Black Lion Drum Set")
        {
            AutoOctaveSwap = false,
            Notes =
            {
                // Drums — white keys (ascending)
                { "C4",  new NoteDefinition(key: "1", octave: 0) },  // Bass Drum
                { "D4",  new NoteDefinition(key: "2", octave: 0) },  // Bass Drum
                { "E4",  new NoteDefinition(key: "3", octave: 0) },  // Snare Drum
                { "F4",  new NoteDefinition(key: "4", octave: 0) },  // Snare Drum
                { "G4",  new NoteDefinition(key: "5", octave: 0) },  // Snare Cross Stick
                { "A4",  new NoteDefinition(key: "6", octave: 0) },  // Ghost Note Snare
                { "B4",  new NoteDefinition(key: "7", octave: 0) },  // Ghost Note Snare
                { "C5",  new NoteDefinition(key: "8", octave: 0) },  // High Tom
                { "D5",  new NoteDefinition(key: "9", octave: 0) },  // Mid Tom
                { "E5",  new NoteDefinition(key: "0", octave: 0) },  // Floor Tom

                // Cymbals — black keys (interleaved)
                { "C#4", new NoteDefinition(key: "F1", octave: 0) }, // Crash Cymbal
                { "D#4", new NoteDefinition(key: "F2", octave: 0) }, // Ride Cymbal
                { "F#4", new NoteDefinition(key: "F3", octave: 0) }, // Hi-Hat Closed
                { "G#4", new NoteDefinition(key: "F4", octave: 0) }, // Hi-Hat Open
                { "A#4", new NoteDefinition(key: "F5", octave: 0) }, // Hi-Hat Foot
            }
        };
    }
}
