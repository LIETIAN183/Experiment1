using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// ImWindow stores rendering information that can't be used in jobs.
    /// </summary>
    public class ImWindow : IDisposable {

        const int MaxChars = 8192;

        // ----------------------------------------------------------
        // Rendering information
        // ----------------------------------------------------------
        internal Mesh Mesh;
        internal NativeList<ImVertex> Vertices;
        internal NativeList<uint> Indices;
        internal NativeReference<int> BufferSize;

        // ----------------------------------------------------------
        // Unmanaged Memory
        // ----------------------------------------------------------
        internal UnmanagedImWindow UnmanagedImWindow;
        internal ImWords Words; // TODO: Allow more storage if we exceed the text buffer

        public ImWindow() {
            const int capacity = 25;
            Mesh = new Mesh() { name = "Primary_Window" };

            var size = new int2(Screen.width, Screen.height);
            var position = new float2(Screen.width / 2, Screen.height / 2);

            UnmanagedImWindow = new UnmanagedImWindow(capacity, size, position);
            Words = new ImWords(MaxChars);

            InitMeshData();
        }

        public ImWindow(int capacity, int2 size, float2 position, string name) {
            Mesh = new Mesh() { name = name };
            UnmanagedImWindow = new UnmanagedImWindow(capacity, size, position);
            Words = new ImWords(MaxChars);
            InitMeshData();
        }

        ~ImWindow() {
            Dispose();
        }

        void InitMeshData() {
            Vertices = new NativeList<ImVertex>(1500, Allocator.Persistent);
            Indices  = new NativeList<uint>(1500, Allocator.Persistent);
            BufferSize     = new NativeReference<int>(Allocator.Persistent);
        }

        public void PushTxt(ImString content, ImRect r, in ImTextStyle style, float cutOff = 0.5f) {
            var unmanagedCmds = UnmanagedImWindow.DrawBuffer.Peek();
            unmanagedCmds.Push(content, style.FontSize);
            unmanagedCmds.Push(r, style.TextColor, style.Column, style.Row, cutOff);
        }

        public void Dispose() {
            Words.Dispose();
            Vertices.Dispose();
            Indices.Dispose();
            BufferSize.Dispose();
            UnmanagedImWindow.Dispose();
        }

        internal void ResetContext() {
            UnmanagedImWindow.ResetContext();
            Words.Reset();
        }

        internal void ResetTempMeshData() {
            Vertices.Clear();
            Indices.Clear();
            BufferSize.Value = 0;
        }
    }

    /// <summary>
    /// Stores the position and offset when dragging the pane.
    /// </summary>
    public struct ImPaneOffset {
        public float2 Position;
        public float2 Offset;
    }

    /// <summary>
    /// To support Unity Jobs and Burst, this will store only blittable information.
    /// </summary>
    public unsafe struct UnmanagedImWindow : IDisposable {

        // ---------------------------------------------------
        // Primary Buffers
        // ---------------------------------------------------
        internal ImDrawBuilder DrawBuffer;

        // ---------------------------------------------------
        // Scope
        // ---------------------------------------------------
        // When storing scopes, I need to treat it like a stack. Push a scope and pop a scope. 
        // This allows me to reference the last known scope.
        [NativeDisableUnsafePtrRestriction]
        internal UnsafeList<ImScope>* ImScopeVector;

        // ---------------------------------------------------
        // Behavior States
        // ---------------------------------------------------
        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, bool>* ImCollapsibles;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, bool>* ImClosed;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, bool>* ImToggled;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, ushort>* ImOptions;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, ImPaneOffset>* ImPaneOffsets;

        [NativeDisableUnsafePtrRestriction]
        internal UnsafeParallelHashMap<uint, float2>* ImScrollOffsets;

        // ----------------------------------------
        // States
        // ----------------------------------------
        internal uint ActiveItem;       // The ActiveItem is which element the mouse is interacting with
        internal uint HotItem;          // The HotItem is which element the mouse is over
        internal uint TrackedItem;      // The TrackedItem is the element that needs to be drawn to a backbuffer
        internal uint LastTrackedItem;  // When we release the tracked item, we save it so we know which draws last.
        internal float2 ScrollOffset;   // Store the offset we must add to change the position
        internal uint2 BufferedIds;     // The buffered ids allows us to only store the potential 
                                        // hot items that are typically rendered last, but logically
                                        // come first in the call order. Moving forward, this should be 
                                        // the ideal way to determine whether we do a logic of a widget.
                                        // We process this at the begining of the next frame and reset 
                                        // so we can properly handled command buffered rendering logic.

        public UnmanagedImWindow(int capacity, int2 size, float2 position) {
            ActiveItem       = 0;
            HotItem          = 0;
            TrackedItem      = 0;
            LastTrackedItem  = 0;
            ScrollOffset     = new float2();
            BufferedIds      = new uint2();

            // ---------------------------------------------------
            // Primary Buffers
            // ---------------------------------------------------
            DrawBuffer = new ImDrawBuilder(3, capacity);

            // ---------------------------------------------------
            // Scopes
            // ---------------------------------------------------
            // Initialize the scopes that we will track.
            ImScopeVector = (UnsafeList<ImScope>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<ImScope>>(),
                UnsafeUtility.AlignOf<UnsafeList<ImScope>>(),
                Allocator.Persistent);
            *ImScopeVector = new UnsafeList<ImScope>(capacity, Allocator.Persistent);

            // ---------------------------------------------------
            // Button Behavior States
            // ---------------------------------------------------
            // Initialize the collapsibles we want to track
            ImCollapsibles = (UnsafeParallelHashMap<uint, bool>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, bool>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, bool>>(),
                Allocator.Persistent);

            *ImCollapsibles = new UnsafeParallelHashMap<uint, bool>(capacity, Allocator.Persistent);

            // Initialize any closed element we want to track
            ImClosed = (UnsafeParallelHashMap<uint, bool>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, bool>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, bool>>(),
                Allocator.Persistent);

            *ImClosed = new UnsafeParallelHashMap<uint, bool>(capacity, Allocator.Persistent);

            ImToggled = (UnsafeParallelHashMap<uint, bool>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, bool>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, bool>>(),
                Allocator.Persistent);

            *ImToggled = new UnsafeParallelHashMap<uint, bool>(capacity, Allocator.Persistent);

            ImOptions = (UnsafeParallelHashMap<uint, ushort>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, ushort>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, ushort>>(),
                Allocator.Persistent);

            *ImOptions = new UnsafeParallelHashMap<uint, ushort>(capacity, Allocator.Persistent);

            ImPaneOffsets = (UnsafeParallelHashMap<uint, ImPaneOffset>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, ImPaneOffset>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, ImPaneOffset>>(),
                Allocator.Persistent);

            *ImPaneOffsets = new UnsafeParallelHashMap<uint, ImPaneOffset>(capacity, Allocator.Persistent);

            ImScrollOffsets = (UnsafeParallelHashMap<uint, float2>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeParallelHashMap<uint, float2>>(),
                UnsafeUtility.AlignOf<UnsafeParallelHashMap<uint, float2>>(),
                Allocator.Persistent);

            *ImScrollOffsets = new UnsafeParallelHashMap<uint, float2>(capacity, Allocator.Persistent);
        }

        public void Dispose() {
            // ---------------------------------------------------
            // Primary Buffers
            // ---------------------------------------------------
            DrawBuffer.Dispose();

            // ---------------------------------------------------
            // Scopes
            // ---------------------------------------------------
            if (ImScopeVector != null) {
                ImScopeVector->Dispose();
                UnsafeUtility.Free(ImScopeVector, Allocator.Persistent);
                ImScopeVector = null;
            }

            // ---------------------------------------------------
            // Button Behaviours
            // ---------------------------------------------------
            if (ImCollapsibles != null) {
                ImCollapsibles->Dispose();
                UnsafeUtility.Free(ImCollapsibles, Allocator.Persistent);
                ImCollapsibles = null;
            }

            if (ImClosed != null) {
                ImClosed->Dispose();
                UnsafeUtility.Free(ImClosed, Allocator.Persistent);
                ImClosed = null;
            }

            if (ImToggled != null) {
                ImToggled->Dispose();
                UnsafeUtility.Free(ImToggled, Allocator.Persistent);
                ImToggled = null;
            }

            if (ImOptions != null) {
                ImOptions->Dispose();
                UnsafeUtility.Free(ImOptions, Allocator.Persistent);
                ImOptions = null;
            }

            if (ImPaneOffsets != null) {
                ImPaneOffsets->Dispose();
                UnsafeUtility.Free(ImPaneOffsets, Allocator.Persistent);
                ImPaneOffsets = null;
            }

            if (ImScrollOffsets != null) {
                ImScrollOffsets->Dispose();
                UnsafeUtility.Free(ImScrollOffsets, Allocator.Persistent);
                ImScrollOffsets = null;
            }
        }

        internal void ResetContext() {
            // Reset the primary vectors
            DrawBuffer.Clear();

            var size = new float2(Screen.width, Screen.height);
            var position = size / 2f;

            var root = new ImScope {
                Rect = new ImRect(position, size / 2),
                Next = new float2(position + size / 2 * new float2(-1, 1))
            };

            // Clear the scope vector so we don't reallocate
            ImScopeVector->Clear();

            // Readd the root scope
            ImScopeVector->Add(root);

            // Update the active item
            ActiveItem = HotItem;

            // Reset the hot item
            HotItem = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResetActiveItem() {
            ActiveItem = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImScope LastScope() {
            return LastScopeRef();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref ImScope LastScopeRef() {
            return ref UnsafeUtility.AsRef<ImScope>(ImScopeVector->LastPtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TryUpdateBufferedIds(uint id) {
            uint drawIndex = (uint)DrawBuffer.Index();
            if (drawIndex > 0 && drawIndex >= BufferedIds.y) {
                BufferedIds = new uint2(id, drawIndex);
            }
        }
    }
}
