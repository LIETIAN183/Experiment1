using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui.Common {
    
    /// <summary>
    /// This static class is currently not actively used and is considered experimental.
    /// Once verified to work with Burst this will move out of experimental stage.
    /// </summary>
    internal static class ImMath {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float lhs, float rhs, float v) {
            return math.distancesq(v, lhs) / math.distancesq(lhs, rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float2 lhs, float2 rhs, float2 v) {
            return math.distancesq(v, lhs) / math.distancesq(lhs, rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseSqrt(float x) {
            unsafe {
                float xhalf = 0.5f * x;
                int i = *(int*)&xhalf;
                i = 0x5f375a86 - (i >> 1);
                x = *(float*)&i;
                return x * (1.5f - xhalf * x * x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float n) {
            unsafe {
                int i = 0x5F375A86 - (*(int*)&n >> 1);
                float f = *(float*)&i;
                return (3 - n * f * f) * n * f * 0.5f;
            }
        }
    }
}
