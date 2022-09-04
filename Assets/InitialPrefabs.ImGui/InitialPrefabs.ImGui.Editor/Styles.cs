using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.NimGui.Editor {

    internal static class EditorStyle {

        public const int TitleFontSize = 25;
        public const int SubHeaderSize = 18;

        public static readonly GUIStyle Title = new GUIStyle() {
            fontSize = TitleFontSize,
            alignment = TextAnchor.LowerCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = new GUIStyleState {
                textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
            },
        };

        public static readonly GUIStyle SubHeader = new GUIStyle() {
            fontSize = SubHeaderSize,
            alignment = TextAnchor.LowerCenter,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = new GUIStyleState {
                textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
            },
        };

        public static readonly GUIStyle CharacterLabel = new GUIStyle() {
            fontSize = 12,
            wordWrap = true,
            normal = new GUIStyleState {
                textColor = Color.gray
            }
        };
    }
}
