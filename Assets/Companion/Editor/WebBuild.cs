using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Companion.EditorTools
{
    /// <summary>
    /// Web player settings and a headless build entry point:
    /// <c>unity build &lt;project&gt; --target WebGL --execute-method Companion.EditorTools.WebBuild.Build --output-path Builds/Web</c>
    /// </summary>
    public static class WebBuild
    {
        const string DefaultOutput = "Builds/Web";

        /// <summary>
        /// WebGPU only: Entities Graphics needs compute/storage buffers that WebGL 2 lacks, so a WebGL 2
        /// fallback would just show an empty scene. The page template explains this to browsers without WebGPU.
        /// </summary>
        public static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "AI Companion";
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.WebGPU });
            PlayerSettings.WebGL.template = "PROJECT:Companion";
            // Gzip + decompression fallback lets any static server (including a local dev server) host the build.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            // Entities relies on reflection over component types; keep stripping conservative.
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Minimal);
        }

        [MenuItem("Tools/Companion/Build Web Player")]
        public static void Build()
        {
            bool succeeded = false;
            try
            {
                ConfigurePlayerSettings();
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

                string output = ArgumentValue("-buildOutput") ?? DefaultOutput;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { CompanionContentBuilder.MainScenePath },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                });

                var summary = report.summary;
                succeeded = summary.result == BuildResult.Succeeded;
                Debug.Log($"[WebBuild] {summary.result}: {summary.totalSize / (1024f * 1024f):F1} MB at {output} " +
                          $"in {summary.totalTime.TotalSeconds:F0}s ({summary.totalErrors} errors, {summary.totalWarnings} warnings)");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(succeeded ? 0 : 1);
        }

        static string ArgumentValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name)
                    return args[i + 1];
            return null;
        }
    }
}
