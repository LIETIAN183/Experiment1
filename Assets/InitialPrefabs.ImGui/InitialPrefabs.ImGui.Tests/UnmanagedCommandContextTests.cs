using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using NUnit.Framework;
using UnityEngine;

namespace InitialPrefabs.NimGui.Tests {

    public unsafe class UnmanagedCommandContextTests {

        ImDrawBuilder cmds;
        const string StringValue = "Test";

        [SetUp]
        public void Setup() {
            cmds = new ImDrawBuilder(2, 10);
            Assert.True(cmds.Commands.IsCreated());
        }

        [TearDown]
        public void TearDown() {
            cmds.Dispose();
            Assert.False(cmds.Commands.IsCreated());
        }

        [Test]
        public void CommandsAddedToContext() {
            var cmd = cmds.Peek();
            cmd.Push(new ImSpriteData());
            cmd.Push(ImDrawCommandType.Image, new ImRect(), Color.blue, 0);
            cmd.Push(new ImString(StringValue), 10);

            Assert.AreEqual(1, cmd.DrawCommands->Length);
            Assert.AreEqual(1, cmd.SpriteCommands->Length);
            Assert.AreEqual(1, cmd.TextCommands->Length);

            cmds.Clear();

            Assert.AreEqual(0, cmd.DrawCommands->Length);
            Assert.AreEqual(0, cmd.SpriteCommands->Length);
            Assert.AreEqual(0, cmd.TextCommands->Length);
        }

        [Test]
        public void CommandsAddedToNextContext() {
            cmds.Next();
            var cmd = cmds.Peek();
            cmd.Push(new ImSpriteData());
            cmd.Push(ImDrawCommandType.Image, new ImRect(), Color.blue, 0);
            cmd.Push(new ImString(StringValue), 10);

            Assert.AreEqual(1, cmd.DrawCommands->Length);
            Assert.AreEqual(1, cmd.SpriteCommands->Length);
            Assert.AreEqual(1, cmd.TextCommands->Length);

            var root = cmds.Root();
            cmds.Previous();
            var peeked = cmds.Peek();
            Assert.AreEqual(root, peeked);
            Assert.AreNotEqual(root, cmd);
        }

        [Test]
        public void Consolidates() {
            cmds.Next();
            var cmd = cmds.Peek();
            cmd.Push(new ImSpriteData());
            cmd.Push(ImDrawCommandType.Image, new ImRect(), Color.blue, 0);
            cmd.Push(new ImString(StringValue), 10);

            Assert.AreEqual(1, cmd.DrawCommands->Length);
            Assert.AreEqual(1, cmd.SpriteCommands->Length);
            Assert.AreEqual(1, cmd.TextCommands->Length);

            var root = cmds.Root();
            Assert.AreNotEqual(root, cmd);

            cmds.Consolidate();

            cmd = cmds.Root();

            Assert.AreEqual(1, cmd.DrawCommands->Length);
            Assert.AreEqual(1, cmd.SpriteCommands->Length);
            Assert.AreEqual(1, cmd.TextCommands->Length);
        }
    }
}
