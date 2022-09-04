using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Render;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui.Text {

    public enum HorizontalAlignment : byte {
        Center = 0,
        Left = 1,
        Right = 2
    }

    public enum VerticalAlignment : byte {
        Center = 0,
        Top = 1,
        Bottom = 2
    }

    public ref struct HeightInfo {
        public readonly float AscentLine;
        public readonly float DescentLine;
        public readonly float TextBlockHeight;
        public readonly float LineHeight;

        public HeightInfo(int lineCount, float fontScale, in ImFontFace faceInfo) {
            AscentLine = faceInfo.AscentLine * fontScale;
            DescentLine = faceInfo.DescentLine * fontScale;
            LineHeight = faceInfo.LineHeight * fontScale;
            TextBlockHeight = LineHeight * lineCount;
        }
    }

    public static class TextUtils {

        // TODO: Probably move this.
        internal static int MaxLines;

        /// <summary>
        /// Stores information about the current width of the line, the character, 
        /// and the # words the line contains.
        /// </summary>
        ref struct LineDebug {

            public float LineWidth;

            // Character info
            public int StartOffset;

            public int CharIndex;
        }

        public struct LineInfo {
            public float LineWidth;
            public int StartOffset;
            public int Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountLines(
            in ImString text,
            in UnsafeArray<ImGlyph> glyphs,
            in float2 dimensions,
            in float scale,
            ref NativeList<LineInfo> lines) {

            var debug = new LineDebug { };
            var comparer = default(GlyphComparer);

            for (int i = 0; i < text.Length; ++i) {
                char c = text[i];

                int glyphIdx = glyphs.BinarySearch(new ImGlyph { Unicode = c }, comparer);
                ImGlyph glyph = glyphs[glyphIdx];
                float advance = (glyph.Advance - glyph.Bearings.x) * scale;
                float next = debug.LineWidth + advance;

                if (next < dimensions.x) {
                    debug.LineWidth = next;
                    debug.CharIndex++;
                } else {
                    lines.AddNoResize(new LineInfo {
                        LineWidth = debug.LineWidth,
                        Length = debug.CharIndex - debug.StartOffset,
                        StartOffset = debug.StartOffset
                    });

                    debug.StartOffset = debug.CharIndex;
                    debug.LineWidth = advance;
                    debug.CharIndex++;
                }
            }

            if (debug.LineWidth > 0) {
                lines.AddNoResize(new LineInfo {
                    LineWidth = debug.LineWidth,
                    Length = debug.CharIndex - debug.StartOffset,
                    StartOffset = debug.StartOffset
                });
            }
        }

        // TODO: The horizontal alignment needs to take in the max width and not just the regular width
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AlignHorizontally(
            in float lineWidth,
            in float width,
            in ImRect rect,
            in HorizontalAlignment alignment) {

            switch (alignment) {
                case HorizontalAlignment.Left:
                    return rect.Position.x - rect.Extents.x;
                case HorizontalAlignment.Right:
                    return rect.Extents.x - width + rect.Position.x;
                default:
                    return rect.Position.x - lineWidth * 0.5f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AlignVertically(
            in HeightInfo heightInfo,
            in ImRect rect,
            in VerticalAlignment alignment) {

            switch (alignment) {
                case VerticalAlignment.Top:
                    return rect.Max.y - heightInfo.AscentLine;
                case VerticalAlignment.Bottom:
                    return rect.Min.y - heightInfo.DescentLine + heightInfo.TextBlockHeight - heightInfo.LineHeight;
                default:
                    return (rect.Min.y + rect.Max.y) * 0.5f - heightInfo.AscentLine + heightInfo.TextBlockHeight * 0.5f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetStringHash(in ImString content) {
            unsafe {
                return math.hash(content.Ptr, sizeof(char) * content.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetStringHash(string content) {
            unsafe {
                fixed (char* ptr = content) {
                    return math.hash(ptr, content.Length * sizeof(char));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static float2 CalculateSize(
            in ImString text,
            in ImFontFace fontFace,
            in UnsafeArray<ImGlyph> glyphs,
            in float width,
            in int fontSize) {

            var finalSize = new NativeReference<float2>(Allocator.TempJob);
            var maxLines = new NativeReference<int>(5, Allocator.TempJob);

            new CalculateTextSizeJobNonAlloc {
                Text = text,
                FontFace = fontFace,
                Glyphs = glyphs,
                Width = width,
                FontSize = fontSize,
                FinalSize = finalSize,
                MaxLines = maxLines,
            }.Run();

            MaxLines = maxLines.Value;

            float2 retVal = finalSize.Value;
            finalSize.Dispose();
            maxLines.Dispose();
            return retVal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountDigits(int i) {
            if (i == 0) {
                return 1;
            }

            var digits = i < 0 ? 1 : 0;
            int num = math.abs(i);

            while (num > 0) {
                num /= 10;
                digits++;
            }
            return digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountDigits(float v, int decimalPlaces) {
            float absValue = math.abs(v);
            int base10 = (int)absValue;

            int count = CountDigits(base10) + math.select(1, 0, v >= 0f);
            float fraction = absValue - base10;

            if (fraction * math.pow(10, decimalPlaces) > 0) {
                count += decimalPlaces + 1;
            }
            return count;
        }

        // TODO: Figure out inplace sorting to avoid stack allocations, or fuck it and keep it
        internal static unsafe ImString ToImString(this int num, ref ImWords words, int extension = 0) {
            var offset = num < 0 ? 0 : -1;
            int length = CountDigits(num) + extension;
            ImString word = words.Request(length);
            num = math.abs(num);

            var stack = new NativeList<char>(length, Allocator.Temp);
            do {
                int mod = num % 10;
                num /= 10;

                stack.Add((char)(mod + 48));
            } while (num != 0);


            for (int i = 0; i < stack.Length; ++i) {
                int flipped = stack.Length + offset - i;
                if (flipped < length) {
                    *(word.Ptr + flipped) = stack[i];
                }
            }

            if (offset >= 0) {
                word.Ptr[0] = '-';
            }

            stack.Dispose();
            return word;
        }

        internal static unsafe ImString ToImString(this float num, ref ImWords words, int decimalPlaces = 2) {
            int base10 = math.abs((int)num);
            var base10Digits = CountDigits(base10);
            var exponent = base10Digits - 1;
            var fraction = math.abs(num) - base10;

            var totalDigits = base10Digits + decimalPlaces + math.select(0, 1, num < 0) + 1;
            var collection = new NativeList<char>(totalDigits, Allocator.Temp);

            if (num < 0) {
                collection.Add('-');
            }

            while (exponent > -1) {
                var powerRaised = (int)math.pow(10, exponent);
                var digit = base10 / powerRaised;
                base10 %= powerRaised;

                collection.Add((char)(digit + 48));
                exponent--;
            }

            collection.Add('.');

            for (int i = 0; i < decimalPlaces; ++i) {
                fraction = fraction * 10 % 10;
                var digit = (int)fraction;
                collection.Add((char)(digit + 48));
            }

            // ---------------------------------------------------------
            // Copy the content from our temp collection to the ImWord.
            // ---------------------------------------------------------
            var word = words.Request(totalDigits);
            UnsafeUtility.MemCpy(word.Ptr, collection.GetUnsafePtr(), sizeof(char) * totalDigits);
            collection.Dispose(); // Turns to a no op
            return word;
        }

        internal static unsafe ImString ToImString(this in char c, ref ImWords words) {
            var word = words.Request(1);
            *word.Ptr = c;
            return word;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static sbyte EncodeAlignments(in HorizontalAlignment horizontal, in VerticalAlignment vertical) {
            sbyte encoded = (sbyte)horizontal;
            return encoded |= (sbyte)(((byte)vertical) << 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DecodeAlignments(in sbyte value, out HorizontalAlignment horizontal, out VerticalAlignment vertical) {
            const byte xMask = 0b0011;
            const byte yMask = 0b1100;

            horizontal = (HorizontalAlignment)(value & xMask);
            vertical = (VerticalAlignment)((value & yMask) >> 2);
        }
    }
}
