using UnityEngine;
using UnityEditor;

namespace Michsky.UI.ModernUIPack
{
    [CustomEditor(typeof(ProgressBar))]
    public class ProgressBarEditor : Editor
    {
        private ProgressBar pbTarget;
        private int currentTab;

        private void OnEnable()
        {
            pbTarget = (ProgressBar)target;
        }

        public override void OnInspectorGUI()
        {
            GUISkin customSkin;
            Color defaultColor = GUI.color;

            if (EditorGUIUtility.isProSkin == true)
                customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Dark");
            else
                customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Light");

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultColor;

            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("PB Top Header"));

            GUILayout.EndHorizontal();
            GUILayout.Space(-42);

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");

            GUILayout.BeginHorizontal();
            GUILayout.Space(17);

            currentTab = GUILayout.Toolbar(currentTab, toolbarTabs, customSkin.FindStyle("Tab Indicator"));

            GUILayout.EndHorizontal();
            GUILayout.Space(-40);
            GUILayout.BeginHorizontal();
            GUILayout.Space(17);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var currentValue = serializedObject.FindProperty("currentValue");
            var maxValue = serializedObject.FindProperty("maxValue");
            var loadingBar = serializedObject.FindProperty("loadingBar");
            var text = serializedObject.FindProperty("text");

            switch (currentTab)
            {
                case 0:
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Current Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    pbTarget.currentValue = EditorGUILayout.Slider(pbTarget.currentValue, 0, pbTarget.maxValue);

                    GUILayout.EndHorizontal();

                    if (pbTarget.loadingBar != null && pbTarget.text != null)
                    {
                        pbTarget.loadingBar.fillAmount = pbTarget.currentValue / pbTarget.maxValue;

                        pbTarget.text.text = ((int)pbTarget.currentValue).ToString("F0");
                    }

                    else
                    {
                        if (pbTarget.loadingBar == null || pbTarget.text == null)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("Some resources are not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Max Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(maxValue, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    break;

                case 1:
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Loading Bar"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(loadingBar, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Text Indicator"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(text, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}