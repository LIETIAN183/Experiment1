using UnityEngine;

namespace InitialPrefabs.NimGui.Text {
    
    /// <summary>
    /// A project wide asset which stores the FontFace and 
    /// potential glyphs that the font can render.
    /// 
    /// <remarks>
    /// This should generally only be constructed in the Editor.
    /// </remarks>
    /// </summary>
    public class SerializedFontData : ScriptableObject {
        
        public ImFontFace FontFaceInfo;

        public ImGlyph[] Glyphs;
    }
}
