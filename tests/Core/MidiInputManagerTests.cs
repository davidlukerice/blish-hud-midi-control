using System;
using System.Collections.Generic;
using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Core;

namespace DavidRice.BlishHud.MidiControl.Tests.Core
{
    [TestFixture]
    public class MidiInputManagerTests
    {
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
