using NUnit.Framework;

namespace InitialPrefabs.NimGui.Tests {

    public unsafe class ImGuiContextInitializationTests {

        [Test]
        public void InitializedAndReleased() {
            ImGuiContext.Initialize();
#if URP_ENABLED
            Assert.IsNotNull(ImGuiContext.ImGuiRenderFeature, "RenderPipeline not found");
#endif
            Assert.AreEqual(1, ImGuiContext.All().Count, "Window not initialized");
            Assert.AreEqual(1, ImGuiContext.Windows.Count);
            ImGuiContext.Release();

            Assert.AreEqual(0, ImGuiContext.All().Count, "Window not initialized");
        }
    }
}
