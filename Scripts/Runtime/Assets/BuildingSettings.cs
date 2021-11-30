using System;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets
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

                    settings = BuildingSettingsFactory.Create();
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
        private SingleBuildingData buildingData = new SingleBuildingData();

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

        [SerializeField]
        private BuildingGroup[] groupItems = Array.Empty<BuildingGroup>();

        #endregion

        #region Properties

        public BuildingData BuildingData => buildingData;

        public string AppName => appName;

        public BuildingTypeItem[] TypeItems
        {
            get => typeItems;
            internal set => typeItems = value;
        }

        public BuildingGroup[] GroupItems
        {
            get => groupItems;
            internal set => groupItems = value;
        }

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
    internal sealed class BuildingGroup
    {
        #region Inspector Data

        [SerializeField]
        private string name;

        [SerializeField]
        private BuildingData[] items;

        #endregion

        #region Properties

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        public BuildingData[] Items
        {
            get => items;
            internal set => items = value;
        }

        #endregion
    }

    [Serializable]
    internal class BuildingData
    {
        #region Inspector Data

        [SerializeField]
        private BuildTarget buildTarget;

        [SerializeField]
        private int buildType;

        [SerializeField]
        private BuildExtras buildExtras;

        #endregion

        #region Properties

        public virtual BuildTarget BuildTarget
        {
            get => buildTarget;
            internal set => buildTarget = value;
        }

        public int BuildType
        {
            get => buildType;
            internal set => buildType = value;
        }

        public BuildExtras BuildExtras
        {
            get => buildExtras;
            internal set => buildExtras = value;
        }

        #endregion
    }

    [Serializable]
    internal sealed class SingleBuildingData : BuildingData
    {
        #region Inspector Data

        [HideInInspector]
        [SerializeField]
        private bool buildTargetOverwritten;

        #endregion

        #region Property Data

        public override BuildTarget BuildTarget
        {
            get => buildTargetOverwritten ? base.BuildTarget : EditorUserBuildSettings.activeBuildTarget;
            internal set
            {
                if (base.BuildTarget == value)
                    return;

                base.BuildTarget = value;
                buildTargetOverwritten = true;
            }
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

#if UNITY_2021_2_OR_NEWER
        [SerializeField]
        private Il2CppCodeGeneration cppCodeGeneration = Il2CppCodeGeneration.OptimizeSpeed;
#endif

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

#if UNITY_2021_2_OR_NEWER
        public Il2CppCodeGeneration CppCodeGeneration
        {
            get => cppCodeGeneration;
            internal set => cppCodeGeneration = value;
        }
#endif

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

    [Flags]
    public enum BuildExtras
    {
        None = 0x0000,
        CodeCoverage = 0x0001,
        UseProfiler = 0x0002,
        StrictMode = 0x0004,
        WaitForConnection = 0x0010,
        ConnectToHost = 0x0020,
        DetailedReport = 0x0040,
        SymlinkSources = 0x0080
    }
}