using NUnit.Framework;

namespace DavidRice.BlishHud.MidiControl.Tests
{
    [TestFixture]
    public class KeymapTests
    {
        [Test]
        public void CanCreateWithIdAndName()
        {
            var keymap = new Keymap(id: "minstrel-auto", name: "The Minstrel (Auto)");

            Assert.That(keymap.Id, Is.EqualTo("minstrel-auto"));
            Assert.That(keymap.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void DefaultAutoOctaveSwapIsTrue()
        {
            var keymap = new Keymap(id: "test", name: "Test");

            Assert.That(keymap.AutoOctaveSwap, Is.True);
        }

        [Test]
        public void CanAddNotes()
        {
            var keymap = new Keymap(id: "test", name: "Test");
            keymap.Notes.Add("C4", new NoteDefinition(key: "1", octave: 1));

            Assert.That(keymap.Notes["C4"].Key, Is.EqualTo("1"));
        }

        [Test]
        public void OctaveDownAndUpKeysAreNullable()
        {
            var keymap = new Keymap(id: "test", name: "Test");

            Assert.That(keymap.OctaveDownKey, Is.Null);
            Assert.That(keymap.OctaveUpKey, Is.Null);
        }
    }
}
