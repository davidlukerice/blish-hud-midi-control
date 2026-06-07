using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class FrameDrumAutoKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = FrameDrumAutoKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("frame-drum-auto"));
            Assert.That(keymap.Name, Is.EqualTo("Frame Drum (Auto)"));
        }

        [Test]
        public void HasAutoOctaveSwapDisabled()
        {
            var keymap = FrameDrumAutoKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.False);
        }

        [Test]
        public void HasNoOctaveShiftKeys()
        {
            var keymap = FrameDrumAutoKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.Null);
            Assert.That(keymap.OctaveUpKey, Is.Null);
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = FrameDrumAutoKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(5));
        }

        [Test]
        public void C4_MapsToKey1()
        {
            var note = FrameDrumAutoKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void CSharp4_MapsToKey2()
        {
            var note = FrameDrumAutoKeymap.Instance.Notes["C#4"];

            Assert.That(note.Key, Is.EqualTo("2"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void D4_MapsToKey3()
        {
            var note = FrameDrumAutoKeymap.Instance.Notes["D4"];

            Assert.That(note.Key, Is.EqualTo("3"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void DSharp4_MapsToKey4()
        {
            var note = FrameDrumAutoKeymap.Instance.Notes["D#4"];

            Assert.That(note.Key, Is.EqualTo("4"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void E4_MapsToKey5()
        {
            var note = FrameDrumAutoKeymap.Instance.Notes["E4"];

            Assert.That(note.Key, Is.EqualTo("5"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }
    }
}
