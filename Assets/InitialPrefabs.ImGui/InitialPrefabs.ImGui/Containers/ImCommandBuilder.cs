using System;
using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using Unity.Collections;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui {

    internal unsafe struct ImDrawBuilder : IDisposable {

        public UnsafeArray<Commands> Commands;

        int index;

        public ImDrawBuilder(int bufferCount, int commandCapacity) {
            Commands = new UnsafeArray<Commands>(bufferCount, Allocator.Persistent);
            index = 0;

            for (int i = 0; i < Commands.Length; ++i) {
                Commands[i] = new Commands(commandCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Commands Peek() {
            return Commands[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next() {
            index = math.clamp(++index, 0, Commands.Length - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Previous() {
            index = math.clamp(--index, 0, Commands.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Commands Root() {
            return Commands.ElementAt(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Length() {
            return Commands.Length;
        }

        public unsafe void Clear() {
            index = 0;

            for (int i = 0; i < Commands.Length; ++i) {
                ref Commands cmd = ref Commands.ElementAt(i);
                cmd.Reset();
            }
        }

        public void Dispose() {
            for (int i = 0; i < Commands.Length; ++i) {
                ref Commands cmd = ref Commands.ElementAt(i);
                cmd.Dispose();
            }

            Commands.Dispose();
        }

        public void Consolidate() {
            ref Commands root = ref Commands.ElementAt(0);

            for (int i = 1; i < Commands.Length; ++i) {
                ref Commands ctx = ref Commands.ElementAt(i);
                root.DrawCommands->AddRange(*ctx.DrawCommands);
                root.SpriteCommands->AddRange(*ctx.SpriteCommands);
                root.TextCommands->AddRange(*ctx.TextCommands);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Index() {
            return index;
        }
    }
}
