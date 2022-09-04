using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.NimGui.Configs {

    [CustomEditor(typeof(StyleConfigs))]
    internal class StyleConfigsEditor : UnityEditor.Editor {

        StyleConfigs configs;
        StringBuilder builder;

        void OnEnable() {
            builder = new StringBuilder(2048);
            configs = serializedObject.targetObject as StyleConfigs;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUILayout.Button("Save to File")) {
                PopulateConfigs();
                CreateFile();
            }
        }

        void PopulateConfigs() {
            var colors = configs.Colors;
            builder.Clear();

            builder.Append("// ---------------------------------------\n");
            builder.Append("// NOTE: GENERATED CODE, DO NOT MODIFY!!!\n");
            builder.Append("// ---------------------------------------\n");
            builder.Append("using UnityEngine;\n");
            builder.Append("using Unity.Mathematics;\n\n");
            builder.Append("namespace InitialPrefabs.NimGui {\n\n");
            builder.Append("    public static class DefaultStyles {\n\n");

            foreach (var color in colors) {
                var colorValue = color.Value;
                var v = ConvertColorValue(colorValue);
                builder.Append($"        public static readonly Color32 {color.VariableName} = " + 
                    $"new Color32({colorValue.r}, {colorValue.g}, {colorValue.b}, {colorValue.a});\n\n");
            }

            foreach (var @float in configs.Floats) {
                builder.Append($"        public const float {@float.VariableName} = {@float.Value}f;\n\n");
            }

            foreach (var @float2 in configs.Float2s) {
                builder.Append($"        public static readonly float2 {@float2.VariableName} = " + 
                    $"new float2({@float2.Value.x}f, {@float2.Value.y}f);\n\n");
            }

            foreach (var @int in configs.Ints) {
                builder.Append($"        public const int {@int.VariableName} = {@int.Value};\n\n");
            }

            builder.Append("    }\n");
            builder.Append("}\n");
        }

        void CreateFile() {
            var path = EditorUtility.SaveFilePanelInProject("Save", "DefaultStyles", "cs", "Click save");
            if (path.Length == 0) {
                return;
            }

            File.WriteAllText(path, builder.ToString());
            AssetDatabase.Refresh();
        }

        int4 ConvertColorValue(Color color) {
            return new int4((int)color.r * 255, (int)color.g * 255, (int)color.b * 255, (int)color.a * 255);
        }
    }
}
