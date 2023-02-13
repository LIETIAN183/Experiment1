using System;
using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Loop;
using InitialPrefabs.NimGui.Text;
using Unity.Collections;
using UnityEngine;

namespace InitialPrefabs.NimGui.Render
{

    public static class ImGuiRenderUtils
    {

        internal const char Box = '█';
        internal const char Checkmark = '√';
        internal const char X = '✕';
        internal const char Expand = '≡';

        static readonly int _MainTex = Shader.PropertyToID("_MainTex");

        static Material Material;
        static UnsafeArray<ImGlyph> Glyphs;
        static ImFontFace FontFace;

        static int BoxIndex;
        static int CheckIndex;
        static int XIndex;
        static int HamburgerMenuIndex;

        internal static ResultFlag CheckDependency()
        {
            var shader = Shader.Find("InitialPrefabs/SDF");
            return shader != null ? ResultFlag.Success : ResultFlag.MissingShader;
        }

        public static void Initialize(Texture2D texture, SerializedFontData fontData)
        {
            if (Glyphs.IsCreated())
            {
                return;
                Glyphs.Dispose();
                Debug.LogWarning("Releasing allocated Glyphs for reinitializtion");
            }

            // TODO: Load the shader from resources...
            var shader = Shader.Find("InitialPrefabs/SDF");
            Material = new Material(shader);
            Material.SetTexture(_MainTex, texture);

            FontFace = fontData.FontFaceInfo;

            Glyphs = new UnsafeArray<ImGlyph>(fontData.Glyphs.Length, Allocator.Persistent);
            // Copy
            for (int i = 0; i < fontData.Glyphs.Length; ++i)
            {
                Glyphs[i] = fontData.Glyphs[i];
            }

            BoxIndex = FindGlyphIndex(Box);
            CheckIndex = FindGlyphIndex(Checkmark);
            XIndex = FindGlyphIndex(X);
            HamburgerMenuIndex = FindGlyphIndex(Expand);
        }

        public static void Release()
        {
            if (Glyphs.IsCreated())
            {
                Glyphs.Dispose();
            }
        }

        /// <summary>
        /// The primary material used for rendering the entire UI.
        /// </summary>
        /// <returns>A reference to the material.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Material GetMaterial()
        {
            return Material;
        }

        /// <summary>
        /// The primary font face used for rendering the entire UI.
        /// </summary>
        /// <returns>A reference to the FontFace.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ImFontFace GetFontFace()
        {
            return ref FontFace;
        }

        /// <summary>
        /// The primary glyphs associated with the FontFace. All glyphs 
        /// are sorted with their unicode values.
        /// </summary>
        /// <returns>The sorted glyphs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeArray<ImGlyph> GetGlyphs()
        {
            return Glyphs;
        }

        /// <summary>
        /// Queues a draw command to draw a solid colored box.
        /// </summary>
        /// <param name="window">The window to enqueue the draw command to.</param>
        /// <param name="rect">The size of the box.</param>
        /// <param name="color">The color of the box.</param>
        /// <param name="cutoff">Optional cutoff, typically this should be set to 0 for no cutoff.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSolidBox(
            this ImWindow window, in ImRect rect, in Color32 color, float cutoff = 0f)
        {

            var uvs = Glyphs[BoxIndex].Uvs;
            var unmanagedCmds = window.UnmanagedImWindow.DrawBuffer.Peek();
            unmanagedCmds.Push(ImDrawCommandType.Image, rect, color, cutoff);
            unmanagedCmds.Push(new ImSpriteData { InnerUV = uvs });
        }

        /// <summary>
        /// Queues a draw command to draw a checkmark.
        /// </summary>
        /// <param name="window">The window to enqueue the draw command to.</param>
        /// <param name="rect">The size of the checkmark.</param>
        /// <param name="color">The color of the checkmark.</param>
        /// <param name="cutoff">Optional cutoff, typically this should be set to 0.5 for minimal cutoff.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCheckmark(
            this ImWindow window, in ImRect rect, in Color32 color, float cutoff = 0.5f)
        {

            var uvs = Glyphs[CheckIndex].Uvs;
            var unmanagedCmds = window.UnmanagedImWindow.DrawBuffer.Peek();
            unmanagedCmds.Push(ImDrawCommandType.Image, rect, color, cutoff);
            unmanagedCmds.Push(new ImSpriteData { InnerUV = uvs });
        }

        /// <summary>
        /// Queues a draw command to draw an X.
        /// </summary>
        /// <param name="window">The window to enqueue to draw command to.</param>
        /// <param name="rect">The size of the x.</param>
        /// <param name="color">The color of the x.</param>
        /// <param name="cutoff">Optional cutoff, typically this should be set to 0.5 for minimal cutoff.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushX(
            this ImWindow window, in ImRect rect, in Color32 color, float cutoff = 0.5f)
        {

            var uvs = Glyphs[XIndex].Uvs;
            var unmanagedCmds = window.UnmanagedImWindow.DrawBuffer.Peek();
            unmanagedCmds.Push(ImDrawCommandType.Image, rect, color, cutoff);
            unmanagedCmds.Push(new ImSpriteData { InnerUV = uvs });
        }

        /// <summary>
        /// Queues a draw command to draw an X.
        /// </summary>
        /// <param name="window">The window to enqueue to draw command to.</param>
        /// <param name="rect">The size of the x.</param>
        /// <param name="color">The color of the x.</param>
        /// <param name="cutoff">Optional cutoff, typically this should be set to 0.5 for minimal cutoff.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushHamburgerMenu(
            this ImWindow window, in ImRect rect, in Color32 color, float cutoff = 0.5f)
        {

            var uvs = Glyphs[HamburgerMenuIndex].Uvs;
            var unmanagedCmds = window.UnmanagedImWindow.DrawBuffer.Peek();
            unmanagedCmds.Push(ImDrawCommandType.Image, rect, color, cutoff);
            unmanagedCmds.Push(new ImSpriteData { InnerUV = uvs });
        }

        static int FindGlyphIndex(char c)
        {
#if UNITY_EDITOR
            if (!Glyphs.IsCreated())
            {
                Debug.LogError("Glyphs has not been initialized!");
                return 0;
            }
#endif
            var idx = Glyphs.BinarySearch(c, default(GlyphComparer));
            if (idx < 0)
            {
                Debug.LogError($"Cannot find Glyph for character: {c}!");
                return 0;
            }

            return idx;
        }
    }
}
