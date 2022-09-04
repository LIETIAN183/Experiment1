using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.TextCore;

namespace InitialPrefabs.NimGui.Text {

    /// <summary>
    /// Stores description of how the font is laid out.
    /// </summary>
    [Serializable]
    public struct ImFontFace {

        public float AscentLine;
        public float BaseLine;
        public float CapLine;
        public float DescentLine;
        public float LineHeight;
        public float MeanLine;
        public float PointSize;
        public float Scale;
        public float StrikeThroughOffset;
        public float StrikeThroughThickness;
        public float SubscriptSize;
        public float SubscriptOffset;
        public float SuperscriptSize;
        public float SuperscriptOffset;
        public float TabWidth;
        public float UnderlineOffset;

        /// <summary>
        /// Constructs a FontFace from UnityEngine.TextCore's FaceInfo.
        /// </summary>
        /// <param name="info">The FaceInfo to construct from.</param>
        /// <returns>A copy of the FaceInfo into a FontFace struct.</returns>
        public static ImFontFace Create(FaceInfo info) {
            return new ImFontFace {
                AscentLine             = info.ascentLine,
                BaseLine               = info.baseline,
                CapLine                = info.capLine,
                DescentLine            = info.descentLine,
                LineHeight             = info.lineHeight,
                MeanLine               = info.meanLine,
                PointSize              = info.pointSize,
                Scale                  = info.scale,
                StrikeThroughThickness = info.strikethroughThickness,
                StrikeThroughOffset    = info.strikethroughThickness,
                SubscriptSize          = info.subscriptSize,
                SubscriptOffset        = info.subscriptOffset,
                SuperscriptSize        = info.superscriptSize,
                SuperscriptOffset      = info.superscriptOffset,
                TabWidth               = info.tabWidth,
                UnderlineOffset        = info.underlineOffset,
            };
        }
    }

    public static class FontFaceExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateLineHeight(
            this in ImFontFace fontFace, int fontSize, float yPadding) {

            return fontFace.LineHeight * (fontSize / fontFace.PointSize) + yPadding;
        }
    }

    /// <summary>
    /// A glyph stores metrics of each character in the font. This 
    /// describes how each character is laid out and how much space
    /// exists between each character.
    /// </summary>
    [Serializable]
    public struct ImGlyph : IComparable<ImGlyph> {
        public uint Unicode;
        #if UNITY_EDITOR
        public Char Char;
        #endif

        /// <summary>
        /// The spacing between the left edge of the character 
        /// to the next character.
        /// </summary>
        public float Advance;

        /// <summary>
        /// How big is the rectangle for the font?
        /// </summary>
        public float2 MetricsSize;

        /// <summary>
        /// X bearing store the spacing between the previous rectangle 
        /// and the character. While y bearing store the offset from the 
        /// baseline to the top of the rectangle.
        /// </summary>
        public float2 Bearings;

        /// <summary>
        /// Stores the texture coordinates to render the font.
        /// XY stores the min, while zw stores the max.
        /// </summary>
        public float4 Uvs;

        public int CompareTo(ImGlyph other) {
            return Unicode.CompareTo(other.Unicode);
        }

        public static implicit operator ImGlyph(char c) => new ImGlyph { Unicode = (uint)c };
    }
}
