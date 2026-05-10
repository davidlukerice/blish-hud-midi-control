#nullable enable

namespace DavidRice.BlishHud.MidiControl.Core
{
    /// <summary>
    /// A domain event representing a single MIDI note-on or note-off message.
    /// Produced by <see cref="MidiInputManager"/> and consumed by <see cref="KeySender"/>.
    /// </summary>
    public readonly struct MidiNoteEvent
    {
        /// <summary>
        /// The MIDI note number (0–127). 60 = middle C (C4).
        /// </summary>
        public int NoteNumber { get; }

        /// <summary>
        /// True for a note-on message (velocity &gt; 0), false for note-off.
        /// </summary>
        public bool IsNoteOn { get; }

        public MidiNoteEvent(int noteNumber, bool isNoteOn)
        {
            NoteNumber = noteNumber;
            IsNoteOn = isNoteOn;
        }

        public override string ToString() => $"{MidiNote.GetNoteName(NoteNumber)} ({NoteNumber}, {(IsNoteOn ? "on" : "off")})";
    }
}
