using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class ChoirBellAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = ChoirBellAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("choir-bell-auto"));
            Assert.That(keymap.Name, Is.EqualTo("Magnanimous Choir Bell (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapEnabled()
        {
            var keymap = ChoirBellAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void HasOctaveShiftKeys()
        {
            var keymap = ChoirBellAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("0"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = ChoirBellAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(17));
        }

        [Test]
        public void C4_MapsToKey1Octave1()
        {
            var note = ChoirBellAutoKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(1));
        }

        [Test]
        public void C5_HasAltOctave()
        {
            var note = ChoirBellAutoKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(2));
            Assert.That(note.AltOctaveKey, Is.EqualTo("1"));
        }

        [Test]
        public void C6_MapsToKey8Octave2()
        {
            var note = ChoirBellAutoKeymap.Instance.Notes["C6"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(2));
        }

        [Test]
        public void CSharp4_IsManualOctaveDown()
        {
            var note = ChoirBellAutoKeymap.Instance.Notes["C#4"];

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void DSharp4_IsManualOctaveUp()
        {
            var note = ChoirBellAutoKeymap.Instance.Notes["D#4"];

            Assert.That(note.Key, Is.EqualTo("0"));
            Assert.That(note.Octave, Is.Null);
        }
    }
}
