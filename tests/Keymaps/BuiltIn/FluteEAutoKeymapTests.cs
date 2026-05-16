using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class FluteEAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = FluteEAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("flute-e-auto"));
            Assert.That(keymap.Name, Is.EqualTo("Flute (E) (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapEnabled()
        {
            var keymap = FluteEAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void HasBothShiftKeysSetToSameKey()
        {
            var keymap = FluteEAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("9"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = FluteEAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(17));
        }

        [Test]
        public void E4_MapsToKey1Octave1()
        {
            var note = FluteEAutoKeymap.Instance.Notes["E4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(1));
        }

        [Test]
        public void E5_HasAltOctave()
        {
            var note = FluteEAutoKeymap.Instance.Notes["E5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(2));
            Assert.That(note.AltOctaveKey, Is.EqualTo("1"));
        }

        [Test]
        public void E6_MapsToKey8Octave2()
        {
            var note = FluteEAutoKeymap.Instance.Notes["E6"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(2));
        }

        [Test]
        public void F4_MapsTo9()
        {
            var note = FluteEAutoKeymap.Instance.Notes["F4"];

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void G4_MapsTo0()
        {
            var note = FluteEAutoKeymap.Instance.Notes["G4"];

            Assert.That(note.Key, Is.EqualTo("0"));
            Assert.That(note.Octave, Is.Null);
        }
    }
}
