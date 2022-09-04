using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui.Editor {

    /**
     * The following code is based off the Sweep and Update Euclidean Distance Transform by 
     * Copyright (C) 2009 Stefan Gustavson (stefan.gustavson@gmail.com).
     *
     * Link: https://weber.itn.liu.se/~stegu/edtaa/ 
     * File reference: edtaa2func.c
     * 
     * This software is distributed under the permissive "MIT License":
     * 
     * Permission is hereby granted, free of charge, to any person obtaining a copy
     * of this software and associated documentation files (the "Software"), to deal
     * in the Software without restriction, including without limitation the rights
     * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     * copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     * 
     * The above copyright notice and this permission notice shall be included in
     * all copies or substantial portions of the Software.
     * 
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
     * THE SOFTWARE.
     */

    internal class Point : System.IEquatable<Point> {
        public float Alpha;
        public float Distance;
        public float2 Gradient;
        public int2 Delta;

        public bool Equals(Point other) {
            return other.Alpha == Alpha &&
                other.Distance == Distance &&
                other.Gradient.Equals(Gradient) &&
                other.Delta.Equals(Delta);
        }
    }

    internal class SDF {

        Texture2D src;
        Texture2D dst;

        readonly int width;
        readonly int height;

        Point[,] points;

        // TODO: Add a constructor
        public SDF(Texture2D src) {
            this.src = src;
            width = src.width;
            height = src.height;
            dst = new Texture2D(width, height, TextureFormat.ARGB32, false);
            points = new Point[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    points[x, y] = new Point();
                }
            }
        }

        public void CreateSDFTexture(Vector2 distances) {
            float scale;
            var c = Color.white;

            if (distances.x > 0) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        points[x, y].Alpha = 1f - src.GetPixel(x, y).a;
                    }
                }
                ComputeEdgeGradients();
                GenerateDistanceTransform();

                scale = 1f / distances.x;

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        c.a = Mathf.Clamp01(points[x, y].Distance * scale);
                        dst.SetPixel(x, y, c);
                    }
                }
            }

            if (distances.y > 0) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        points[x, y].Alpha = src.GetPixel(x, y).a;
                    }
                }
                ComputeEdgeGradients();
                GenerateDistanceTransform();

                scale = 1f / distances.y;

                if (distances.x > 0f) {
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            c.a = 0.5f + (dst.GetPixel(x, y).a - Mathf.Clamp01(points[x, y].Distance * scale)) * 0.5f;
                            dst.SetPixel(x, y, c);
                        }
                    }
                } else {
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            c.a = Mathf.Clamp01(1f - points[x, y].Distance * scale);
                            dst.SetPixel(x, y, c);
                        }
                    }
                }
            }
        }

        public Texture2D GetFinalTexture() {
            return dst;
        }

        void ComputeEdgeGradients() {
            var srqt2 = math.sqrt(2);

            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    var p = points[x, y];
                    if (p.Alpha > 0 && p.Alpha < 1) {
                        var gradient =
                            - points[x - 1, y - 1].Alpha 
                            - points[x - 1, y + 1].Alpha 
                            + points[x + 1, y - 1].Alpha 
                            + points[x + 1, y + 1].Alpha;

                        p.Gradient = math.normalize(new float2(
                            gradient + (points[x + 1, y].Alpha - points[x - 1, y].Alpha) * srqt2,
                            gradient + (points[x, y + 1].Alpha - points[x, y - 1].Alpha) * srqt2));
                    }
                }
            }
        }

        void GenerateDistanceTransform() {
            Point point;

            // init the distances
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    point = points[x, y];
                    point.Delta = new int2();

                    if (point.Alpha <= 0f) {
                        point.Distance = 1000000f;
                    } else if (point.Alpha < 1f) {
                        point.Distance = ApproximateEdgeDelta(point.Gradient, point.Alpha);
                    } else {
                        point.Distance = 0f;
                    }
                }
            }

            // 8 point signed sequential euclidean distance xform
            // scan up
            for (int y = 1; y < height; y++) {
                point = points[0,  y];
                if (point.Distance > 0f) {
                    UpdateDistance(ref point, 0, y, new int2(0, -1));
                    UpdateDistance(ref point, 0, y, new int2(1, -1));
                }

                for (int x = 1; x < width - 1; x++) {
                    point = points[x, y];
                    if (point.Distance > 0f) {
                        UpdateDistance(ref point, x, y, new int2(-1,  0));
                        UpdateDistance(ref point, x, y, new int2(-1, -1));
                        UpdateDistance(ref point, x, y, new int2( 0, -1));
                        UpdateDistance(ref point, x, y, new int2( 1, -1));
                    }
                }

                point = points[width - 1, y];
                if (point.Distance > 0f) {
                    UpdateDistance(ref point, width - 1, y, new int2(-1,  0));
                    UpdateDistance(ref point, width - 1, y, new int2(-1, -1));
                    UpdateDistance(ref point, width - 1, y, new int2( 0, -1));
                }

                for (int x = width - 2; x >= 0; x--) {
                    point = points[x, y];
                    if (point.Distance > 0f) {
                        UpdateDistance(ref point, x, y, new int2(1, 0));
                    }
                }
            }
            // Scan down
            for (int y = height - 2; y >= 0; y--) {
                point = points[width - 1, y];
                if (point.Distance > 0f) {
                    UpdateDistance(ref point, width - 1, y, new int2( 0, 1));
                    UpdateDistance(ref point, width - 1, y, new int2(-1, 1));
                }

                for (int x = width - 2; x > 0; x--) {
                    point = points[x, y];
                    if (point.Distance > 0f) {
                        UpdateDistance(ref point, x, y, new int2( 1, 0));
                        UpdateDistance(ref point, x, y, new int2( 1, 1));
                        UpdateDistance(ref point, x, y, new int2( 0, 1));
                        UpdateDistance(ref point, x, y, new int2(-1, 1));
                    }
                }

                point = points[0, y];
                if (point.Distance > 0f) {
                    UpdateDistance(ref point, 0, y, new int2(1, 0));
                    UpdateDistance(ref point, 0, y, new int2(1, 1));
                    UpdateDistance(ref point, 0, y, new int2(0, 1));
                }

                for (int x = 1; x < width; x++) {
                    point = points[x, y];
                    if (point.Distance > 0f) {
                        UpdateDistance(ref point, x, y, new int2(-1, 0));
                    }
                }
            }
        }

        void UpdateDistance(ref Point p, int x, int y, int2 offset) {
            var neighbor = points[x + offset.x, y + offset.y];
            var closest = points[x + offset.x - neighbor.Delta.x, y + offset.y - neighbor.Delta.y];

            if (closest.Alpha == 0f || closest.Equals(p)) {
                return;
            }

            var d = neighbor.Delta - offset;
            var distance = math.sqrt(d.x * d.x + d.y * d.y) + ApproximateEdgeDelta(d, closest.Alpha);
            if (distance < p.Distance) {
                p.Distance = distance;
                p.Delta = d;
            }
        }

        float ApproximateEdgeDelta(float2 g, float alpha) {
            if (g.Equals(float2.zero)) {
                return 0.5f - alpha;
            }

            var length = math.sqrt(g.x * g.x + g.y * g.y);
            g = math.abs(g / length);

            if (g.x < g.y) {
                var temp = g.x;
                g.x = g.y;
                g.y = temp;
            }

            var a1 = 0.5f * g.y / g.x;
            if (alpha < a1) {
                return 0.5f * (g.x + g.y) - math.sqrt(2f * g.x * g.y * alpha);
            }

            if (alpha < (1f - a1)) {
                return (0.5f - alpha) * g.x;
            }

            return -0.5f * (g.x + g.y) + math.sqrt(2f * g.x * g.y * (1f - alpha));
        }
    }
}
