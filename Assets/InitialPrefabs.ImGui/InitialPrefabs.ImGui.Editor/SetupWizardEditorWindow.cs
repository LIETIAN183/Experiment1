using InitialPrefabs.NimGui.Render;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#if URP_ENABLED 
using UnityEngine.Rendering.Universal;
#endif

namespace InitialPrefabs.NimGui.Editor {

    public class SetupWizardEditorWindow : EditorWindow {

        [MenuItem("Tools/NimGui/Setup Wizard", false, 1)]
        static void ShowWindow() {
            var window = EditorWindow.GetWindow<SetupWizardEditorWindow>("Setup Wizard");
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }

        [InitializeOnLoadMethod]
        static void InitOnEditor() {
            if (!EditorPrefs.HasKey(StartupKey)) {
                EditorPrefs.SetBool(StartupKey, true);
            }

            var show = EditorPrefs.GetBool(StartupKey);
            var isWindowOpened = EditorWindow.HasOpenInstances<SetupWizardEditorWindow>();

            // Only show the window if 
            // - the preference is to show
            // - we don't have an active Window
            // - we are not switching to play mode
            if (show && 
                !isWindowOpened && 
                !EditorApplication.isPlayingOrWillChangePlaymode) {
                ShowWindow();
            }
        }

        const string StartupKey = "ImGuiShowStartUp";
        const string SDFMessage = "Your project is missing the SDF shader! Your builds will " +
            "not render NimGui properly without the shader. Press the button below to " +
            "add it to your Graphics Settings' Always Included Shaders.";

        const string MissingSRP = "You have enabled URP in NimGui, but have not assigned " +
            "the Scriptable Render Pipeline Settings in your Project's Graphics Settings.";

        const string BuiltinMsg = "Please call DefaultImGuiInitialization.SetupCamera(camera, CameraEvent) to " + 
            "initialize with the Builtin RenderPipeline in a MonoBehaviour.\n" + 
            "If you want to use URP, please add URP_ENABLED to your Project Setting's Scripting Defines.";

        SerializedObject graphicsObj;
        SerializedProperty includes;
        Shader sdf;
        Texture2D documentation;
        Texture2D email;
        Texture2D forum;
        Texture2D bugs;

        void OnEnable() {
            sdf = Shader.Find("InitialPrefabs/SDF");
            var settings = GetGraphicsSettings();

            if (settings != null) {
                graphicsObj = new SerializedObject(settings);
                includes = graphicsObj.FindProperty("m_AlwaysIncludedShaders");
            }

            documentation = AssetDatabaseUtils.Query("t: Texture documentation").First().As<Texture2D>();
            email = AssetDatabaseUtils.Query("t: Texture email").First().As<Texture2D>();
            forum = AssetDatabaseUtils.Query("t: Texture forum").First().As<Texture2D>();
            bugs = AssetDatabaseUtils.Query("t: Texture bugs").First().As<Texture2D>();
        }

        void OnGUI() {
            EditorGUILayout.LabelField("NimGui Setup Wizard", EditorStyle.Title);

            GUILayout.Label("RenderPipeline Setup", EditorStyle.SubHeader);
            bool showRpWarning = !IsRenderPipelineSetup(out var rpMessage, out var messageType, out var buttonEnabled);

            if (showRpWarning) {
                EditorGUILayout.HelpBox(rpMessage, messageType);
            } else {
                EditorGUILayout.HelpBox("ImGuiRenderFeature(s) exists on all of your ForwardRenderers.", MessageType.Info);
            }

#if URP_ENABLED
            GUI.enabled = showRpWarning;
            if (buttonEnabled) {
                if (GUILayout.Button(new GUIContent(
                        "Fix now",
                        null,
                        "This will add the ImGuiRenderFeature to all ForwardRenderers. Press Ctrl/Cmd Z to undo this action."))) {

                    string[] guids = AssetDatabase.FindAssets("t: ForwardRendererData");

                    foreach (var guid in guids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
#if UNITY_2020_3_OR_NEWER && !UNITY_2021_2_OR_NEWER
                        var forwardRenderer = AssetDatabase.LoadAssetAtPath<ForwardRendererData>(path);
#elif UNITY_2021_2_OR_NEWER
                        var forwardRenderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
#endif
                        var serializedForwardRenderer = new SerializedObject(forwardRenderer);
                        var rendererFeatures = serializedForwardRenderer.FindProperty("m_RendererFeatures");
                        var rendererMap = serializedForwardRenderer.FindProperty("m_RendererFeatureMap");

                        if (!ContainsImGui(rendererFeatures)) {
                            serializedForwardRenderer.Update();
                            var imguiFeature = ScriptableObject.CreateInstance(
                                typeof(ImGuiRenderFeature)) as ImGuiRenderFeature;

                            imguiFeature.Event = RenderPassEvent.AfterRenderingPostProcessing;
                            imguiFeature.name = "NewImGuiRenderFeature";

                            AssetDatabase.AddObjectToAsset(imguiFeature, path);
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(imguiFeature,
                                out string renderFeatureGuid, out long localId);

                            var last = rendererFeatures.arraySize;
                            rendererFeatures.arraySize++;
                            var componentProp = rendererFeatures.GetArrayElementAtIndex(last);
                            componentProp.objectReferenceValue = imguiFeature;

                            rendererMap.arraySize++;
                            var mapElement = rendererMap.GetArrayElementAtIndex(last);
                            mapElement.longValue = localId;

                            serializedForwardRenderer.ApplyModifiedProperties();
                        }
                    }
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }
            GUI.enabled = true;
#endif

            bool showSDFWarning = !DoesSDFShaderExist();

            GUILayout.Label("Shader Setup", EditorStyle.SubHeader);

            if (showSDFWarning) {
                EditorGUILayout.HelpBox(SDFMessage,
                    MessageType.Warning);
            } else {
                EditorGUILayout.HelpBox("The SDF shader is included in the build.", MessageType.Info);
            }

            GUI.enabled = showSDFWarning;
            if (GUILayout.Button(new GUIContent("Add Shader"))) {
                graphicsObj.Update();

                var last = includes.arraySize++;
                var lastElement = includes.GetArrayElementAtIndex(last);
                lastElement.objectReferenceValue = sdf;

                graphicsObj.ApplyModifiedProperties();
                AssetDatabase.Refresh();
            }
            GUI.enabled = true;

            var height = GUILayout.Height(30f);

            EditorGUILayout.Space();
            GUILayout.Label("Resources", EditorStyle.SubHeader);
            if (GUILayout.Button(
                    new GUIContent(
                        "Documentation",
                        documentation,
                        "Visit the documentation online."),
                    height)) {

                Application.OpenURL("https://initialprefabs.gitlab.io/imgui.book");
            }

            EditorGUILayout.Space();
            GUILayout.Label("Support", EditorStyle.SubHeader);

            if (GUILayout.Button(
                    new GUIContent(
                        "Email",
                        email,
                        "Have a question, send us an email."),
                    height)) {
                Application.OpenURL("mailto: info@initialprefabs.com");
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Forum",
                        forum,
                        "Visit the forums for discussions."),
                    height)) {
                Application.OpenURL("https://forum.unity.com/threads/in-development-nimgui-a-1-draw-call-immediate-mode-gui-for-unity.1171601/");
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Bugs",
                        bugs,
                        "You can post bug reports here."),
                    height)) {
                Application.OpenURL("https://github.com/InitialPrefabs/nimgui/issues");
            }

            var value = EditorPrefs.GetBool(StartupKey);
            bool showOnStartup = GUILayout.Toggle(value, "Show On Startup");
            EditorPrefs.SetBool(StartupKey, showOnStartup);
        }

        GraphicsSettings GetGraphicsSettings() {
            return AssetDatabase.LoadAssetAtPath<GraphicsSettings>(
                "ProjectSettings/GraphicsSettings.asset");
        }

        bool DoesSDFShaderExist() {
            for (int i = 0; i < includes.arraySize; ++i) {
                var element = includes.GetArrayElementAtIndex(i);
                var shader = (Shader)element.objectReferenceValue;

                if (sdf == shader) {
                    return true;
                }
            }
            return false;
        }

        bool ContainsImGui(SerializedProperty property) {
#if URP_ENABLED
            for (int i = 0; i < property.arraySize; i++) {
                var element = property.GetArrayElementAtIndex(i);
                var imguiFeature = element.objectReferenceValue as ImGuiRenderFeature;

                if (imguiFeature != null) {
                    return true;
                }
            }
#endif
            return false;
        }

        bool IsRenderPipelineSetup(out string message, out MessageType messageType, out bool enableButton) {
#if URP_ENABLED
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) {
                message = MissingSRP;
                messageType = MessageType.Error;
                enableButton = false;
                return false;
            }

            if (!PipelineUtils.TryGetRendererAssets<ImGuiRenderFeature, ImGuiRenderPass>(out var renderFeature)) {
                message = "Your UniversalRP Forward Renderers are missing the ImGuiRenderFeature.";
                messageType = MessageType.Error;
                enableButton = true;
                return false;
            }

            messageType = MessageType.Info;
            message = "Your Render Pipeline is setup.";
            enableButton = false;
            return true;
#else
            message =  BuiltinMsg;
            messageType = MessageType.Warning;
            enableButton = false;
            return false;
#endif
        }
    }
}
