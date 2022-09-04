namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Describes the type of draw command enqueued.
    /// </summary>
    public enum ImDrawCommandType : int {
        Image = 1 << 0,
        Text = 1 << 1,
        Scissor = 1 << 2
    }
}
