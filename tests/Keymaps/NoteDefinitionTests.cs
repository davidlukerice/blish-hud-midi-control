using NUnit.Framework;

namespace DavidRice.BlishHud.MidiControl.Tests
{
    [TestFixture]
    public class NoteDefinitionTests
    {
        [Test]
        public void CanCreateWithNullKey()
        {
            var note = new NoteDefinition(
                key: null,
                forceInternalOctave: 0);

            Assert.That(note.Key, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.EqualTo(0));
        }

        [Test]
        public void KeyDefaultsToNullWhenOmitted()
        {
            var note = new NoteDefinition(forceInternalOctave: 1);

            Assert.That(note.Key, Is.Null);
        }

        [Test]
        public void CanCreateWithRequiredFields()
        {
            var note = new NoteDefinition(key: "1", octave: 0);

            Assert.That(note.Key, Is.EqualTo("1"));
            Assert.That(note.Octave, Is.EqualTo(0));
        }

        [Test]
        public void CanCreateWithAltOctaveFields()
        {
            var note = new NoteDefinition(
                key: "1",
                octave: 1,
                altOctave: 0,
                altOctaveKey: "8");

            Assert.That(note.AltOctave, Is.EqualTo(0));
            Assert.That(note.AltOctaveKey, Is.EqualTo("8"));
        }

        [Test]
        public void CanCreateWithForceInternalOctave()
        {
            var note = new NoteDefinition(
                key: "9",
                octave: null,
                forceInternalOctave: 0);

            Assert.That(note.Key, Is.EqualTo("9"));
            Assert.That(note.Octave, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.EqualTo(0));
        }

        [Test]
        public void NullableFieldsDefaultToNull()
        {
            var note = new NoteDefinition(key: "2", octave: 1);

            Assert.That(note.AltOctave, Is.Null);
            Assert.That(note.AltOctaveKey, Is.Null);
            Assert.That(note.ForceInternalOctave, Is.Null);
        }
    }
}
