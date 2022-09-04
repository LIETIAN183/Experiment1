using System;
using System.Collections.Generic;
using System.Text;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
#if !URP_ENABLED
using InitialPrefabs.NimGui.Loop;
#endif

namespace InitialPrefabs.NimGui.Example
{

    struct Option<T> where T : struct, IDisposable
    {
        public T Value;

        public readonly bool IsValid;

        public Option(T value)
        {
            Value = value;
            IsValid = true;
        }

        public void Dispose()
        {
            if (IsValid)
            {
                Value.Dispose();
            }
        }
    }

    static class OptionExtensions
    {
        public static Option<ProfilerRecorder> Some(this ProfilerRecorder recorder)
        {
            return new Option<ProfilerRecorder>(recorder);
        }
    }

    public class NimGuiDemo : MonoBehaviour
    {

        readonly string[] options = new string[] { "Option 1", "Option 2", "Option 3" };
        readonly StringBuilder builder = new StringBuilder(128, 128);
        readonly StringBuilder textEditBuilder = new StringBuilder(128, 128);

        const string WindowLabel = "Widget Gallery";

#pragma warning disable CS0649
        [SerializeField]
        TextAsset loremIpsumTxtAsset;

        [SerializeField]
        Rect windowRect = new Rect(20, 20, 120, 50);

        [SerializeField]
        RenderPipelineAsset pipelineAsset;

        [SerializeField]
        bool showUnityImGui;

        [SerializeField]
        [Tooltip("This is only used when you are running the Builtin RP")]
        Camera cam;
#pragma warning restore CS0649

        bool initialState;
        bool paneState;
        string loremIpsum;
        ushort fontSize;
        string inputTest;

        Option<ProfilerRecorder> renderingRecorder;
        Option<ProfilerRecorder> workerThreadRecorder;
        Option<ProfilerRecorder> mainThreadRecorder;

        int builtinImGuiIndex;
        int builtinSliderValue;
        bool builtinToggleValue;
        bool builtinHideValue;

        RenderPipelineAsset cachedPipelineAsset;

        void OnEnable()
        {
#if UNITY_EDITOR
            // Allow the Editor to run as fast as possible.
            Application.targetFrameRate = 0;
#endif
#if !URP_ENABLED
            DefaultImGuiInitialization.SetupCamera(cam, CameraEvent.AfterEverything);
#elif URP_ENABLED && UNITY_EDITOR
            cachedPipelineAsset = GraphicsSettings.renderPipelineAsset;
            GraphicsSettings.renderPipelineAsset = pipelineAsset;
#endif
            renderingRecorder = ProfilerRecorder
                .StartNew(ProfilerCategory.Render, "Vertices Count")
                .Some();

            var availableStatHandles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(availableStatHandles);

            foreach (var h in availableStatHandles)
            {
                var statDesc = ProfilerRecorderHandle.GetDescription(h);

                if (statDesc.Name == "BuildWindowMeshJob (Burst)")
                {
                    workerThreadRecorder = ProfilerRecorder
                        .StartNew(statDesc.Category, statDesc.Name)
                        .Some();
                }
            }

            mainThreadRecorder = ProfilerRecorder
                .StartNew(ProfilerCategory.Internal, "Main Thread")
                .Some();
        }

        void Start()
        {
            loremIpsum = loremIpsumTxtAsset.text.Trim('\n');
            fontSize = 14;
        }

        void OnDisable()
        {
#if !URP_ENABLED
            DefaultImGuiInitialization.TearDownCamera(cam, CameraEvent.AfterEverything);
#endif
            renderingRecorder.Dispose();
            workerThreadRecorder.Dispose();
            mainThreadRecorder.Dispose();

#if URP_ENABLED && UNITY_EDITOR
            GraphicsSettings.renderPipelineAsset = cachedPipelineAsset;
#endif
        }

        void OnGUI()
        {
            if (showUnityImGui)
            {
                windowRect = GUI.Window(0, windowRect, DoMyWindow, WindowLabel);
            }
        }

        void DoMyWindow(int windowID)
        {
            // Make a very long rect that is 20 pixels tall.
            // This will make the window be resizable by the top
            // title bar - no matter how wide it gets.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.Label("Here are a list of widgets!");
            if (GUILayout.Button("Click here!"))
            {
                initialState = !initialState;
            }

            if (initialState)
            {
                GUILayout.Label("Click the button again to hide me!");
            }
            GUILayout.Label("GUILayout does not support Dropdowns :(");
            // Unfortunately Unity does not have a native dropdown menu in ImGui
            for (int i = 0; i < options.Length; i++)
            {
                if (GUILayout.Button(options[i]))
                {
                    builtinImGuiIndex = i;
                }
            }

            var message = builder.Clear().Append("Currently selected: ").Append(options[builtinImGuiIndex]).ToString();
            GUILayout.Label(message);

            if (builtinToggleValue = GUILayout.Toggle(builtinToggleValue, "Toggle"))
            {
                GUILayout.Label("Toggles internally keep the bool state.");
            }
            GUILayout.Label("We can also change the font size.", new GUIStyle
            {
                fontSize = builtinSliderValue,
            });
            builtinSliderValue = (int)GUILayout.HorizontalSlider(builtinSliderValue, 14, 30);

            GUILayout.Label("GUILayout does not support ProgressBars :(");

            GUILayout.TextField("Test");
            if (GUILayout.Button("Collapsible Area"))
            {
                builtinHideValue = !builtinHideValue;
            }

            if (builtinHideValue)
            {
                GUILayout.Label("You can hide groups of widgets under a collapsible area.");
            }

            GUILayout.Label("NimGui is always 1 draw call, builtin ImGui is not");
        }

        void Update()
        {
            const string paneTitle = "Extra Pane";

            if (ImGui.Button("Quit"))
            {
                Application.Quit();
            }

            ImGui.SameLine();

            if (ImGui.Button("Reset"))
            {
                ImGui.Prune(paneTitle);
                ImGui.Prune("Stats");
                var window = ImGuiContext.GetCurrentWindow();
                window.OpenPane("Stats");
                window.OpenPane("Widget Gallery");
            }

            float2 position = new float2(Screen.width, Screen.height) / 2f;
            float2 paneSize = new float2(500, 520);
            using (ImPane pane = new ImPane("Widget Gallery", position, paneSize, ImPaneFlags.Pinned))
            {
                if (pane.IsVisible)
                {
                    ImGui.Label("Here are a list of widgets!");
                    ImGui.SameLine();
                    ImTextStyle textStyle = ImTextStyle.New();
                    textStyle.WithColumn(HorizontalAlignment.Left);
                    ImGui.Label("This pane is pinned!", in textStyle);
                    ImGui.Line();

                    if (ImGui.Button("Click here!"))
                    {
                        initialState = !initialState;
                    }

                    if (initialState)
                    {
                        ImGui.Label("Click the button again to hide me!");
                    }
                    ImGui.Line();

                    ushort index = ImGui.Dropdown("Dropdown", options);
                    builder.Clear()
                        .Append("Currently selected: ")
                        .Append(options[index]);
                    ImGui.Label(builder);
                    ImGui.Line();

                    if (ImGui.Toggle("Toggle", true))
                    {
                        ImGui.Label("Toggles internally keep the bool state.", in textStyle.WithFontSize(14));

                        ImGui.Label(
                            "We can also change the font's size.",
                            in textStyle.WithFontSize(fontSize));
                    }
                    ImGui.Line();

                    fontSize = (ushort)ImGui.Slider("Font Size", 14, 30);
                    float t = 1.0f - (30.0f - fontSize) / 16.0f;
                    ImGui.Line();

                    ImGui.Label("Progress Bar");
                    ImGui.ProgressBar(t);
                    ImGui.Line();

                    ImGui.TextField("TextEdit", textEditBuilder);
                    ImGui.Line();

                    using (ImCollapsibleArea collapsibleArea = new ImCollapsibleArea("Collapsible", false))
                    {
                        if (collapsibleArea.IsVisible)
                        {
                            ImGui.Label("You can hide groups of widgets under a collapsible area.");
                            ImWindow window = ImGuiContext.GetCurrentWindow();

                            paneState = !window.IsClosed(paneTitle);

                            if (ImGui.Button(paneState ? "Show Pane" : "Close Pane"))
                            {
                                if (paneState)
                                {
                                    window.OpenPane(paneTitle);
                                }
                                else
                                {
                                    window.ClosePane(paneTitle);
                                }
                            }
                        }
                    }
                    ImGui.Line();
                    ImSkipLineStyle.New();
                    ImGui.SkipLine();

                    ImGui.Line();
                    ImGui.Label("NimGui is always 1 draw call with minimal to zero GC!");
                    ImGui.Line();
                }
            }

            float2 statsSize = new float2(300, 150);
            using (ImPane statsPane = new ImPane(
                "Stats",
                new float2(Screen.width, Screen.height) - (statsSize + new float2(0, 35)) * 0.5f,
                statsSize))
            {

                ImGui.Label("The Stats pane may be GC'ed due to the ProfilerRecorder");

                if (statsPane.IsVisible)
                {
                    if (renderingRecorder.IsValid)
                    {
                        builder.Clear()
                            .Append("Rendering: ")
                            .Append(renderingRecorder.Value.LastValue)
                            .Append(" Vertices");
                        ImGui.Label(builder);
                    }

                    if (mainThreadRecorder.IsValid)
                    {
                        builder.Clear()
                            .Append("Main Thread: ")
                            .Append(Math.Round(mainThreadRecorder.Value.LastValueAsDouble * 1e-6, 4))
                            .Append(" ms");

                        ImGui.Label(builder);
                    }

                    builder.Clear();
                    if (workerThreadRecorder.IsValid)
                    {
                        builder
                            .Append("Building UI took: ")
                            .Append(Math.Round(workerThreadRecorder.Value.LastValueAsDouble * 1e-6, 4))
                            .Append(" ms");
                    }
                    else
                    {
                        builder.Append("Please see the editor version for ingame thread times.");
                    }
                    ImGui.Label(builder);
                }
            }

            position += new float2(510, 0);
            using (ImPane pane = new ImPane(paneTitle, position, new float2(500, 450), ImPaneFlags.Closed))
            {
                if (pane.IsVisible)
                {
                    ImGui.Label("Panes are sections of a window that can group widgets together.");
                    ImGui.Label("Can be dragged, closed, collapsed, and pinned.");
                    ImGui.Line();

                    using (ImScrollArea scrollArea = new ImScrollArea("Scroll Area"))
                    {
                        ImTextStyle style = ImTextStyle.New();
                        style.WithColumn(HorizontalAlignment.Center)
                            .WithFontSize(24);

                        // Calculate the remaining line size for the current window.
                        ImWindow window = ImGuiContext.GetCurrentWindow();
                        float2 lineSize = ImGui.CalculateRemainingLineSize(
                            window,
                            style.FontSize,
                            in style.Padding);

                        ImGui.Label("Lorem Ipsum", in lineSize, in style);
                        ImGui.Label(loremIpsum);
                    }
                }
            }
        }
    }
}
