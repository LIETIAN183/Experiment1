using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Profiling;

namespace InitialPrefabs.NimGui {

    internal static class ImLayoutUtility {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImRect CreateRect(in ImScope scope, in float2 size, in float2 offset) {
            var flip = new float2(1, -1);
            var extents = size / 2;

            var padding = DefaultStyles.Padding * flip;
            var position = scope.Next + extents * flip + padding + offset;
            return new ImRect(position, extents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void UpdateScope(ref ImScope scope, in float2 size) {
            var marker = new ProfilerMarker("UpdateScope");
            marker.Begin();
            scope.Previous = scope.Next;
            var left       = scope.Rect.Position.x - scope.Rect.Extents.x;
            scope.Next     = new float2(left, -size.y + scope.Next.y + -DefaultStyles.Padding.y);
            scope.Delta    = size;
            marker.End();
        }
    }
}
