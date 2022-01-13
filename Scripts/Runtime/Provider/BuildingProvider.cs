using System;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Provider
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
        private SerializedProperty _groupItemsProperty;

        public BuildingProvider()
            : base("Project/Player/Building", SettingsScope.Project, new[] { "Build", "Building", "Tool", "Tooling", "Run", "Running", "Compile", "Compiling" })
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
            _groupItemsProperty = _settings.FindProperty("groupItems");
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
            
            EditorGUILayout.Space(15f);
            EditorGUILayout.PropertyField(_groupItemsProperty, new GUIContent("Building Groups"));

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
}