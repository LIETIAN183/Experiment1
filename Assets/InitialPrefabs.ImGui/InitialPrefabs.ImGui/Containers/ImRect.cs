using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Stores the center and extents of the box.
    /// </summary>
    public struct ImRect : IEquatable<ImRect> {

        /// <summary>
        /// The size of the rectangle.
        /// </summary>
        /// <value>A multiplication of the Extents by 2</value>
        public float2 Size {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Extents * 2f;
        }

        /// <summary>
        /// The bottom left corner of the rectangle.
        /// </summary>
        /// <value>This is the position subtracted by the extents.</value>
        public float2 Min {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Position - Extents;
        }

        /// <summary>
        /// The top right corner of the rectangle.
        /// </summary>
        /// <value>This is the position added by the extents.</value>
        public float2 Max {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Position + Extents;
        }

        /// <summary>
        /// Typically, the center of the rectangle.
        /// </summary>
        public float2 Position;

        /// <summary>
        /// The extents is the width and height of the rectangle from the center.
        /// </summary>
        public float2 Extents;

        /// <summary>
        /// Creates a rectangle given the center and extents.
        /// </summary>
        /// <param name="center">The position of the rectangle.</param>
        /// <param name="extents">The extents is the width and height of the rectangle from the center.</param>
        public ImRect(float2 center, float2 extents) {
            Position = center;
            Extents = extents;
        }
        
        /// <summary>
        /// Checks if the point is wihtin the rectangle.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True, if inside the rectangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(float2 point) {
            if (point[0] < Position[0] - Extents[0]) {
                return false;
            }
            if (point[0] > Position[0] + Extents[0]) {
                return false;
            }
            if (point[1] < Position[1] - Extents[1]) {
                return false;
            }
            if (point[1] > Position[1] + Extents[1]) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clamps the current bounds of a rectangle to this rectangle's 
        /// Min and Max points.
        /// </summary>
        /// <param name="bounds">A float4 where xy is the min and zw is the max.</param>
        /// <returns>The clamped values in relation to this rectangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float4 Clamp([NoAlias] in float4 bounds) {
            var min = Position - Extents;
            var max = Position + Extents;

            return new float4(
                math.clamp(bounds.xy, min, max),
                math.clamp(bounds.zw, min, max)
            );
        }

        public bool Equals(ImRect other) {
            return Position.Equals(other.Position) && Extents.Equals(other.Extents);
        }

        public override int GetHashCode() {
            return (int)(math.hash(Position) ^ math.hash(Extents));
        }

        public override string ToString() {
            return $"Position: {Position}, Extents: {Extents}, Size: {Size}";
        }

        public static implicit operator Rect(ImRect rect) {
            return new Rect(rect.Position - rect.Extents, rect.Size);
        }
    }
}
