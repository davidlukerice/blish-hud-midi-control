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
            using var thread = new KeySendThread((SendAction _) => { });
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
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            sender.Send(new MidiNoteEvent(66, isNoteOn: true), MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);

            // F#4 forces internal octave to 0
            Assert.That(sender.CurrentOctave, Is.EqualTo(0));
        }

        [Test]
        public void Send_FiresNoteProcessedEvent()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            KeySendResult? captured = null;
            sender.NoteProcessed += (evt, result) => captured = result;

            var note = new MidiNoteEvent(48, isNoteOn: true); // C3
            sender.Send(note, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(captured.Value.NewOctave, Is.EqualTo(0));
        }

        [Test]
        public void FreshInstance_StartsAtOctaveZero()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender1 = new KeySender(thread);
            sender1.Send(new MidiNoteEvent(66, isNoteOn: true), MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0);
            Assert.That(sender1.CurrentOctave, Is.EqualTo(0));

            // Simulating a keymap change: Module replaces the KeySender with a fresh instance.
            var sender2 = new KeySender(thread);
            Assert.That(sender2.CurrentOctave, Is.EqualTo(0), "Fresh KeySender must reset octave tracker to 0.");
        }

        // ---- Key Hold mode tests ----

        [Test]
        public void Resolve_KeyHold_NoteOn_ReturnsKeyDown()
        {
            var note = new MidiNoteEvent(48, isNoteOn: true); // C3
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50, enableKeyHold: true);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyDown));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x02u)); // "1"
        }

        [Test]
        public void Resolve_KeyHold_NoteOff_ReturnsKeyUp()
        {
            var note = new MidiNoteEvent(48, isNoteOn: false); // C3 off
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50, enableKeyHold: true);

            Assert.That(result.Actions, Has.Length.EqualTo(1));
            Assert.That(result.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyUp));
            Assert.That(result.Actions[0].ScanCode, Is.EqualTo(0x02u)); // "1"
        }

        [Test]
        public void Resolve_KeyHold_OctaveShift_IsKeyTap()
        {
            // D4 is octave 1, current octave 0 → shift up then note
            var note = new MidiNoteEvent(62, isNoteOn: true); // D4
            var result = KeySender.Resolve(note, MinstrelAutoKeymap.Instance, currentOctave: 0, autoSwap: true, shiftDelayMs: 50, enableKeyHold: true);

            Assert.That(result.Actions, Has.Length.EqualTo(2));
            Assert.That(result.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyTap)); // shift
            Assert.That(result.Actions[1].EventType, Is.EqualTo(KeyEventType.KeyDown)); // note
        }

        [Test]
        public void Send_KeyHold_DuplicateNoteOn_SuppressesSecondDown()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOn = new MidiNoteEvent(48, isNoteOn: true); // C3

            KeySendResult? first = null;
            KeySendResult? second = null;
            sender.NoteProcessed += (evt, result) =>
            {
                if (evt.NoteNumber == 48 && evt.IsNoteOn && first == null)
                    first = result;
                else if (evt.NoteNumber == 48 && evt.IsNoteOn)
                    second = result;
            };

            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(first.Value.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyDown));

            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Value.Actions, Is.Empty);
            Assert.That(second.Value.WasSuppressed, Is.True);
        }

        [Test]
        public void Send_KeyHold_NoteOffWhileStillHeld_SuppressesUp()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOn = new MidiNoteEvent(48, isNoteOn: true); // C3
            var noteOff = new MidiNoteEvent(48, isNoteOn: false);

            // Send note-on twice to get refcount = 2
            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            // Now note-off once; refcount should go to 1, no key-up sent
            KeySendResult? offResult = null;
            sender.NoteProcessed += (evt, result) =>
            {
                if (!evt.IsNoteOn)
                    offResult = result;
            };

            sender.Send(noteOff, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(offResult, Is.Not.Null);
            Assert.That(offResult!.Value.Actions, Is.Empty);
            Assert.That(offResult.Value.WasSuppressed, Is.True);
        }

        [Test]
        public void Send_KeyHold_NoteOffWithoutNoteOn_SendsKeyUp()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOff = new MidiNoteEvent(48, isNoteOn: false);

            KeySendResult? offResult = null;
            sender.NoteProcessed += (evt, result) => offResult = result;
            sender.Send(noteOff, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(offResult, Is.Not.Null);
            Assert.That(offResult!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(offResult.Value.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyUp));
            Assert.That(offResult.Value.WasSuppressed, Is.False);
        }

        [Test]
        public void Send_KeyHold_ReleaseAllHeldKeys_ClearsTracker()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOn = new MidiNoteEvent(48, isNoteOn: true);

            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            // Release all held keys clears the tracker
            sender.ReleaseAllHeldKeys();

            // Next note-on should send a new KeyDown because tracker is empty
            KeySendResult? afterRelease = null;
            sender.NoteProcessed += (evt, result) => afterRelease = result;
            sender.Send(noteOn, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(afterRelease, Is.Not.Null);
            Assert.That(afterRelease!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(afterRelease.Value.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyDown));
            Assert.That(afterRelease.Value.WasSuppressed, Is.False);
        }

        [Test]
        public void Send_KeyHold_NoteOffAfterOctaveShift_ReleasesOriginalKey()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOnC3 = new MidiNoteEvent(48, isNoteOn: true); // C3 -> "1"
            var noteOnD4 = new MidiNoteEvent(62, isNoteOn: true); // D4 -> shift up + "2"
            var noteOffC3 = new MidiNoteEvent(48, isNoteOn: false);

            KeySendResult? c3Up = null;
            sender.NoteProcessed += (evt, result) =>
            {
                if (evt.NoteNumber == 48 && !evt.IsNoteOn)
                    c3Up = result;
            };

            sender.Send(noteOnC3, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOnD4, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOffC3, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(c3Up, Is.Not.Null);
            Assert.That(c3Up!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(c3Up.Value.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyUp));
            Assert.That(c3Up.Value.Actions[0].ScanCode, Is.EqualTo(0x02u)); // still "1"
        }

        [Test]
        public void Send_KeyHold_AltOctaveMidHold_ReleasesAltKey()
        {
            using var thread = new KeySendThread((SendAction _) => { });
            thread.Start();

            var sender = new KeySender(thread);
            var noteOnC4 = new MidiNoteEvent(60, isNoteOn: true); // C4 at oct 0 -> alt key "8"
            var noteOnD4 = new MidiNoteEvent(62, isNoteOn: true); // D4 -> shift up + "2"
            var noteOffC4 = new MidiNoteEvent(60, isNoteOn: false);

            KeySendResult? c4Down = null;
            KeySendResult? c4Up = null;
            sender.NoteProcessed += (evt, result) =>
            {
                if (evt.NoteNumber == 60 && evt.IsNoteOn)
                    c4Down = result;
                else if (evt.NoteNumber == 60 && !evt.IsNoteOn)
                    c4Up = result;
            };

            sender.Send(noteOnC4, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOnD4, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);
            sender.Send(noteOffC4, MinstrelAutoKeymap.Instance, autoSwap: true, shiftDelayMs: 0, enableKeyHold: true);

            Assert.That(c4Down, Is.Not.Null);
            Assert.That(c4Down!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(c4Down.Value.Actions[0].ScanCode, Is.EqualTo(0x09u)); // "8" via alt octave

            Assert.That(c4Up, Is.Not.Null);
            Assert.That(c4Up!.Value.Actions, Has.Length.EqualTo(1));
            Assert.That(c4Up.Value.Actions[0].EventType, Is.EqualTo(KeyEventType.KeyUp));
            Assert.That(c4Up.Value.Actions[0].ScanCode, Is.EqualTo(0x09u)); // still "8", not "1"
        }
    }
}
