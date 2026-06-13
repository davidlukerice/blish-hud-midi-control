using System;
using System.Collections.Generic;
using NAudio.Midi;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Core;

namespace DavidRice.BlishHud.MidiControl.Tests.Core
{
    [TestFixture]
    public class MidiInputManagerTests
    {
        [Test]
        public void TryConvertToMidiNoteEvent_NoteOnWithVelocity_ReturnsNoteOn()
        {
            var noteOn = new NoteOnEvent(0, 1, 60, 100, 0);
            var result = MidiEventConverter.TryConvertToMidiNoteEvent(noteOn);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.NoteNumber, Is.EqualTo(60));
            Assert.That(result.Value.IsNoteOn, Is.True);
        }

        [Test]
        public void TryConvertToMidiNoteEvent_NoteOnWithZeroVelocity_ReturnsNoteOff()
        {
            var noteOn = new NoteOnEvent(0, 1, 60, 0, 0);
            var result = MidiEventConverter.TryConvertToMidiNoteEvent(noteOn);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.NoteNumber, Is.EqualTo(60));
            Assert.That(result.Value.IsNoteOn, Is.False);
        }

        [Test]
        public void TryConvertToMidiNoteEvent_NoteOffEvent_ReturnsNoteOff()
        {
            var noteOff = new NoteEvent(0, 1, MidiCommandCode.NoteOff, 60, 0);
            var result = MidiEventConverter.TryConvertToMidiNoteEvent(noteOff);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.NoteNumber, Is.EqualTo(60));
            Assert.That(result.Value.IsNoteOn, Is.False);
        }

        [Test]
        public void TryConvertToMidiNoteEvent_KeyAfterTouch_ReturnsNull()
        {
            var afterTouch = new NoteEvent(0, 1, MidiCommandCode.KeyAfterTouch, 60, 100);
            var result = MidiEventConverter.TryConvertToMidiNoteEvent(afterTouch);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryConvertToMidiNoteEvent_NullEvent_ReturnsNull()
        {
            var result = MidiEventConverter.TryConvertToMidiNoteEvent(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void EvaluateConnection_TargetIsNull_ReturnsNoAction()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: null,
                availableDevices: new[] { "My Device" },
                isDeviceOpen: true);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.NoAction));
        }

        [Test]
        public void EvaluateConnection_TargetIsEmpty_ReturnsNoAction()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "",
                availableDevices: new[] { "My Device" },
                isDeviceOpen: true);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.NoAction));
        }

        [Test]
        public void EvaluateConnection_OpenAndInList_ReturnsNoAction()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: new[] { "Other", "My Device" },
                isDeviceOpen: true);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.NoAction));
        }

        [Test]
        public void EvaluateConnection_OpenAndNotInList_ReturnsClose()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: new[] { "Other", "Another" },
                isDeviceOpen: true);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.Close));
        }

        [Test]
        public void EvaluateConnection_OpenAndNoDevices_ReturnsClose()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: Array.Empty<string>(),
                isDeviceOpen: true);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.Close));
        }

        [Test]
        public void EvaluateConnection_NotOpenAndInList_ReturnsReopen()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: new[] { "My Device" },
                isDeviceOpen: false);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.Reopen));
        }

        [Test]
        public void EvaluateConnection_NotOpenAndNotInList_ReturnsNoAction()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: new[] { "Other" },
                isDeviceOpen: false);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.NoAction));
        }

        [Test]
        public void EvaluateConnection_NotOpenAndNoDevices_ReturnsNoAction()
        {
            var result = ConnectionEvaluator.Evaluate(
                targetDeviceName: "My Device",
                availableDevices: Array.Empty<string>(),
                isDeviceOpen: false);

            Assert.That(result, Is.EqualTo(ConnectionEvaluator.Action.NoAction));
        }
    }
}
