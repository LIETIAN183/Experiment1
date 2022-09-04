using InitialPrefabs.NimGui.Text;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace InitialPrefabs.NimGui.Editor {

    public class NimGuiFontGenerator : EditorWindow {

        [MenuItem("Tools/NimGui/Font Generator", false, 2)]
        static void ShowWindow() {
            var window = GetWindow<NimGuiFontGenerator>("Font Editor");
            window.minSize = new Vector2(400, 800);
            window.maxSize = new Vector2(400, 800);
            window.Show();
        }

        const int Dimension = 256;

        string defaultCharacters;

        Font font;
        int fontSize = 106;
        bool useDefault = true;
        string characters = string.Empty;

        Vector2 distances = new Vector2(3, 3);

        Material material;
        RenderTexture renderTexture;
        Texture2D rawTexture;

        TextAsset FindDefaultCharacterSheet() {
            const string fileName = "default-characters.txt";
            var guids = AssetDatabase.FindAssets("t: TextAsset");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(fileName)) {
                    return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                }
            }

            throw new System.InvalidOperationException(
                "Missing default-characters.txt, are you sure you imported the package correctly?");
        }

        void OnEnable() {
            CreateRenderAssetsInternal();

            // Setup the default characters
            var textAsset = FindDefaultCharacterSheet();
            defaultCharacters = textAsset.text.Trim();
            Resources.UnloadAsset(textAsset);
        }

        void OnDisable() {
            if (material != null) {
                DestroyImmediate(material);
            }

            if (rawTexture != null) {
                DestroyImmediate(rawTexture);
            }

            if (renderTexture != null) {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
        }

        void OnGUI() {
            using (var scope = new EditorGUI.ChangeCheckScope()) {
                if (Application.isPlaying) {
                    EditorGUILayout.LabelField("Cannot edit this during play mode", EditorStyle.Title);
                    return;
                }

                CreateRenderAssetsInternal();

                EditorGUILayout.Space(25);
                EditorGUILayout.LabelField(new GUIContent("Font Generator"), EditorStyle.Title);
                EditorGUILayout.Space(15);

                var rebuildTexture = DrawOptions();

                GUI.enabled = font != null && rawTexture != null && renderTexture != null;
                bool regenerateTexture = GUILayout.Button("Update Font Texture Preview");
                GUI.enabled = true;

                var dim = new Vector2Int(Dimension, Dimension);

                if ((regenerateTexture || rebuildTexture) && !string.IsNullOrEmpty(characters)) {
                    font.RequestCharactersInTexture(characters, fontSize, FontStyle.Normal);
                    var fontMat = font.material;
                    var mainTexture = font.material.mainTexture;

                    dim = new Vector2Int(mainTexture.width, mainTexture.height);

                    // Rebuild the render texture
                    if (dim.x != renderTexture.width || dim.y != renderTexture.height) {
                        renderTexture.Release();
                        DestroyImmediate(renderTexture);

                        renderTexture = new RenderTexture(dim.x, dim.y, 0);
                        renderTexture.filterMode = FilterMode.Point;
                        renderTexture.Create();
                    }

                    // Rebuild the preview texture
                    if (dim.x != rawTexture.width || dim.y != rawTexture.height) {
#if UNITY_2021_2_OR_NEWER
                        rawTexture.Reinitialize(dim.x, dim.y, TextureFormat.ARGB32, false);
#else
                        DestroyImmediate(rawTexture);
                        rawTexture = new Texture2D(dim.x, dim.y, TextureFormat.ARGB32, false);
#endif
                    }

                    renderTexture.Release();
                    renderTexture = new RenderTexture(dim.x, dim.y, 0);
                    renderTexture.filterMode = FilterMode.Point;
                    renderTexture.Create();

                    // Assign the material's main texture and font color.
                    material.mainTexture = fontMat.mainTexture;
                    material.color = Color.white;

                    var lastRT = RenderTexture.active;
                    RenderTexture.active = renderTexture;

                    GL.PushMatrix();
                    material.SetPass(0);
                    GL.Color(new Color(1, 1, 1, 1));
                    GL.LoadPixelMatrix(0, dim.x, 0, dim.y);
                    GL.Begin(GL.QUADS);

                    GL.TexCoord(new Vector3(0, 0, 0));
                    GL.Vertex(new Vector3(0, 0, 0));

                    GL.TexCoord(new Vector3(0, 1, 0));
                    GL.Vertex(new Vector3(0, dim.y, 0));

                    GL.TexCoord(new Vector3(1, 1, 0));
                    GL.Vertex(new Vector3(dim.x, dim.y, 0));

                    GL.TexCoord(new Vector3(1, 0, 0));
                    GL.Vertex(new Vector3(dim.x, 0, 0));

                    GL.End();
                    GL.PopMatrix();

                    rawTexture.ReadPixels(new Rect(0, 0, dim.x, dim.y), 0, 0);
                    rawTexture.Apply();

                    RenderTexture.active = lastRT;
                }

                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = GUILayoutUtility.GetRect(dim.x, dim.y);
                // rect.position = lastRect.position + new Vector2(0, 50);

                if (rawTexture != null) {
                    GUI.DrawTexture(rect, renderTexture, ScaleMode.ScaleToFit, true, dim.x / dim.y);
                }

                if (GUILayout.Button("Save")) {
                    GenerateSerializedFontData();
                }
            }
        }

        bool DrawOptions() {
            var currentFont = (Font)EditorGUILayout.ObjectField("Font", font, typeof(Font), false);
            var currentSize = EditorGUILayout.IntField(
                new GUIContent(
                    "Font Size",
                    "How large should the font size be when generating an image?"),
                fontSize);
            var currentUseDefault = EditorGUILayout.Toggle("Use default characters", useDefault);

            string currentChars;

            if (useDefault) {
                currentChars = defaultCharacters;
                EditorGUILayout.LabelField(currentChars, EditorStyle.CharacterLabel);
            } else {
                currentChars = EditorGUILayout.TextField(new GUIContent(
                    "Characters",
                    "These characters will be generated into the font texture."),
                    characters);
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Render Texture", renderTexture, typeof(RenderTexture), false);
            EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
            GUI.enabled = true;
            
            distances = EditorGUILayout.Vector2Field(new GUIContent(
                "Distances",
                "x is the Inner Distance, while Y is the Outer Distance."
            ), distances);

            EditorGUILayout.LabelField(new GUIContent("Font Texture Preview",
                "When a change is detected, the font texture will be generated and shown below."),
                EditorStyle.SubHeader);

            bool didChange = currentFont != font ||
                currentSize != fontSize ||
                currentUseDefault != useDefault ||
                currentChars != characters;

            font = currentFont;
            fontSize = currentSize;
            useDefault = currentUseDefault;
            characters = currentChars;

            return didChange && font != null;
        }

        void CreateRenderAssetsInternal() {
            if (material == null) {
                var shader = Shader.Find("InitialPrefabs/SDF");
                material = new Material(shader);
            }

            if (rawTexture == null) {
                rawTexture = new Texture2D(Dimension, Dimension, TextureFormat.ARGB32, false);
                rawTexture.hideFlags = HideFlags.HideAndDontSave;
                rawTexture.filterMode = FilterMode.Point;
            }

            if (renderTexture == null) {
                renderTexture = new RenderTexture(Dimension, Dimension, 0);
                renderTexture.hideFlags = HideFlags.HideAndDontSave;
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.Create();
            }
        }

        SerializedFontData GenerateSerializedFontData() {
            var fontData = ScriptableObject.CreateInstance<SerializedFontData>();

            FontEngine.InitializeFontEngine();
            FontEngine.LoadFontFace(font, fontSize);
            FontEngine.SetFaceSize(fontSize);
            var faceInfo = FontEngine.GetFaceInfo();

            font.RequestCharactersInTexture(characters, fontSize, FontStyle.Normal);
            fontData.FontFaceInfo = ImFontFace.Create(faceInfo);

            var list = new List<ImGlyph>();
            bool addedSpace = false;

            for (int i = 0; i < characters.Length; ++i) {
                var c = characters[i];
                if (font.GetCharacterInfo(c, out var character, fontSize, FontStyle.Normal)) {
                    FontEngine.TryGetGlyphIndex((uint)c, out uint glyphIndex);

                    if (c == ' ') {
                        addedSpace = true;
                    }
                    FontEngine.TryGetGlyphWithIndexValue(
                        glyphIndex, 
                        GlyphLoadFlags.LOAD_DEFAULT, 
                        out var glyph);

                    var convertedGlyph = new ImGlyph {
                        Unicode        = (uint)c,
                        Char           = c,
                        Advance        = character.advance,
                        MetricsSize    = new float2(glyph.metrics.width, glyph.metrics.height),
                        Bearings       = new float2(glyph.metrics.horizontalBearingX, glyph.metrics.horizontalBearingY),
                        Uvs            = new float4(character.uvBottomLeft, character.uvTopRight)
                    };
                    list.Add(convertedGlyph);
                }
            }

            if (!addedSpace) {
                const char space = ' ';
                list.Add(new ImGlyph {
                    Unicode = (uint)space,
                    Char = space,
                    Advance = 53
                });
            }

            list.Sort(default(GlyphComparer));
            fontData.Glyphs = list.ToArray();

            var path = EditorUtility.SaveFolderPanel("Save", "", "");
            if (path.Length != 0) {
                var texturePath = $"{path}/{font.name}_Atlas.png";
                var sdf = new SDF(rawTexture);
                sdf.CreateSDFTexture(this.distances);
                var finalTexture = sdf.GetFinalTexture();
                var bytes = finalTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(texturePath, bytes);

                var assetPath = path.Substring(path.IndexOf("Assets"));

                var glyphAssetPath = $"{assetPath}/{font.name}_Glyphs.asset";
                AssetDatabase.CreateAsset(fontData, glyphAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Object.DestroyImmediate(finalTexture);
            }

            FontEngine.UnloadFontFace();
            return fontData;
        }
    }
}
