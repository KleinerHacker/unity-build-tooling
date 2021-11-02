using System;
using System.IO;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Provider;
using UnityBuildTooling.Editor.build_tooling.Scripts.Toolbar;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Utils
{
    public static class UnityBuilding
    {
        private const string TargetKey = "${TARGET}";
        internal const string DefaultTargetPath = "Builds/" + TargetKey;

        public static void Build(bool run)
        {
            var buildingSettings = BuildingSettings.Singleton;
            var buildingType = buildingSettings.TypeItems[buildingSettings.BuildType];

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildingSettings.BuildTarget);
            var cppCompilerConfiguration = CalculateConfiguration(buildingType);
            if (cppCompilerConfiguration.HasValue)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, cppCompilerConfiguration.Value);
            }
            PlayerSettings.SetManagedStrippingLevel(buildTargetGroup, buildingType.StrippingLevel);

            var targetPath = DefaultTargetPath.Replace(TargetKey, buildingSettings.BuildTarget.ToString()) + "/" + buildingType.TargetPath;
            var appName = buildingSettings.AppName + GetExtension(buildingSettings.BuildTarget);
            var options = new BuildPlayerOptions
            {
                scenes = KnownScenes,
                target = buildingSettings.BuildTarget,
                locationPathName = targetPath + "/" + appName,
                options = CalculateOptions(buildingType, buildingSettings.BuildExtras, run, buildingSettings.Clean),
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

        private static BuildOptions CalculateOptions(BuildingTypeItem buildingType, BuildingToolbar.BuildExtras buildExtras, bool autoRun, bool clean)
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

            if (buildExtras.HasFlag(BuildingToolbar.BuildExtras.CodeCoverage))
            {
                options |= BuildOptions.EnableCodeCoverage;
            }
            
            if (buildExtras.HasFlag(BuildingToolbar.BuildExtras.StrictMode))
            {
                options |= BuildOptions.StrictMode;
            }
            
            if (buildExtras.HasFlag(BuildingToolbar.BuildExtras.UseProfiler))
            {
                options |= BuildOptions.ConnectWithProfiler;
                options |= BuildOptions.EnableDeepProfilingSupport;
            }

            if (buildExtras.HasFlag(BuildingToolbar.BuildExtras.OpenFolder))
            {
                options |= BuildOptions.ShowBuiltPlayer;
            }

            if (autoRun)
            {
                options |= BuildOptions.AutoRunPlayer;
            }

            if (clean)
            {
                options |= BuildOptions.CleanBuildCache;
            }

            return options;
        }

        private static Il2CppCompilerConfiguration? CalculateConfiguration(BuildingTypeItem item)
        {
            return item.CPPSettings switch
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
                    return ".apk";
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTarget), buildTarget, null);
            }
        }
    }
}