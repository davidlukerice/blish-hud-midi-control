using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps
{
    [TestFixture]
    public class KeymapRegistryTests
    {
        [Test]
        public void ConstructorRegistersAllBuiltInKeymaps()
        {
            var registry = new KeymapRegistry();

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(10));
        }

        [Test]
        public void ConstructorRegistersMinstrelAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void ConstructorRegistersGrandPianoAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("grand-piano-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Ornate Grand Piano (Auto)"));
        }

        [Test]
        public void ConstructorRegistersChoirBellAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("choir-bell-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Magnanimous Choir Bell (Auto)"));
        }

        [Test]
        public void ConstructorRegistersFluteCAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("flute-c-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Flute (C) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersFluteEAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("flute-e-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Flute (E) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersLuteAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("lute-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Lute (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHarpAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("harp-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Harp (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHornCAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("horn-c-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Horn (C) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersHornEAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("horn-e-auto");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Horn (E) (Auto)"));
        }

        [Test]
        public void ConstructorRegistersMinstrelNonAuto()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel");
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel"));
        }

        [Test]
        public void FindById_ReturnsMatch()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("minstrel-auto");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("The Minstrel (Auto)"));
        }

        [Test]
        public void FindById_ReturnsNull_WhenNotFound()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindById("nonexistent");

            Assert.That(found, Is.Null);
        }

        [Test]
        public void FindByName_ReturnsMatch()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindByName("The Minstrel (Auto)");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo("minstrel-auto"));
        }

        [Test]
        public void FindByName_ReturnsNull_WhenNotFound()
        {
            var registry = new KeymapRegistry();

            var found = registry.FindByName("Nonexistent Keymap");

            Assert.That(found, Is.Null);
        }

        [Test]
        public void Register_AddsKeymap()
        {
            var registry = new KeymapRegistry();
            var custom = new Keymap(id: "custom-1", name: "My Custom Keymap");

            registry.Register(custom);

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(11));
            Assert.That(registry.FindById("custom-1"), Is.SameAs(custom));
        }
    }
}
