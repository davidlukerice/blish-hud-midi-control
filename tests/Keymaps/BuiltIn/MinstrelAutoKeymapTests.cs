using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class MinstrelAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = MinstrelAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("minstrel-auto"));
            Assert.That(keymap.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapEnabled()
        {
            var keymap = MinstrelAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void HasOctaveShiftKeys()
        {
            var keymap = MinstrelAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("0"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = MinstrelAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(27));
        }

        [Test]
        public void C3_MapsToKey1Octave0()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["C3"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void C4_HasAltOctave()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(0));
            Assert.That(note.AltOctaveKey, Is.EqualTo("8"));
        }

        [Test]
        public void C5_HasAltOctave()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.EqualTo(1));
            Assert.That(note.AltOctave, Is.EqualTo(2));
            Assert.That(note.AltOctaveKey, Is.EqualTo("1"));
        }

        [Test]
        public void CSharp4_IsManualOctaveDown()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["C#4"];

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.Null);
        }

        [Test]
        public void DSharp4_IsManualOctaveUp()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["D#4"];

            Assert.That(note.Key, Is.EqualTo("0"));
            Assert.That(note.Octave, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.Null);
        }

        [Test]
        public void FSharp4_ForcesInternalOctave0()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["F#4"];

            Assert.That(note.Key, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.EqualTo(0));
        }

        [Test]
        public void GSharp4_ForcesInternalOctave1()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["G#4"];

            Assert.That(note.Key, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.EqualTo(1));
        }

        [Test]
        public void ASharp4_ForcesInternalOctave2()
        {
            var note = MinstrelAutoKeymap.Instance.Notes["A#4"];

            Assert.That(note.Key, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.EqualTo(2));
        }
    }
}
