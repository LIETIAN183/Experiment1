using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using static InitialPrefabs.NimGui.Text.TextUtils;

namespace InitialPrefabs.NimGui.Render {

    [BurstCompile]
    internal struct ResizeBufferCapacityJob : IJob {

        public int DrawCount;

        [ReadOnly]
        public NativeReference<int> Size;

        public NativeList<ImVertex> Vertices;
        public NativeList<uint> Indices;

        public void Execute() {
            if (Vertices.Capacity < Size.Value) {
                Vertices.Capacity = Size.Value;
            }

            var indexSize = Size.Value * 3;
            if (Indices.Capacity < indexSize) {
                Indices.Capacity = indexSize;
            }
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, DisableSafetyChecks = true)]
    internal unsafe struct CalculateBufferSizeJob : IJob {

        public ReadOnlyQueue<ImDrawData> DrawData;

        public ReadOnlyQueue<TextUnit> TextData;

        [WriteOnly]
        public NativeReference<int> Size;

        public void Execute() {
            var size = 0;
            for (int i = 0; i < DrawData.Count; ++i) {
                var element = DrawData.Dequeue();

                switch (element.Type) {
                    case ImDrawCommandType.Image:
                        // TODO: Support rounded corners, which will have more vertices
                        size += 4;
                        break;
                    case ImDrawCommandType.Text:
                        ref var content = ref TextData.Dequeue();

                        // Each character will have four vertices
                        size += content.ImString.Length * 4;
                        break;
                }
            }
            Size.Value = size;
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, DisableSafetyChecks = true)]
    internal struct CalculateTextSizeJobNonAlloc : IJob {

        public ImString Text;
        public ImFontFace FontFace;
        public UnsafeArray<ImGlyph> Glyphs;

        public float Width;
        public int FontSize;

        public NativeReference<int> MaxLines;

        [WriteOnly]
        public NativeReference<float2> FinalSize;

        public void Execute() {
            var scale   = FontSize / FontFace.PointSize;

            var comparer   = new GlyphComparer();
            var debugWidth = 0f;
            var maxWidth   = 0f;
            var lineNum    = 0;

            for (int i = 0; i < Text.Length; ++i) {
                var glyphIdx = Glyphs.BinarySearch(new ImGlyph { Unicode = Text[i] }, comparer);
                var glyph    = Glyphs[glyphIdx];
                var advance  = (glyph.Advance - glyph.Bearings.x) * scale;
                var next     = debugWidth + advance;

                if (next < Width) {
                    debugWidth = next;
                } else {
                    maxWidth = debugWidth > maxWidth ? debugWidth : maxWidth;
                    debugWidth = advance;
                    lineNum++;
                }
            }

            if (debugWidth > 0) {
                lineNum++;
                maxWidth = debugWidth > maxWidth ? debugWidth : maxWidth;
            }

            FinalSize.Value = math.ceil(new float2(maxWidth, lineNum * (FontFace.LineHeight * scale)));

            if (lineNum > MaxLines.Value ) {
                MaxLines.Value = lineNum;
            }
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, DisableSafetyChecks = true)]
    internal struct BuildWindowMeshJob : IJob {

        public ImRect ScreenRect;

        public ImFontFace FontFaceInfo;

        // ---------------------------------------------------
        // Read Access
        // ---------------------------------------------------
        public ReadOnlyQueue<TextUnit> TextContent;

        [ReadOnly]
        public UnsafeArray<ImGlyph> Glyphs;

        [ReadOnly]
        public UnsafeList<ImDrawData> DrawVector;

        [ReadOnly]
        public ReadOnlyQueue<ImSpriteData> SpriteQueue;

        // ---------------------------------------------------
        // Read Write Access
        // ---------------------------------------------------
        public NativeList<LineInfo> Lines;
        public NativeList<uint> Indices;
        public NativeList<ImVertex> Vertices;

        public void Execute() {
            ImRect scissor = ScreenRect;
            NativeArray<ImVertex> tempVertices = new NativeArray<ImVertex>(4, Allocator.Temp);
            for (int i = 0; i < DrawVector.Length; ++i) {
                ref var element = ref DrawVector.ElementAt(i);

                switch (element.Type) {
                    case ImDrawCommandType.Image:
                        ref var spriteData = ref SpriteQueue.Dequeue();
                        CreateImage(
                            in element,
                            in scissor,
                            in spriteData,
                            ref tempVertices,
                            ref Vertices,
                            ref Indices);
                        break;
                    case ImDrawCommandType.Text:
                        ref var content = ref TextContent.Dequeue();

                        CreateTextData(
                            in element,
                            in scissor,
                            in content,
                            in FontFaceInfo,
                            in Glyphs,
                            ref tempVertices,
                            ref Lines,
                            ref Vertices,
                            ref Indices);
                        Lines.Clear();
                        break;
                    case ImDrawCommandType.Scissor:
                        scissor = element.Arguments == 1 ? element.Rect : ScreenRect;
                        break;
                }
            }
            tempVertices.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void CreateTextData(
            in ImDrawData drawData,
            in ImRect scissor,
            in TextUnit content,
            in ImFontFace FontFaceInfo,
            in UnsafeArray<ImGlyph> glyphs,
            ref NativeArray<ImVertex> tempVertices,
            ref NativeList<LineInfo> lines,
            ref NativeList<ImVertex> vertices,
            ref NativeList<uint> indices) {

            // Dequeue an element in the TextContent
            var fontSize = content.PointSize;
            var @string  = content.ImString;

            // Font style
            // TODO: Support options for the text
            var fontScale = fontSize / FontFaceInfo.PointSize;
            var adjustedSize = drawData.Rect.Size - DefaultStyles.Padding;

            TextUtils.CountLines(
                in @string,
                in glyphs,
                in adjustedSize,
                in fontScale,
                ref lines);

            var heightInfo = new HeightInfo(lines.Length, fontScale, in FontFaceInfo);
            TextUtils.DecodeAlignments(in drawData.Arguments, out var horizontal, out var vertical);

            var comparer = default(GlyphComparer);

            var linesRO = lines.AsArray();
            for (int lineNum = 0; lineNum < linesRO.Length; lineNum++) {
                ref var line = ref linesRO.ElementAt(lineNum);

                // The start position is determined by the position in which we want to render
                // and shifted to the bottom left.
                var startPos = new float2(
                    TextUtils.AlignHorizontally(
                        in line.LineWidth,
                        in adjustedSize.x,
                        in drawData.Rect,
                        in horizontal),
                    TextUtils.AlignVertically(
                        in heightInfo,
                        in drawData.Rect,
                        in vertical) - lineNum * heightInfo.LineHeight);

                for (int i = 0; i < line.Length; ++i) {
                    ref char unicode = ref @string[i + line.StartOffset];

                    var idx   = glyphs.BinarySearch(new ImGlyph { Unicode = (uint)unicode }, comparer);
                    var glyph = glyphs[idx];

                    var metrics = glyph.MetricsSize * fontScale;
                    var bearings = glyph.Bearings * fontScale;
                    var localHeight = metrics.y - bearings.y;

                    // TODO: Optimize this here
                    var extremities = new float4(
                        startPos.x + bearings.x,
                        startPos.y - localHeight,
                        startPos.x + bearings.x + metrics.x,
                        startPos.y - localHeight + metrics.y);

                    // Clamp those coordinates
                    var all = scissor.Clamp(in extremities);
                    var uv0 = MeshUtils.Scissor(in extremities, in all, in glyph.Uvs);

                    var color = drawData.Color.ToFloat4();

                    // Grab the # of vertices prior to adding more
                    var indexOffset = (uint)vertices.Length;

                    tempVertices[0] = new ImVertex {
                        Color    = color,
                        Position = new float3(all.xy, 0),
                        UV0      = uv0.xy,
                        UV1      = drawData.Cutoff
                    };
                    tempVertices[1] = new ImVertex {
                        Color    = color,
                        Position = new float3(all.xw, 0),
                        UV0      = uv0.xw,
                        UV1      = drawData.Cutoff
                    };
                    tempVertices[2] = new ImVertex {
                        Color    = color,
                        Position = new float3(all.zw, 0),
                        UV0      = uv0.zw,
                        UV1      = drawData.Cutoff
                    };
                    tempVertices[3] = new ImVertex {
                        Color    = color,
                        Position = new float3(all.zy, 0),
                        UV0      = uv0.zy,
                        UV1      = drawData.Cutoff
                    };
                    vertices.AddRangeNoResize(tempVertices.GetUnsafePtr(), 4);

                    var idxTemp = new uint4(indexOffset, new uint3(indexOffset) + new uint3(1, 2, 3));
                    var tri = new uint3x2(idxTemp.xyz, idxTemp.xzw);
                    indices.AddRangeNoResize(&tri, 6);

                    startPos.x += (glyph.Advance - glyph.Bearings.x) * fontScale;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void CreateImage(
            in ImDrawData drawData,
            in ImRect scissor,
            in ImSpriteData spriteData,
            ref NativeArray<ImVertex> tempVertices,
            ref NativeList<ImVertex> vertices,
            ref NativeList<uint> indices) {

            var indexOffset = (uint)vertices.Length;
            var minmax = new float4(drawData.Rect.Min, drawData.Rect.Max);
            var all = scissor.Clamp(in minmax);

            var innerUV = MeshUtils.Scissor(in minmax, in all, in spriteData.InnerUV);

            tempVertices[0] = new ImVertex {
                Color    = drawData.Color.ToFloat4(),
                Position = new float3(all.xy, 0),
                UV0      = innerUV.xy,
                UV1      = drawData.Cutoff
            };
            tempVertices[1] = new ImVertex {
                Color    = drawData.Color.ToFloat4(),
                Position = new float3(all.xw, 0),
                UV0      = innerUV.xw,
                UV1      = drawData.Cutoff
            };
            tempVertices[2] = new ImVertex {
                Color    = drawData.Color.ToFloat4(),
                Position = new float3(all.zw, 0),
                UV0      = innerUV.zw,
                UV1      = drawData.Cutoff
            };
            tempVertices[3] = new ImVertex {
                Color    = drawData.Color.ToFloat4(),
                Position = new float3(all.zy, 0),
                UV0      = innerUV.zy,
                UV1      = drawData.Cutoff
            };
            vertices.AddRangeNoResize(tempVertices.GetUnsafePtr(), 4);

            var tempIndex = new uint4(indexOffset, new uint3(indexOffset) + new uint3(1, 2, 3));
            var tri = new uint3x2(tempIndex.xyz, tempIndex.xzw);
            indices.AddRangeNoResize(&tri, 6);
        }
    }
}
