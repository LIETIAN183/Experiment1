using System;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui.Configs {

    [CreateAssetMenu(menuName = "Scriptable Objects/StyleConfigs")]
    internal class StyleConfigs : ScriptableObject {

        [Serializable]
        public struct ColorField {
            public string VariableName;
            public Color32 Value;
        }

        [Serializable]
        public struct FloatField {
            public string VariableName;
            public float Value;
        }

        [Serializable]
        public struct IntField {
            public string VariableName;
            public int Value;
        }

        [Serializable]
        public struct Float2Field {
            public string VariableName;
            public float2 Value;
        }

        public ColorField[] Colors;
        public FloatField[] Floats;
        public Float2Field[] Float2s;
        public IntField[] Ints;
    }
}
