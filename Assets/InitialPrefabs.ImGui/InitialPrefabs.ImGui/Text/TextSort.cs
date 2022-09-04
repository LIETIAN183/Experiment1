using System.Collections.Generic;

namespace InitialPrefabs.NimGui.Text {

    /// <summary>
    /// Convenience struct to easily compare two glyphs' relative order.
    /// </summary>
    public struct GlyphComparer : IComparer<ImGlyph> {
        public int Compare(ImGlyph x, ImGlyph y) {
            return x.CompareTo(y);
        }
    }
}
