using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.BuiltIn
{
    [TestFixture]
    public class GeneralKeymapTests
    {
        [Test]
        public void HasCorrectIdAndName()
        {
            var keymap = GeneralKeymap.Instance;

            Assert.That(keymap.Id, Is.EqualTo("general"));
            Assert.That(keymap.Name, Is.EqualTo("General (Manual)"));
        }

        [Test]
        public void HasAutoOctaveSwapDisabled()
        {
            var keymap = GeneralKeymap.Instance;

            Assert.That(keymap.AutoOctaveSwap, Is.False);
        }

        [Test]
        public void HasOctaveShiftKeys()
        {
            var keymap = GeneralKeymap.Instance;

            Assert.That(keymap.OctaveDownKey, Is.EqualTo("9"));
            Assert.That(keymap.OctaveUpKey, Is.EqualTo("0"));
        }

        [Test]
        public void HasExpectedNoteCount()
        {
            var keymap = GeneralKeymap.Instance;

            Assert.That(keymap.Notes.Count, Is.EqualTo(15));
        }

        [Test]
        public void C4_MapsToKey1()
        {
            var note = GeneralKeymap.Instance.Notes["C4"];

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void D4_MapsToKey2()
        {
            var note = GeneralKeymap.Instance.Notes["D4"];

            Assert.That(note.Key, Is.EqualTo("2"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void E4_MapsToKey3()
        {
            var note = GeneralKeymap.Instance.Notes["E4"];

            Assert.That(note.Key, Is.EqualTo("3"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void D5_IsOctaveDownShiftKey()
        {
            var note = GeneralKeymap.Instance.Notes["D5"];

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void E5_IsOctaveUpShiftKey()
        {
            var note = GeneralKeymap.Instance.Notes["E5"];

            Assert.That(note.Key, Is.EqualTo("0"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void C5_MapsToKey8()
        {
            var note = GeneralKeymap.Instance.Notes["C5"];

            Assert.That(note.Key, Is.EqualTo("8"));
            Assert.That(note.Octave, Is.Null);
        }

        [Test]
        public void NoNoteHasOctaveProperty()
        {
            var keymap = GeneralKeymap.Instance;

            foreach (var kvp in keymap.Notes)
            {
                Assert.That(kvp.Value.Octave, Is.Null,
                    $"Note {kvp.Key} should not have an octave property in manual mode.");
            }
        }
    }
}
