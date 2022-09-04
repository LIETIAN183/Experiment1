using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    internal struct TextUnit {
        public ImString ImString;
        public int PointSize;

        public TextUnit(ImString content, int pointSize) {
            ImString = content;
            PointSize = pointSize;
        }
    }

    internal unsafe struct Commands : IDisposable {

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeList<ImDrawData>* DrawCommands;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeList<ImSpriteData>* SpriteCommands;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeList<TextUnit>* TextCommands;

        public Commands(int capacity) {
            DrawCommands = (UnsafeList<ImDrawData>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<ImDrawData>>(),
                UnsafeUtility.AlignOf<UnsafeList<ImDrawData>>(),
                Allocator.Persistent);

            *DrawCommands = new UnsafeList<ImDrawData>(capacity, Allocator.Persistent);

            SpriteCommands = (UnsafeList<ImSpriteData>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<ImSpriteData>>(),
                UnsafeUtility.AlignOf<UnsafeList<ImSpriteData>>(),
                Allocator.Persistent);

            *SpriteCommands = new UnsafeList<ImSpriteData>(capacity, Allocator.Persistent);

            TextCommands = (UnsafeList<TextUnit>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<TextUnit>>(),
                UnsafeUtility.AlignOf<UnsafeList<TextUnit>>(),
                Allocator.Persistent);

            *TextCommands = new UnsafeList<TextUnit>(capacity, Allocator.Persistent);
        }

        public void Dispose() {
            if (DrawCommands != null) {
                DrawCommands->Dispose();
                UnsafeUtility.Free(DrawCommands, Allocator.Persistent);
                DrawCommands = null;
            }

            if (SpriteCommands != null) {
                SpriteCommands->Dispose();
                UnsafeUtility.Free(SpriteCommands, Allocator.Persistent);
                SpriteCommands = null;
            }

            if (TextCommands != null) {
                TextCommands->Dispose();
                UnsafeUtility.Free(TextCommands, Allocator.Persistent);
                TextCommands = null;
            }
        }

        public void PopScissor() {
            DrawCommands->Add(new ImDrawData {
                Type      = ImDrawCommandType.Scissor,
                Arguments = 0
            });
        }

        public void Push(in ImDrawCommandType type, in ImRect rect, in Color32 color, in float cutOff) {
            DrawCommands->Add(new ImDrawData {
                Color     = color,
                Rect      = rect,
                Type      = type,
                Arguments = -1,
                Cutoff    = cutOff
            });
        }

        public void Push(in ImSpriteData data) {
            SpriteCommands->Add(data);
        }

        public void Push(in ImString s, in int pointSize) {
            TextCommands->Add(new TextUnit(s, pointSize));
        }

        public void Push(in ImRect rect, in Color32 color, in HorizontalAlignment column, in VerticalAlignment row, float cutoff) {
            DrawCommands->Add(new ImDrawData {
                Color     = color,
                Rect      = rect,
                Type      = ImDrawCommandType.Text,
                Arguments = TextUtils.EncodeAlignments(in column, in row),
                Cutoff    = cutoff
            });
        }

        public void PushScissor(in ImRect rect) {
            DrawCommands->Add(new ImDrawData {
                Type      = ImDrawCommandType.Scissor,
                Arguments = 1,
                Rect      = rect
            });
        }

        public void Reset() {
            DrawCommands->Clear();
            SpriteCommands->Clear();
            TextCommands->Clear();
        }
    }
}
