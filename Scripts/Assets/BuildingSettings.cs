using System;
using UnityBuildTooling.Editor.build_tooling.Scripts.Toolbar;
using UnityBuildTooling.Editor.build_tooling.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Assets
{
    internal sealed class BuildingSettings : ScriptableObject
    {
        private const string Path = "Assets/building.asset";

        public static BuildingSettings Singleton
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<BuildingSettings>(Path);
                if (settings == null)
                {
                    Debug.Log("Unable to find game settings, create new");

                    settings = CreateInstance<BuildingSettings>();
                    settings.typeItems = new[]
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
                            StrippingLevel = ManagedStrippingLevel.Low,
                            BuildAppBundle = true,
                        }
                    };
                    AssetDatabase.CreateAsset(settings, Path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                return settings;
            }
        }

        public static SerializedObject SerializedSingleton => new SerializedObject(Singleton);

        #region Inspector Data

        [SerializeField]
        private BuildingData buildingData = new BuildingData();
        [SerializeField]
        private bool clean = true;
        [SerializeField]
        private bool showFolder = true;

        [FormerlySerializedAs("targetName")]
        [SerializeField]
        private string appName;

        [FormerlySerializedAs("items")]
        [SerializeField]
        private BuildingTypeItem[] typeItems = Array.Empty<BuildingTypeItem>();

        #endregion

        #region Properties

        public BuildingData BuildingData => buildingData;

        public string AppName => appName;

        public BuildingTypeItem[] TypeItems => typeItems;

        public bool Clean
        {
            get => clean;
            internal set => clean = value;
        }

        public bool ShowFolder
        {
            get => showFolder;
            internal set => showFolder = value;
        }

        #endregion

        #region Builtin Methods

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = Application.productName;
            }
        }

        #endregion

        internal void ResetBuildTarget()
        {
            buildingData.ResetBuildTarget();
        }
    }

    [Serializable]
    internal sealed class BuildingData
    {
        #region Inspector Data

        [SerializeField]
        private bool buildTargetOverwritten;
        [SerializeField]
        private BuildTarget buildTarget;
        [SerializeField]
        private int buildType;
        [SerializeField]
        private BuildingToolbar.BuildExtras buildExtras;

        #endregion

        #region Properties

        public BuildTarget BuildTarget
        {
            get => buildTargetOverwritten ? buildTarget : EditorUserBuildSettings.activeBuildTarget;
            internal set
            {
                if (buildTarget == value)
                    return;
                
                buildTarget = value;
                buildTargetOverwritten = true;
            }
        }

        public int BuildType
        {
            get => buildType;
            internal set => buildType = value;
        }

        public BuildingToolbar.BuildExtras BuildExtras
        {
            get => buildExtras;
            internal set => buildExtras = value;
        }

        #endregion
        
        internal void ResetBuildTarget()
        {
            buildTargetOverwritten = false;
        }
    }

    [Serializable]
    internal sealed class BuildingTypeItem
    {
        #region Inspector Data

        [SerializeField]
        private string name;

        [Space]
        [SerializeField]
        [Tooltip("Sub path of default target path <" + UnityBuilding.DefaultTargetPath + ">")]
        private string targetPath;

        [Header("Build Options")]
        [SerializeField]
        private bool developmentBuild;

        [SerializeField]
        private bool allowDebugging;

        [SerializeField]
        private bool compress;

        [SerializeField]
        private IL2CPPSettings cppSettings;

        [SerializeField]
        private bool cppIncrementalBuild;

        [SerializeField]
        private ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.Disabled;

        [Header("Android")]
        [SerializeField]
        private AndroidArchitecture androidArchitecture = AndroidArchitecture.All;

        [SerializeField]
        private bool buildAppBundle;

        [Header("iOS")]
        [SerializeField]
        private AppleMobileArchitecture appleMobileArchitecture = AppleMobileArchitecture.Universal;

        [Space]
        [SerializeField]
        private string[] defines;

        #endregion

        #region Properties

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        public string TargetPath
        {
            get => targetPath;
            internal set => targetPath = value;
        }

        public bool DevelopmentBuild
        {
            get => developmentBuild;
            internal set => developmentBuild = value;
        }

        public bool AllowDebugging
        {
            get => allowDebugging;
            internal set => allowDebugging = value;
        }

        public bool Compress
        {
            get => compress;
            internal set => compress = value;
        }

        public IL2CPPSettings CppSettings
        {
            get => cppSettings;
            internal set => cppSettings = value;
        }

        public bool CppIncrementalBuild
        {
            get => cppIncrementalBuild;
            internal set => cppIncrementalBuild = value;
        }

        public ManagedStrippingLevel StrippingLevel
        {
            get => strippingLevel;
            internal set => strippingLevel = value;
        }

        public AndroidArchitecture AndroidArchitecture
        {
            get => androidArchitecture;
            internal set => androidArchitecture = value;
        }

        public bool BuildAppBundle
        {
            get => buildAppBundle;
            internal set => buildAppBundle = value;
        }

        public AppleMobileArchitecture AppleMobileArchitecture
        {
            get => appleMobileArchitecture;
            internal set => appleMobileArchitecture = value;
        }

        public string[] Defines
        {
            get => defines;
            internal set => defines = value;
        }

        #endregion
    }

    public enum IL2CPPSettings
    {
        Deactivated,
        Debug,
        Release,
        Master,
    }
}