using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui.Render {

    /// <summary>
    /// Stores rendering data such as position, color, texcoord0, texcoord1.
    /// </summary>
    public struct ImVertex {
        public float3 Position;
        public float4 Color;
        public float2 UV0;
        public float2 UV1;
    }

    /// <summary>
    /// Stores universal data for all elements rendered.
    /// </summary>
    public struct ImDrawData {

        /// <summary>
        /// Stores the type of element we intend to render
        /// </summary>
        public ImDrawCommandType Type;

        /// <summary>
        /// Stores the general bounds of the UI element.
        /// </summary>
        public ImRect Rect;

        /// <summary>
        /// Extraneous arguments to help decipher the draw data. This 
        /// is arbitrary data.
        /// </summary>
        public sbyte Arguments;

        /// <summary>
        /// Stores the vertex color of the mesh.
        /// </summary>
        public Color32 Color;

        /// <summary>
        /// Since we are using SDFs, it is important that each element in a "draw call"
        /// determine the cutoff for the SDF.
        ///
        /// For example, with text, you may want a cutoff of 0.5. But for solid blocks, 
        /// you may want a cutoff of 0 so that the rect is full.
        /// </summary>
        public float Cutoff;
    }

    /// <summary>
    /// Convenient struct to store sprite data from Unity.
    /// </summary>
    public struct ImSpriteData {
        /// <summary>
        /// The UVs of the image where xy is the min and zw is the max.
        /// </summary>
        public float4 InnerUV;
    }
}
