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

        [Test]
        public void TryParseNoteName_NaturalNotes_AreCorrect()
        {
            Assert.That(MidiNote.TryParseNoteName("C4", out int c4), Is.True);
            Assert.That(c4, Is.EqualTo(60));

            Assert.That(MidiNote.TryParseNoteName("C-1", out int cNeg1), Is.True);
            Assert.That(cNeg1, Is.EqualTo(0));

            Assert.That(MidiNote.TryParseNoteName("G9", out int g9), Is.True);
            Assert.That(g9, Is.EqualTo(127));
        }

        [Test]
        public void TryParseNoteName_SharpNotes_AreCorrect()
        {
            Assert.That(MidiNote.TryParseNoteName("C#4", out int note), Is.True);
            Assert.That(note, Is.EqualTo(61));

            Assert.That(MidiNote.TryParseNoteName("F#4", out note), Is.True);
            Assert.That(note, Is.EqualTo(66));

            Assert.That(MidiNote.TryParseNoteName("C#5", out note), Is.True);
            Assert.That(note, Is.EqualTo(73));
        }

        [Test]
        public void TryParseNoteName_FlatNotes_AreCorrect()
        {
            Assert.That(MidiNote.TryParseNoteName("Db4", out int db4), Is.True);
            Assert.That(db4, Is.EqualTo(61));

            Assert.That(MidiNote.TryParseNoteName("Bb3", out int bb3), Is.True);
            Assert.That(bb3, Is.EqualTo(58));

            Assert.That(MidiNote.TryParseNoteName("Eb4", out int eb4), Is.True);
            Assert.That(eb4, Is.EqualTo(63));
        }

        [Test]
        public void TryParseNoteName_EnharmonicEquivalents_AreCorrect()
        {
            Assert.That(MidiNote.TryParseNoteName("B#3", out int bSharp3), Is.True);
            Assert.That(bSharp3, Is.EqualTo(60));

            Assert.That(MidiNote.TryParseNoteName("Cb4", out int cFlat4), Is.True);
            Assert.That(cFlat4, Is.EqualTo(59));
        }

        [Test]
        public void TryParseNoteName_InvalidInputs_ReturnFalse()
        {
            Assert.That(MidiNote.TryParseNoteName("H3", out _), Is.False);
            Assert.That(MidiNote.TryParseNoteName("C", out _), Is.False);
            Assert.That(MidiNote.TryParseNoteName("4", out _), Is.False);
            Assert.That(MidiNote.TryParseNoteName("", out _), Is.False);
            Assert.That(MidiNote.TryParseNoteName(null!, out _), Is.False);
        }
    }
}
