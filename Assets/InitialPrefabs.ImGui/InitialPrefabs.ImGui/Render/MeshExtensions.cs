using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Text;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace InitialPrefabs.NimGui.Render {

    internal static class MeshUtils {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 Scissor(in float4 extremities, in float4 all, in float4 uvs) {
            var dimSq = new float2(
                math.distancesq(extremities.x, extremities.z), 
                math.distancesq(extremities.y, extremities.w));

            const float zero = 0.001f;
            var x = math.max(dimSq.x, zero);
            var y = math.max(dimSq.y, zero);

            return new float4(
                math.lerp(uvs.x, uvs.z, math.distancesq(all.x, extremities.x) / x),
                math.lerp(uvs.y, uvs.w, math.distancesq(all.y, extremities.y) / y),
                math.lerp(uvs.z, uvs.x, math.distancesq(all.z, extremities.z) / x),
                math.lerp(uvs.w, uvs.y, math.distancesq(all.w, extremities.w) / y));
        }
    }

    internal unsafe static class MeshExtensions {
        
        internal static readonly VertexAttributeDescriptor[] VertexAttributeDescriptors = new [] {
            new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color,     VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2),
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float4 ToFloat4(this in Color32 color) {
            return new float4(color.r, color.g, color.b, color.a) / 255;
        }

        internal static void Prepare(this ImWindow window) {
            ImDrawBuilder builder = window.UnmanagedImWindow.DrawBuffer;
            for (int i = 0; i < builder.Length(); ++i) {
                builder.Consolidate();
            }

            // Set the buffered ids.
            ref UnmanagedImWindow unmanagedWindow =  ref window.UnmanagedImWindow;
            if (unmanagedWindow.BufferedIds.y > 0) {
                unmanagedWindow.HotItem = unmanagedWindow.BufferedIds.x;
            }
            unmanagedWindow.BufferedIds = new uint2();
        }

#if UNITY_WEBGL
        internal static void RunBuild(this ImWindow window) {
            var unmanagedWindow = window.UnmanagedImWindow;
            var root            = unmanagedWindow.DrawBuffer.Root();
            var drawData        = *root.DrawCommands;

            if (drawData.Length == 0) {
                return;
            }

            var sprites = (*root.SpriteCommands).AsReadOnlyQueue();
            var lines   = new NativeList<TextUtils.LineInfo>(TextUtils.MaxLines, Allocator.TempJob);
            TextUtils.MaxLines = 0;

            new CalculateBufferSizeJob {
                DrawData = root.DrawCommands->AsReadOnlyQueue(),
                TextData = root.TextCommands->AsReadOnlyQueue(),
                Size     = window.BufferSize
            }.Run();

            new ResizeBufferCapacityJob {
                DrawCount       = root.DrawCommands->Length,
                Size            = window.BufferSize,
                Vertices        = window.Vertices,
                Indices         = window.Indices,
            }.Run();

            var dimensions = new float2(Screen.width, Screen.height) * 0.5f;

            new BuildWindowMeshJob {
                ScreenRect   = new ImRect(dimensions, dimensions),
                DrawVector   = drawData,
                Vertices     = window.Vertices,
                Indices      = window.Indices,
                SpriteQueue  = sprites,
                Glyphs       = ImGuiRenderUtils.GetGlyphs(), 
                TextContent  = root.TextCommands->AsReadOnlyQueue(),
                Lines        = lines,
                FontFaceInfo = ImGuiRenderUtils.GetFontFace(),
            }.Run();

            lines.Dispose();
        }
#else

        internal static JobHandle ScheduleBuild(this ImWindow window, JobHandle inputDeps) {
            var unmanagedWindow = window.UnmanagedImWindow;
            var root            = unmanagedWindow.DrawBuffer.Root();
            var drawData        = *root.DrawCommands;

            if (drawData.Length == 0) {
                return default(JobHandle);
            }

            var sprites = (*root.SpriteCommands).AsReadOnlyQueue();
            var lines   = new NativeList<TextUtils.LineInfo>(TextUtils.MaxLines, Allocator.TempJob);
            TextUtils.MaxLines = 0;

            var calculateBufferSizeJob = new CalculateBufferSizeJob {
                DrawData = root.DrawCommands->AsReadOnlyQueue(),
                TextData = root.TextCommands->AsReadOnlyQueue(),
                Size     = window.BufferSize
            }.Schedule(inputDeps);

            var resizeBufferJob = new ResizeBufferCapacityJob {
                DrawCount       = root.DrawCommands->Length,
                Size            = window.BufferSize,
                Vertices        = window.Vertices,
                Indices         = window.Indices,
            }.Schedule(calculateBufferSizeJob);

            var dimensions = new float2(Screen.width, Screen.height) * 0.5f;

            var buildWindowMeshJob = new BuildWindowMeshJob {
                ScreenRect   = new ImRect(dimensions, dimensions),
                DrawVector   = drawData,
                Vertices     = window.Vertices,
                Indices      = window.Indices,
                SpriteQueue  = sprites,
                Glyphs       = ImGuiRenderUtils.GetGlyphs(), 
                TextContent  = root.TextCommands->AsReadOnlyQueue(),
                Lines        = lines,
                FontFaceInfo = ImGuiRenderUtils.GetFontFace(),
            }.Schedule(resizeBufferJob);

            return lines.Dispose(buildWindowMeshJob);
        }
#endif

        internal unsafe static void Draw(
            this ImWindow window, 
            CommandBuffer cmdBuffer, 
            MaterialPropertyBlock block) {

            var marker = new ProfilerMarker("Submit_Command_Buffer");
            marker.Begin();

            var mesh = window.Mesh;
            cmdBuffer.DrawMesh(
                mesh, 
                Matrix4x4.identity,
                ImGuiRenderUtils.GetMaterial(), 
                0, 
                0, 
                block);

            marker.End();
        }

        internal static void GenerateMeshInternal(this ImWindow window) {
            window.Mesh.Clear();
            // Set up the vertex buffer
            window.Mesh.SetVertexBufferParams(window.Vertices.Length, VertexAttributeDescriptors);
            window.Mesh.SetVertexBufferData(window.Vertices.AsArray(), 0, 0, window.Vertices.Length);

            // Set up the index buffer
            window.Mesh.SetIndexBufferParams(window.Indices.Length, IndexFormat.UInt32);
            window.Mesh.SetIndexBufferData(window.Indices.AsArray(), 0, 0, window.Indices.Length);
            window.Mesh.SetSubMesh(0, new SubMeshDescriptor(0, window.Indices.Length, MeshTopology.Triangles));
        }
    }
}
