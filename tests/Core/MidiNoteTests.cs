using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Core;

namespace DavidRice.BlishHud.MidiControl.Tests.Core
{
    [TestFixture]
    public class MidiNoteTests
    {
        [Test]
        public void GetNoteName_MiddleC_IsC4()
        {
            Assert.That(MidiNote.GetNoteName(60), Is.EqualTo("C4"));
        }

        [Test]
        public void GetNoteName_C3_IsCorrect()
        {
            // MIDI note 48 should be C3 in scientific pitch notation
            Assert.That(MidiNote.GetNoteName(48), Is.EqualTo("C3"));
        }

        [Test]
        public void GetNoteName_SharpNotes_AreCorrect()
        {
            // MIDI 61 = C#4, 66 = F#4, 73 = C#5
            Assert.That(MidiNote.GetNoteName(61), Is.EqualTo("C#4"));
            Assert.That(MidiNote.GetNoteName(66), Is.EqualTo("F#4"));
            Assert.That(MidiNote.GetNoteName(73), Is.EqualTo("C#5"));
        }

        [Test]
        public void GetNoteName_EdgeCases()
        {
            // Lowest MIDI note (0) is C-1
            Assert.That(MidiNote.GetNoteName(0), Is.EqualTo("C-1"));
            // Highest MIDI note (127) is G9
            Assert.That(MidiNote.GetNoteName(127), Is.EqualTo("G9"));
        }
    }
}
