using System;
using System.IO;
using Companion.Authoring;
using Unity.Mathematics;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Companion.EditorTools
{
    /// <summary>
    /// Generates the companion's materials, pose library, SubScene and main scene, and applies the
    /// project settings Entities Graphics needs. Safe to re-run; pose edits are kept unless reset.
    /// </summary>
    public static class CompanionContentBuilder
    {
        const string GeneratedFolder = "Assets/Companion/Generated";
        const string MaterialsFolder = GeneratedFolder + "/Materials";
        const string TexturesFolder = GeneratedFolder + "/Textures";
        const string PoseLibraryPath = GeneratedFolder + "/CompanionPoseLibrary.asset";
        const string SubSceneFolder = "Assets/Scenes/Companion";
        const string SubScenePath = SubSceneFolder + "/CompanionSubScene.unity";
        public const string MainScenePath = "Assets/Scenes/Main.unity";

        static readonly Color BallColor = new Color32(255, 200, 87, 255);
        static readonly Color FeatureColor = new Color32(42, 36, 51, 255);
        static readonly Color NoseColor = new Color32(244, 160, 70, 255);
        static readonly Color MouthFillColor = new Color32(122, 46, 58, 255);
        static readonly Color ShadowTint = new Color32(60, 52, 90, 255);
        public static readonly Color BackgroundColor = new Color32(220, 230, 247, 255);

        struct Materials
        {
            public Material Ball, Feature, Nose, MouthFill, Highlight, Shadow;
        }

        [MenuItem("Tools/Companion/Rebuild All")]
        public static void BuildAll() => Run(resetPoses: false);

        [MenuItem("Tools/Companion/Reset Poses To Defaults")]
        public static void ResetPosesAndBuild() => Run(resetPoses: true);

        static void Run(bool resetPoses)
        {
            try
            {
                EnsureFolder(MaterialsFolder);
                EnsureFolder(TexturesFolder);
                EnsureFolder(SubSceneFolder);

                var materials = CreateMaterials();
                var library = LoadOrCreatePoseLibrary(resetPoses);
                ConfigureRenderPipeline();
                WebBuild.ConfigurePlayerSettings();
                BuildSubScene(materials, library);
                BuildMainScene();
                AssetDatabase.SaveAssets();
                Debug.Log("[CompanionContentBuilder] Rebuilt materials, poses, SubScene and main scene.");
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        // ---------------------------------------------------------------- assets

        static Materials CreateMaterials()
        {
            var lit = FindShader("Universal Render Pipeline/Lit");
            var unlit = FindShader("Universal Render Pipeline/Unlit");
            return new Materials
            {
                Ball = Lit(lit, "Ball", BallColor, 0.45f),
                Feature = Lit(lit, "Feature", FeatureColor, 0.85f),
                Nose = Lit(lit, "Nose", NoseColor, 0.5f),
                MouthFill = Lit(lit, "MouthFill", MouthFillColor, 0.3f),
                Highlight = Unlit(unlit, "EyeHighlight", Color.white),
                Shadow = ShadowMaterial(unlit, CreateShadowTexture()),
            };
        }

        static Shader FindShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null)
                throw new InvalidOperationException($"Shader '{name}' not found. Is URP installed?");
            return shader;
        }

        static Material Lit(Shader shader, string name, Color color, float smoothness)
        {
            var material = LoadOrCreateMaterial(shader, name);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Unlit(Shader shader, string name, Color color)
        {
            var material = LoadOrCreateMaterial(shader, name);
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material ShadowMaterial(Shader unlit, Texture2D texture)
        {
            var material = LoadOrCreateMaterial(unlit, "SoftShadow");
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", ShadowTint);
            // URP transparent, alpha-blended surface.
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material LoadOrCreateMaterial(Shader shader, string name)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            return material;
        }

        static Texture2D CreateShadowTexture()
        {
            const int size = 128;
            string path = TexturesFolder + "/SoftShadow.png";
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float2 uv = (new float2(x, y) + 0.5f) / size * 2f - 1f;
                float alpha = math.pow(1f - math.smoothstep(0f, 1f, math.length(uv)), 1.4f) * 0.5f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static CompanionPoseLibrary LoadOrCreatePoseLibrary(bool reset)
        {
            var library = AssetDatabase.LoadAssetAtPath<CompanionPoseLibrary>(PoseLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<CompanionPoseLibrary>();
                DefaultPoses.Populate(library);
                AssetDatabase.CreateAsset(library, PoseLibraryPath);
            }
            else if (reset)
            {
                DefaultPoses.Populate(library);
                EditorUtility.SetDirty(library);
            }
            return library;
        }

        // ---------------------------------------------------------------- settings

        /// <summary>Entities Graphics on URP requires Forward+; MSAA keeps the ball's silhouette smooth.</summary>
        static void ConfigureRenderPipeline()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRendererData", new[] { "Assets" }))
            {
                var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(AssetDatabase.GUIDToAssetPath(guid));
                if (renderer == null || renderer.renderingMode == UnityEngine.Rendering.Universal.RenderingMode.ForwardPlus)
                    continue;
                renderer.renderingMode = UnityEngine.Rendering.Universal.RenderingMode.ForwardPlus;
                EditorUtility.SetDirty(renderer);
            }

            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets" }))
            {
                var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (pipeline == null || pipeline.msaaSampleCount == 4)
                    continue;
                pipeline.msaaSampleCount = 4;
                EditorUtility.SetDirty(pipeline);
            }
        }

        // ---------------------------------------------------------------- scenes

        static void BuildSubScene(Materials materials, CompanionPoseLibrary library)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("Companion");
            var companion = root.AddComponent<CompanionAuthoring>();
            companion.poseLibrary = library;
            float radius = companion.ballRadius;
            var dims = companion.Dimensions;

            Primitive(PrimitiveType.Quad, "Shadow", root.transform, new Vector3(0f, 0.003f, 0f), Quaternion.Euler(90f, 0f, 0f),
                new Vector3(1.15f, 1.15f, 1f), materials.Shadow, PartKind.Shadow);

            var body = Node("Body", root.transform, Vector3.zero, Quaternion.identity, Vector3.one, PartKind.Body);
            var centre = Node("BallCenter", body, new Vector3(0f, radius, 0f), Quaternion.identity, Vector3.one, PartKind.Fixed);
            Primitive(PrimitiveType.Sphere, "Ball", centre, Vector3.zero, Quaternion.identity, Vector3.one * (2f * radius), materials.Ball, PartKind.Fixed);

            var face = Node("Face", centre, Vector3.zero, Quaternion.identity, Vector3.one, PartKind.Face);
            foreach (int side in new[] { -1, 1 })
            {
                string suffix = side < 0 ? "L" : "R";

                var eyePivot = Node($"EyePivot_{suffix}", face, Vector3.zero, Surface(side * 15f, 5.5f), Vector3.one, PartKind.Fixed);
                var eye = Primitive(PrimitiveType.Sphere, $"Eye_{suffix}", eyePivot, new Vector3(0f, 0f, -(radius - 0.004f)), Quaternion.identity,
                    new Vector3(0.095f, 0.125f, 0.05f), materials.Feature, PartKind.Eye, side);
                Primitive(PrimitiveType.Sphere, $"EyeHighlight_{suffix}", eye, new Vector3(0.17f, 0.2f, -0.36f), Quaternion.identity,
                    Vector3.one * 0.3f, materials.Highlight, PartKind.Fixed);

                var browPivot = Node($"BrowPivot_{suffix}", face, Vector3.zero, Surface(side * 17f, 23f), Vector3.one, PartKind.BrowPivot, side);
                Primitive(PrimitiveType.Capsule, $"Brow_{suffix}", browPivot, new Vector3(0f, 0f, -(radius - 0.002f)), Quaternion.Euler(0f, 0f, 90f),
                    new Vector3(0.042f, 0.075f, 0.028f), materials.Feature, PartKind.Brow, side);
            }

            var nosePivot = Node("NosePivot", face, Vector3.zero, Surface(0f, -5f), Vector3.one, PartKind.Fixed);
            Primitive(PrimitiveType.Sphere, "Nose", nosePivot, new Vector3(0f, 0f, -(radius - 0.006f)), Quaternion.identity,
                new Vector3(0.075f, 0.06f, 0.05f), materials.Nose, PartKind.Fixed);

            // Mouth parts are laid out at rest here; RigApplySystem rebuilds them every frame.
            var mouthPivot = Node("MouthPivot", face, Vector3.zero, Surface(0f, -20f), Vector3.one, PartKind.Fixed);
            var rest = MouthShape.From(RigParams.AtRest());
            for (int i = 0; i < MouthContour.SegmentCount; i++)
            {
                MouthContour.Segment(dims, rest, i, out var position, out var rotation, out var scale);
                Primitive(PrimitiveType.Capsule, $"MouthLine_{i:00}", mouthPivot, position, rotation, scale, materials.Feature, PartKind.MouthSegment, 0, i);
            }
            for (int i = 0; i < MouthContour.FillColumns; i++)
            {
                MouthContour.Fill(dims, rest, i, out var position, out var rotation, out var scale);
                Primitive(PrimitiveType.Capsule, $"MouthFill_{i}", mouthPivot, position, rotation, scale, materials.MouthFill, PartKind.MouthFill, 0, i);
            }

            EditorSceneManager.SaveScene(scene, SubScenePath);
        }

        static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 0.62f, -4.2f), Quaternion.Euler(1.5f, 0f, 0f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 30f;
            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = false;
            cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;

            // Warm key light from the upper right, cool fill from the left.
            AddDirectionalLight("Key Light", Quaternion.Euler(38f, -28f, 0f), new Color(1f, 0.96f, 0.9f), 1.15f);
            AddDirectionalLight("Fill Light", Quaternion.Euler(10f, 35f, 0f), new Color(0.78f, 0.84f, 1f), 0.45f);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.78f, 0.8f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.6f, 0.68f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.38f, 0.4f);

            var subScene = new GameObject("Companion SubScene").AddComponent<SubScene>();
            subScene.SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubScenePath);
            subScene.AutoLoadScene = true;

            EditorSceneManager.SaveScene(scene, MainScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
        }

        static void AddDirectionalLight(string name, Quaternion rotation, Color color, float intensity)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = rotation;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>Pivot rotation that places a child at local -Z onto the ball at the given angles (degrees).</summary>
        static Quaternion Surface(float azimuthDegrees, float elevationDegrees)
        {
            return SphereMath.Orientation(math.radians(azimuthDegrees), math.radians(elevationDegrees));
        }

        static Transform Node(string name, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale,
            PartKind kind, int side = 0, int index = 0)
        {
            var go = new GameObject(name);
            Place(go.transform, parent, position, rotation, scale);
            AddRigNode(go, kind, side, index);
            return go.transform;
        }

        static Transform Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale,
            Material material, PartKind kind, int side = 0, int index = 0)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Place(go.transform, parent, position, rotation, scale);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            AddRigNode(go, kind, side, index);
            return go.transform;
        }

        static void Place(Transform transform, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.SetParent(parent, false);
            transform.SetLocalPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }

        static void AddRigNode(GameObject go, PartKind kind, int side, int index)
        {
            var node = go.AddComponent<RigNodeAuthoring>();
            node.kind = kind;
            node.side = side;
            node.index = index;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
