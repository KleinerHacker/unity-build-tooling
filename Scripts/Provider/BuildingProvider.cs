using System;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Toolbar;
using UnityBuildTooling.Editor.build_tooling.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Provider
{
    public sealed class BuildingProvider : SettingsProvider
    {
        #region Static Area

        [SettingsProvider]
        public static SettingsProvider CreateGameSettingsProvider()
        {
            return new BuildingProvider();
        }

        #endregion

        private SerializedObject _settings;
        private SerializedProperty _appNameProperty;
        private SerializedProperty _typeItemsProperty;

        public BuildingProvider()
            : base("Project/Build Tools", SettingsScope.Project, new[] { "Build", "Building", "Tool", "Tooling", "Run", "Running", "Compile", "Compiling" })
        {
        }

        #region Builtin Methods

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = BuildingSettings.SerializedSingleton;
            if (_settings == null)
                return;
            _appNameProperty = _settings.FindProperty("appName");
            _typeItemsProperty = _settings.FindProperty("typeItems");
            if (_typeItemsProperty == null)
                throw new InvalidOperationException("Items not found");
        }

        public override void OnGUI(string searchContext)
        {
            if (_settings == null || _appNameProperty == null || _typeItemsProperty == null)
                return;
            
            _settings.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_appNameProperty, new GUIContent("Name of application"), GUILayout.ExpandWidth(true));
            GUILayout.Space(15f);
            if (GUILayout.Button("Reset", GUILayout.Width(100f)))
            {
                _appNameProperty.stringValue = Application.productName;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(15f);
            EditorGUILayout.PropertyField(_typeItemsProperty, new GUIContent("Building Types"));

            _settings.ApplyModifiedProperties();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Space(25f);
            EditorGUILayout.LabelField("Common Build Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Scenes:");
            EditorGUILayout.LabelField(string.Join(Environment.NewLine, EditorBuildSettings.scenes.Select(x => x.path).ToArray()), EditorStyles.wordWrappedLabel);
            EditorGUI.EndDisabledGroup();
        }

        #endregion
    }

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
                            CPPSettings = IL2CPPSettings.Deactivated,
                            StrippingLevel = ManagedStrippingLevel.Disabled,
                        },
                        new BuildingTypeItem
                        {
                            Name = "Debug Native",
                            TargetPath = "DebugNative",
                            DevelopmentBuild = true,
                            Compress = false,
                            AllowDebugging = true,
                            Defines = new[] { "DEBUG" },
                            CPPSettings = IL2CPPSettings.Debug,
                            StrippingLevel = ManagedStrippingLevel.Disabled,
                        },
                        new BuildingTypeItem
                        {
                            Name = "Release",
                            TargetPath = "Release",
                            DevelopmentBuild = false,
                            Compress = true,
                            AllowDebugging = false,
                            Defines = new[] { "RELEASE" },
                            CPPSettings = IL2CPPSettings.Deactivated,
                            StrippingLevel = ManagedStrippingLevel.Low,
                        },
                        new BuildingTypeItem
                        {
                            Name = "Release Native",
                            TargetPath = "ReleaseNative",
                            DevelopmentBuild = false,
                            Compress = true,
                            AllowDebugging = false,
                            Defines = new[] { "RELEASE" },
                            CPPSettings = IL2CPPSettings.Master,
                            StrippingLevel = ManagedStrippingLevel.Low,
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
        private bool buildTargetOverwritten;
        [SerializeField]
        private BuildTarget buildTarget;
        [SerializeField]
        private int buildType;
        [SerializeField]
        private BuildingToolbar.BuildExtras buildExtras;
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

        public string AppName => appName;

        public BuildingTypeItem[] TypeItems => typeItems;

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
        private ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.Disabled;

        [Space]
        [SerializeField]
        private AndroidArchitecture androidArchitecture = AndroidArchitecture.All;

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

        public IL2CPPSettings CPPSettings
        {
            get => cppSettings;
            internal set => cppSettings = value;
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