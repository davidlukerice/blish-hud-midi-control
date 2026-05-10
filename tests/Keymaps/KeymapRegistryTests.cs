using NUnit.Framework;
using DavidRice.BlishHud.MidiControl.Keymaps;

namespace DavidRice.BlishHud.MidiControl.Tests.Keymaps
{
    [TestFixture]
    public class KeymapRegistryTests
    {
        [Test]
        public void ConstructorRegistersMinstrelAuto()
        {
            var registry = new KeymapRegistry();

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(1));
            Assert.That(registry.AllKeymaps[0].Id, Is.EqualTo("minstrel-auto"));
            Assert.That(registry.AllKeymaps[0].Name, Is.EqualTo("The Minstrel (Auto)"));
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

            Assert.That(registry.AllKeymaps.Count, Is.EqualTo(2));
            Assert.That(registry.FindById("custom-1"), Is.SameAs(custom));
        }
    }
}
