using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class VerdarachAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = VerdarachAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("verdarach-auto"));
            Assert.That(keymap.Name, Is.EqualTo("Verdarach (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapEnabled()
        {
            var keymap = VerdarachAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void HasOctaveShiftKeys()
        {
            var keymap = VerdarachAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("0"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = VerdarachAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(22));
        }

        [Test]
        public void C3_MapsToKey1Octave0()
        {
            var note = VerdarachAutoKeymap.Instance.Notes["C3"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void C4_HasAltOctave()
        {
            var note = VerdarachAutoKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(0));
            Assert.That(note.AltOctaveKey, Is.EqualTo("8"));
        }

        [Test]
        public void C5_HasAltOctave()
        {
            var note = VerdarachAutoKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(2));
            Assert.That(note.AltOctaveKey, Is.EqualTo("1"));
        }

        [Test]
        public void C6_MapsToKey8Octave2()
        {
            var note = VerdarachAutoKeymap.Instance.Notes["C6"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(2));
        }
    }
}
