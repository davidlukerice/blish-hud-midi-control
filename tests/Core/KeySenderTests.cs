#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Core;
using DavidRice.BlishHud.MidiControl.Keymaps;
using DavidRice.BlishHud.MidiControl.Keymaps.BuiltIn;
using DavidRice.BlishHud.MidiControl.Input;

namespace DavidRice.BlishHud.MidiControl.Tests.Core
{
    [TestFixture]
    public class KeySenderTests
    {
        // ---- Resolve (pure function tests) ----

        [Test]
        public void Resolve_NoteOn_SameOctave_SendsKey()
        {
            // C3 is in octave 0, key "1" → scan code 0x02
            var note = new MidiNoteEvent(48, isNoteOn: true); // C3
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x02u)); // "1"
            Assert.That(result.Actions[0].DelayAfterMs, Is.EqualTo(0));
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_NoteOff_ReturnsEmpty()
        {
            var note = new MidiNoteEvent(48, isNoteOn: false); // C3 off
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Is.Empty);
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_UnknownNote_ReturnsEmpty()
        {
            // A note way outside the minstrel range
            var note = new MidiNoteEvent(127, isNoteOn: true);
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Is.Empty);
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_ForceInternalOctave_SetsOctave_NoKey()
        {
            // F#4 forces internal octave to 0, sends no key
            var note = new MidiNoteEvent(66, isNoteOn: true); // F#4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 1, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Is.Empty);
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_ManualOctaveDown_SendsKey_And_DecrementsOctave()
        {
            // C#4 maps to key "9" (octave down) with no octave property
            var note = new MidiNoteEvent(61, isNoteOn: true); // C#4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 1, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x0Au)); // "9"
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_ManualOctaveUp_SendsKey_And_IncrementsOctave()
        {
            // D#4 maps to key "0" (octave up) with no octave property
            var note = new MidiNoteEvent(63, isNoteOn: true); // D#4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 1, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x0Bu)); // "0"
            Assert.That(result.NewOctave, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_AutoSwap_NoteInSameOctave_NoShift()
        {
            // D3 is octave 0, current octave is 0 → no shift needed
            var note = new MidiNoteEvent(50, isNoteOn: true); // D3
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x03u)); // "2"
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_AutoSwap_NoteInHigherOctave_SendsShiftThenKey()
        {
            // D4 is octave 1, current octave is 0 → shift up once
            // "0" = octave up = scan code 0x0B
            // "2" = note key = scan code 0x03
            var note = new MidiNoteEvent(62, isNoteOn: true); // D4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(2));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x0Bu)); // "0" shift up
            Assert.That(result.Actions[0].DelayAfterMs, Is.EqualTo(0)); // last before note
            Assert.That(result.Actions[1].ScanCode, Is.EqualTo(0x03u)); // "2"
            Assert.That(result.NewOctave, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_AutoSwap_NoteInLowerOctave_SendsShiftThenKey()
        {
            // C3 is octave 0, current octave is 1 → shift down once
            // "9" = octave down = scan code 0x0A
            var note = new MidiNoteEvent(48, isNoteOn: true); // C3
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 1, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(2));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x0Au)); // "9" shift down
            Assert.That(result.Actions[1].ScanCode, Is.EqualTo(0x02u)); // "1"
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_AutoSwap_MultiOctaveAddsDelay()
        {
            // D5 is octave 2, current octave is 0 → shift up twice
            var note = new MidiNoteEvent(74, isNoteOn: true); // D5
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 75);

            // Two shifts + one note = 3 actions
            Assert.That(result.Actions, Has.Length.EqualTo(3));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x0Bu)); // "0" up
            Assert.That(result.Actions[0].DelayAfterMs, Is.EqualTo(75)); // delay between shifts
            Assert.That(result.Actions[1].ScanCode, Is.EqualTo(0x0Bu)); // "0" up
            Assert.That(result.Actions[1].DelayAfterMs, Is.EqualTo(0)); // last before note
            Assert.That(result.Actions[2].ScanCode, Is.EqualTo(0x03u)); // "2"
            Assert.That(result.NewOctave, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_AutoSwap_AltOctaveAvailable_AvoidsShift()
        {
            // C4 is normally octave 1, key "1".
            // But it has altOctave 0 with key "8".
            // Current octave 0 → should use alt key "8" instead of shifting.
            var note = new MidiNoteEvent(60, isNoteOn: true); // C4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x09u)); // "8" via alt octave
            Assert.That(result.NewOctave, Is.EqualTo(0)); // octave unchanged
        }

        [Test]
        public void Resolve_AutoSwap_C5_AtOctave2_UsesAlt()
        {
            // C5 is normally octave 1, key "8".
            // But it has altOctave 2 with key "1".
            // Current octave 2 → should use alt key "1".
            var note = new MidiNoteEvent(72, isNoteOn: true); // C5
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 2, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x02u)); // "1" via alt octave
            Assert.That(result.NewOctave, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_AutoSwapDisabled_NoShift()
        {
            // D4 is octave 1. Current octave is 0.
            // With autoSwap disabled, should just send the key without shifting.
            var note = new MidiNoteEvent(62, isNoteOn: true); // D4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: false, shiftDelayMs: 50);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x03u)); // "2"
            Assert.That(result.NewOctave, Is.EqualTo(0)); // octave unchanged
        }

        [Test]
        public void Resolve_UnknownScanCode_IsSkipped()
        {
            // Create a keymap that references a key not in KeyToScanCode
            var badKeymap = new Keymap("bad", "Bad")
            {
                OctaveUpKey = "9",
                Notes =
                {
                    { "C3", new NoteDefinition(key: "NOT_A_KEY", octave: 0) }
                }
            };

            var note = new MidiNoteEvent(48, isNoteOn: true); // C3
            var result = KeySender.Resolve(note, badKeymap, currentOctave: 0, autoSwap: true, shiftDelayMs: 50);

            Assert.That(result.Actions, Is.Empty);
            Assert.That(result.NewOctave, Is.EqualTo(0));
        }

        // ---- Send (integration with KeySendThread) ----

        [Test]
        public void Send_QueuesActionsIntoThread()
        {
            using var thread = new KeySendThread(_ => { });
            thread.Start();

            var sender = new KeySender(thread);
            var note = new MidiNoteEvent(48, isNoteOn: true); // C3

            sender.Send(note, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);

            Thread.Sleep(100);
            thread.Shutdown();
        }

        [Test]
        public void Send_UpdatesCurrentOctave()
        {
            using var thread = new KeySendThread(_ => { });
            thread.Start();

            var sender = new KeySender(thread);
            sender.Send(new MidiNoteEvent(66, isNoteOn: true), MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);

            // F#4 forces internal octave to 0
            Assert.That(sender.CurrentOctave, Is.EqualTo(0));
        }

        [Test]
        public void FreshInstance_StartsAtOctaveZero()
        {
            using var thread = new KeySendThread(_ => { });
            thread.Start();

            var sender1 = new KeySender(thread);
            sender1.Send(new MidiNoteEvent(66, isNoteOn: true), MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);
            Assert.That(sender1.CurrentOctave, Is.EqualTo(0));

            // Simulating a keymap change: Module replaces the KeySender with a fresh instance.
            var sender2 = new KeySender(thread);
            Assert.That(sender2.CurrentOctave, Is.EqualTo(0), "Fresh KeySender must reset octave tracker to 0.");
        }
    }
}
