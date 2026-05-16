using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class MinstrelKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = MinstrelKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("minstrel"));
            Assert.That(keymap.Name, Is.EqualTo("The Minstrel"));
        }

        [Test]
        public void HasAutoOctaveSwapDisabled()
        {
            var keymap = MinstrelKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.False);
        }

        [Test]
        public void HasNoOctaveShiftKeys()
        {
            var keymap = MinstrelKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.Null);
            Assert.That(keymap.OctaveUpKey, Is.Null);
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = MinstrelKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(10));
        }

        [Test]
        public void C4_MapsToKey1()
        {
            var note = MinstrelKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void C5_MapsToKey8()
        {
            var note = MinstrelKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void CSharp4_MapsTo9()
        {
            var note = MinstrelKeymap.Instance.Notes["C#4"];

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void DSharp4_MapsTo0()
        {
            var note = MinstrelKeymap.Instance.Notes["D#4"];

            Assert.That(note.Key, Is.EqualTo("0"));
            Assert.That(note.Octave, Is.Null);
        }
    }
}
