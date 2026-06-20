#nullable enable

using System.Linq;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl;
using DavidRice.BlishHud.MidiControl.Keymaps.Visualization;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps.Visualization
{
    [TestFixture]
    public class KeybedLayoutCalculatorTests
    {
        [Test]
        public void Calculate_EmptyKeymap_ReturnsEmptyLayout()
        {
            var keymap = new Keymap("empty", "Empty");

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            Assert.That(layout.IsEmpty, Is.True);
            Assert.That(layout.Keys, Is.Empty);
        }

        [Test]
        public void Calculate_SingleOctaveKeymap_IncludesFullOctave()
        {
            var keymap = new Keymap("single", "Single")
            {
                Notes =
                {
                    { "C3", new NoteDefinition(key: "1", octave: 0) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            Assert.That(layout.IsEmpty, Is.False);
            Assert.That(layout.Keys, Has.Count.EqualTo(12));
            Assert.That(layout.StartOctave, Is.EqualTo(3));
            Assert.That(layout.EndOctave, Is.EqualTo(3));

            var c3 = layout.Keys.First(k => k.NoteName == "C3");
            Assert.That(c3.IsMapped, Is.True);
            Assert.That(c3.Gw2Key, Is.EqualTo("1"));
            Assert.That(c3.Octave, Is.EqualTo(0));

            var d3 = layout.Keys.First(k => k.NoteName == "D3");
            Assert.That(d3.IsMapped, Is.False);
        }

        [Test]
        public void Calculate_MultiOctaveKeymap_IncludesIntermediateEmptyOctaves()
        {
            var keymap = new Keymap("multi", "Multi")
            {
                Notes =
                {
                    { "C3", new NoteDefinition(key: "1", octave: 0) },
                    { "C5", new NoteDefinition(key: "8", octave: 2) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            Assert.That(layout.Keys, Has.Count.EqualTo(36));
            Assert.That(layout.StartOctave, Is.EqualTo(3));
            Assert.That(layout.EndOctave, Is.EqualTo(5));

            Assert.That(layout.Keys.Any(k => k.NoteName == "C4"), Is.True);
            Assert.That(layout.Keys.Any(k => k.NoteName == "B4"), Is.True);
        }

        [Test]
        public void Calculate_NonContiguousOctaves_IncludesFullSpan()
        {
            var keymap = new Keymap("span", "Span")
            {
                Notes =
                {
                    { "E3", new NoteDefinition(key: "3", octave: 0) },
                    { "G4", new NoteDefinition(key: "5", octave: 1) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            Assert.That(layout.Keys, Has.Count.EqualTo(24));
            Assert.That(layout.StartOctave, Is.EqualTo(3));
            Assert.That(layout.EndOctave, Is.EqualTo(4));

            Assert.That(layout.Keys.Any(k => k.NoteName == "C3"), Is.True);
            Assert.That(layout.Keys.Any(k => k.NoteName == "B4"), Is.True);
        }

        [Test]
        public void Calculate_BlackAndWhiteKeys_AreIdentified()
        {
            var keymap = new Keymap("chromatic", "Chromatic")
            {
                Notes =
                {
                    { "C3", new NoteDefinition(key: "1", octave: 0) },
                    { "C#3", new NoteDefinition(key: "F1", octave: 0) },
                    { "D3", new NoteDefinition(key: "2", octave: 0) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            Assert.That(layout.Keys.First(k => k.NoteName == "C3").IsBlackKey, Is.False);
            Assert.That(layout.Keys.First(k => k.NoteName == "C#3").IsBlackKey, Is.True);
            Assert.That(layout.Keys.First(k => k.NoteName == "D3").IsBlackKey, Is.False);
        }

        [Test]
        public void Calculate_KeySwitch_DetectedFromOctaveKeys()
        {
            var keymap = new Keymap("switches", "Switches")
            {
                OctaveDownKey = "9",
                OctaveUpKey = "0",
                Notes =
                {
                    { "C#3", new NoteDefinition(key: "9") },
                    { "D#3", new NoteDefinition(key: "0") },
                    { "C3", new NoteDefinition(key: "1", octave: 0) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            var cSharp = layout.Keys.First(k => k.NoteName == "C#3");
            Assert.That(cSharp.IsMapped, Is.True);
            Assert.That(cSharp.IsKeySwitch, Is.True);

            var dSharp = layout.Keys.First(k => k.NoteName == "D#3");
            Assert.That(dSharp.IsKeySwitch, Is.True);

            var c3 = layout.Keys.First(k => k.NoteName == "C3");
            Assert.That(c3.IsKeySwitch, Is.False);
        }

        [Test]
        public void Calculate_AltOctave_IsPreserved()
        {
            var keymap = new Keymap("alt", "Alt")
            {
                Notes =
                {
                    { "C4", new NoteDefinition(key: "1", octave: 1, altOctave: 0, altOctaveKey: "8") }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            var c4 = layout.Keys.First(k => k.NoteName == "C4");
            Assert.That(c4.HasAltOctave, Is.True);
            Assert.That(c4.AltOctave, Is.EqualTo(0));
            Assert.That(c4.AltOctaveKey, Is.EqualTo("8"));
        }

        [Test]
        public void Calculate_FlatNoteName_ParsesCorrectly()
        {
            var keymap = new Keymap("flat", "Flat")
            {
                Notes =
                {
                    { "Bb3", new NoteDefinition(key: "7", octave: 0) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            var bb3 = layout.Keys.First(k => k.NoteName == "Bb3");
            Assert.That(bb3.IsMapped, Is.True);
            Assert.That(bb3.NoteNumber, Is.EqualTo(58));
        }

        [Test]
        public void Calculate_Keys_AreOrderedByNoteNumber()
        {
            var keymap = new Keymap("ordered", "Ordered")
            {
                Notes =
                {
                    { "G3", new NoteDefinition(key: "5", octave: 0) },
                    { "C3", new NoteDefinition(key: "1", octave: 0) }
                }
            };

            var layout = KeybedLayoutCalculator.Calculate(keymap);

            var noteNumbers = layout.Keys.Select(k => k.NoteNumber).ToList();
            Assert.That(noteNumbers, Is.Ordered.Ascending);
            Assert.That(noteNumbers.First(), Is.EqualTo(48)); // C3
            Assert.That(noteNumbers.Last(), Is.EqualTo(59)); // B3
        }
    }
}
