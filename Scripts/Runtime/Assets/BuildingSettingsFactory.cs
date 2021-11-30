using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets
{
    internal static class BuildingSettingsFactory
    {
        public static BuildingSettings Create()
        {
            var settings = ScriptableObject.CreateInstance<BuildingSettings>();
            settings.TypeItems = new[]
            {
                new BuildingTypeItem
                {
                    Name = "Debug",
                    TargetPath = "Debug",
                    DevelopmentBuild = true,
                    Compress = false,
                    AllowDebugging = true,
                    Defines = new[] { "DEBUG" },
                    CppSettings = IL2CPPSettings.Deactivated,
                    StrippingLevel = ManagedStrippingLevel.Disabled,
                    BuildAppBundle = false,
                },
                new BuildingTypeItem
                {
                    Name = "Debug Native",
                    TargetPath = "DebugNative",
                    DevelopmentBuild = true,
                    Compress = false,
                    AllowDebugging = true,
                    Defines = new[] { "DEBUG" },
                    CppSettings = IL2CPPSettings.Debug,
                    CppIncrementalBuild = true,
#if UNITY_2021_2_OR_NEWER
                    CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize,
#endif
                    StrippingLevel = ManagedStrippingLevel.Disabled,
                    BuildAppBundle = false,
                },
                new BuildingTypeItem
                {
                    Name = "Release",
                    TargetPath = "Release",
                    DevelopmentBuild = false,
                    Compress = true,
                    AllowDebugging = false,
                    Defines = new[] { "RELEASE" },
                    CppSettings = IL2CPPSettings.Deactivated,
                    StrippingLevel = ManagedStrippingLevel.Low,
                    BuildAppBundle = true,
                },
                new BuildingTypeItem
                {
                    Name = "Release Native",
                    TargetPath = "ReleaseNative",
                    DevelopmentBuild = false,
                    Compress = true,
                    AllowDebugging = false,
                    Defines = new[] { "RELEASE" },
                    CppSettings = IL2CPPSettings.Master,
                    CppIncrementalBuild = false,
#if UNITY_2021_2_OR_NEWER
                    CppCodeGeneration = Il2CppCodeGeneration.OptimizeSpeed,
#endif
                    StrippingLevel = ManagedStrippingLevel.Low,
                    BuildAppBundle = true,
                }
            };
            settings.GroupItems = new[]
            {
                new BuildingGroup
                {
                    Name = "Debug",
                    Items = new[]
                    {
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 0,
                            BuildExtras = BuildExtras.None
                        },
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 1,
                            BuildExtras = BuildExtras.None
                        }
                    }
                },
                new BuildingGroup
                {
                    Name = "Release",
                    Items = new[]
                    {
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 2,
                            BuildExtras = BuildExtras.None
                        },
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 3,
                            BuildExtras = BuildExtras.None
                        }
                    }
                },
                new BuildingGroup
                {
                    Name = "Mono",
                    Items = new[]
                    {
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 0,
                            BuildExtras = BuildExtras.None
                        },
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 2,
                            BuildExtras = BuildExtras.None
                        }
                    }
                },
                new BuildingGroup
                {
                    Name = "IL2CPP",
                    Items = new[]
                    {
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 1,
                            BuildExtras = BuildExtras.None
                        },
                        new BuildingData
                        {
                            BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                            BuildType = 3,
                            BuildExtras = BuildExtras.None
                        }
                    }
                }
            };

            return settings;
        }
    }
}