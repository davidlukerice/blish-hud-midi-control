#nullable enable

using NAudio.Midi;

namespace DavidRice.BlishHud.MidiControl.Core
{
    /// <summary>
    /// Pure helper for converting NAudio <see cref="MidiEvent"/> instances into the module's
    /// <see cref="MidiNoteEvent"/> model. Kept separate from <see cref="MidiInputManager"/>
    /// so it can be unit-tested without loading the Blish HUD runtime.
    /// </summary>
    public static class MidiEventConverter
    {
        /// <summary>
        /// Converts an NAudio <see cref="MidiEvent"/> into a module <see cref="MidiNoteEvent"/>.
        /// Returns <c>null</c> for non-note events or after-touch messages.
        /// </summary>
        public static MidiNoteEvent? TryConvertToMidiNoteEvent(MidiEvent? midiEvent)
        {
            if (!(midiEvent is NoteEvent noteEvent))
                return null;

            // NAudio parses a true MIDI Note Off message as a NoteEvent with CommandCode.NoteOff,
            // while a Note On with velocity 0 is represented as NoteOnEvent (also treated as off).
            // Use MidiEvent.IsNoteOn/IsNoteOff so both forms are recognized correctly.
            if (MidiEvent.IsNoteOn(noteEvent))
                return new MidiNoteEvent(noteEvent.NoteNumber, isNoteOn: true);

            if (MidiEvent.IsNoteOff(noteEvent))
                return new MidiNoteEvent(noteEvent.NoteNumber, isNoteOn: false);

            return null;
        }
    }
}
