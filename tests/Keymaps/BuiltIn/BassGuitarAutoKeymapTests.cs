using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class BassGuitarAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = BassGuitarAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("bass-guitar-auto"));
            Assert.That(keymap.Name, Is.EqualTo("Bass Guitar (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapEnabled()
        {
            var keymap = BassGuitarAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void HasOctaveShiftKeys()
        {
            var keymap = BassGuitarAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("0"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = BassGuitarAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(15));
        }

        [Test]
        public void C3_MapsToKey1Octave0()
        {
            var note = BassGuitarAutoKeymap.Instance.Notes["C3"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void C4_HasAltOctave()
        {
            var note = BassGuitarAutoKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(0));
            Assert.That(note.AltOctaveKey, Is.EqualTo("8"));
        }

        [Test]
        public void C5_MapsToKey8Octave1_NoAltOctave()
        {
            var note = BassGuitarAutoKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.Null);
            Assert.That(note.AltOctaveKey, Is.Null);
        }
    }
}
