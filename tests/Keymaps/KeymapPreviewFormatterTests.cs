#nullable enable

using System.Linq;
using DavidRice.BlishHud.MidiControl;
using DavidRice.BlishHud.MidiControl.Keymaps;
using NUnit.Framework;

namespace tests.Keymaps
{
    [TestFixture]
    public class KeymapPreviewFormatterTests
    {
        [Test]
        public void FormatLines_SameOctaveNote_FormatsWithOctave()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes = { { "C4", new NoteDefinition(key: "1", octave: 1) } }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines, Has.Count.EqualTo(1));
            Assert.That(lines[0], Is.EqualTo("C4 → 1 (oct 1)"));
        }

        [Test]
        public void FormatLines_AltOctave_FormatsAlt()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes = { { "C4", new NoteDefinition(key: "1", octave: 1, altOctave: 0, altOctaveKey: "8") } }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines[0], Is.EqualTo("C4 → 1 (oct 1) | alt: 8 on oct 0"));
        }

        [Test]
        public void FormatLines_OctaveShiftKey_FormatsOctShift()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes = { { "C#4", new NoteDefinition(key: "9") } }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines[0], Is.EqualTo("C#4 → 9 (oct shift)"));
        }

        [Test]
        public void FormatLines_ForceInternalOctave_FormatsInternal()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes = { { "F#4", new NoteDefinition(forceInternalOctave: 2) } }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines[0], Is.EqualTo("F#4 → internal octave: 2"));
        }

        [Test]
        public void FormatLines_UnmappedNote_Skips()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes = { { "X9", new NoteDefinition() } }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines, Is.Empty);
        }

        [Test]
        public void FormatLines_SortsByOctaveThenLetterThenAccidental()
        {
            var keymap = new Keymap("test", "Test")
            {
                Notes =
                {
                    { "C#4", new NoteDefinition(key: "x") },
                    { "C3",  new NoteDefinition(key: "1", octave: 0) },
                    { "C4",  new NoteDefinition(key: "1", octave: 1) },
                    { "D4",  new NoteDefinition(key: "2", octave: 1) },
                    { "Db4", new NoteDefinition(key: "y", octave: 1) },
                }
            };

            var lines = KeymapPreviewFormatter.FormatLines(keymap);

            Assert.That(lines, Is.EqualTo(new[]
            {
                "C3 → 1 (oct 0)",
                "C4 → 1 (oct 1)",
                "C#4 → x (oct shift)",
                "Db4 → y (oct 1)",
                "D4 → 2 (oct 1)",
            }));
        }

        [Test]
        public void FormatLines_MinstrelKeymap_MatchesExpectedOutput()
        {
            var keymap = DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn.MinstrelAutoKeymap.Instance;
            var lines = KeymapPreviewFormatter.FormatLines(keymap).ToList();

            // Spot-check a few known entries in sorted order.
            Assert.That(lines, Has.Member("C3 → 1 (oct 0)"));
            Assert.That(lines, Has.Member("C4 → 1 (oct 1) | alt: 8 on oct 0"));
            Assert.That(lines, Has.Member("C5 → 8 (oct 1) | alt: 1 on oct 2"));
            Assert.That(lines, Has.Member("C#4 → 9 (oct shift)"));
            Assert.That(lines, Has.Member("F#4 → internal octave: 0"));

            // Verify ordering: C3 (oct 3) should come before C4 (oct 4).
            int c3Index = lines.IndexOf("C3 → 1 (oct 0)");
            int c4Index = lines.IndexOf("C4 → 1 (oct 1) | alt: 8 on oct 0");
            Assert.That(c3Index, Is.LessThan(c4Index));
        }

        [Test]
        public void FormatLines_EmptyKeymap_ReturnsEmpty()
        {
            var keymap = new Keymap("empty", "Empty");
            var lines = KeymapPreviewFormatter.FormatLines(keymap);
            Assert.That(lines, Is.Empty);
        }

        [Test]
        public void FormatLines_NullKeymap_Throws()
        {
            Assert.That(() => KeymapPreviewFormatter.FormatLines(null!), Throws.ArgumentNullException);
        }
    }
}
