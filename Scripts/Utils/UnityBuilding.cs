using System;
using System.IO;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Assets;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Utils
{
    internal static class UnityBuilding
    {
        private const string TargetKey = "${TARGET}";
        internal const string DefaultTargetPath = "Builds/" + TargetKey;

        public static void Build(BuildingGroup @group)
        {
            foreach (var item in group.Items)
            {
                Build(BuildBehavior.BuildOnly, item);
            }
        }

        public static void Build(BuildBehavior behavior, BuildingData overwriteData = null)
        {
            var buildingSettings = BuildingSettings.Singleton;
            var buildingData = overwriteData ?? buildingSettings.BuildingData;
            var buildingType = buildingSettings.TypeItems[buildingData.BuildType];

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildingData.BuildTarget);
            var cppCompilerConfiguration = CalculateConfiguration(buildingType);
            if (cppCompilerConfiguration.HasValue)
            {
                PlayerSettings.SetScriptingBackend(buildTargetGroup, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, cppCompilerConfiguration.Value);
                PlayerSettings.SetIncrementalIl2CppBuild(buildTargetGroup, buildingType.CppIncrementalBuild);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(buildTargetGroup, ScriptingImplementation.Mono2x);
            }

            EditorUserBuildSettings.buildAppBundle = buildingType.BuildAppBundle;

            var targetPath = DefaultTargetPath.Replace(TargetKey, buildingData.BuildTarget.ToString()) + "/" + buildingType.TargetPath;
            var appName = buildingSettings.AppName + GetExtension(buildingData.BuildTarget);
            var options = new BuildPlayerOptions
            {
                scenes = KnownScenes,
                target = buildingData.BuildTarget,
                locationPathName = targetPath + "/" + appName,
                options = CalculateOptions(buildingType, buildingData.BuildExtras, behavior, buildingSettings.Clean, buildingSettings.ShowFolder),
                extraScriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(',', StringSplitOptions.RemoveEmptyEntries).Concat(buildingType.Defines).ToArray()
            };

            if (buildingSettings.Clean && Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }

            var oldBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            var oldBuildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            try
            {
                var buildReport = BuildPipeline.BuildPlayer(options);
                if (buildReport.summary.result != BuildResult.Succeeded)
                {
                    EditorUtility.DisplayDialog("Build", "Build has failed", "OK");
                }
            }
            finally
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.FromBuildTargetGroup(oldBuildGroup), oldBuildTarget);
            }
        }

        private static string[] KnownScenes => EditorBuildSettings.scenes.Select(x => x.path).ToArray();

        private static BuildOptions CalculateOptions(BuildingTypeItem buildingType, BuildExtras buildExtras, BuildBehavior behavior, bool clean, bool showFolder)
        {
            var options = BuildOptions.None;
            if (buildingType.Compress)
            {
                options |= BuildOptions.CompressWithLz4HC;
            }

            if (buildingType.AllowDebugging)
            {
                options |= BuildOptions.AllowDebugging;
            }

            if (buildingType.DevelopmentBuild)
            {
                options |= BuildOptions.Development;
            }

            if (buildExtras.HasFlag(BuildExtras.CodeCoverage))
            {
                options |= BuildOptions.EnableCodeCoverage;
            }

            if (buildExtras.HasFlag(BuildExtras.StrictMode))
            {
                options |= BuildOptions.StrictMode;
            }

            if (buildExtras.HasFlag(BuildExtras.UseProfiler))
            {
                options |= BuildOptions.ConnectWithProfiler;
                options |= BuildOptions.EnableDeepProfilingSupport;
            }

            if (showFolder)
            {
                options |= BuildOptions.ShowBuiltPlayer;
            }

            if (buildExtras.HasFlag(BuildExtras.WaitForConnection))
            {
                options |= BuildOptions.WaitForPlayerConnection;
            }

            if (buildExtras.HasFlag(BuildExtras.ConnectToHost))
            {
                options |= BuildOptions.ConnectToHost;
            }

            if (buildExtras.HasFlag(BuildExtras.DetailedReport))
            {
                options |= BuildOptions.DetailedBuildReport;
            }

            if (buildExtras.HasFlag(BuildExtras.SymlinkSources))
            {
                options |= BuildOptions.SymlinkSources;
            }

            switch (behavior)
            {
                case BuildBehavior.BuildOnly:
                    break;
                case BuildBehavior.BuildAndRun:
                    options |= BuildOptions.AutoRunPlayer;
                    break;
                case BuildBehavior.BuildScriptsOnly:
                    options |= BuildOptions.BuildScriptsOnly;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (clean)
            {
                options |= BuildOptions.CleanBuildCache;
            }

            return options;
        }

        private static Il2CppCompilerConfiguration? CalculateConfiguration(BuildingTypeItem item)
        {
            return item.CppSettings switch
            {
                IL2CPPSettings.Deactivated => null,
                IL2CPPSettings.Debug => Il2CppCompilerConfiguration.Debug,
                IL2CPPSettings.Master => Il2CppCompilerConfiguration.Master,
                IL2CPPSettings.Release => Il2CppCompilerConfiguration.Release,
                _ => throw new NotImplementedException()
            };
        }

        private static string GetExtension(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.iOS:
                case BuildTarget.WebGL:
                case BuildTarget.WSAPlayer:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.PS4:
                case BuildTarget.XboxOne:
                case BuildTarget.tvOS:
                case BuildTarget.Switch:
                case BuildTarget.Lumin:
                case BuildTarget.Stadia:
                case BuildTarget.CloudRendering:
                case BuildTarget.GameCoreXboxOne:
                case BuildTarget.PS5:
                case BuildTarget.EmbeddedLinux:
                case BuildTarget.NoTarget:
                    return "";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.Android:
                    return EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null);
            }
        }

        public enum BuildBehavior
        {
            BuildOnly,
            BuildAndRun,
            BuildScriptsOnly,
        }
    }
}