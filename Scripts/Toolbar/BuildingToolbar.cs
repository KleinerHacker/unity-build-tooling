using System;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Provider;
using UnityBuildTooling.Editor.build_tooling.Scripts.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Toolbar
{
    [InitializeOnLoad]
    public static class BuildingToolbar
    {
        private static readonly GenericMenu BuildMenu = new GenericMenu();
        private static readonly EditorToolDelegate EditorToolRefresh, EditorToolBuild;

        private static readonly BuildingSettings BuildingSettings;
        private static readonly SerializedObject SerializedObject;

        static BuildingToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            BuildMenu.AddItem(new GUIContent("Build"), false, () => Build(UnityBuilding.BuildBehavior.BuildOnly));
            BuildMenu.AddItem(new GUIContent("Build && Run"), false, () => Build(UnityBuilding.BuildBehavior.BuildAndRun));
            BuildMenu.AddSeparator(null);
            BuildMenu.AddItem(new GUIContent("Build Scripts Only"), false, () => Build(UnityBuilding.BuildBehavior.BuildScriptsOnly));
            
            EditorToolRefresh = ScriptableObject.CreateInstance<EditorToolDelegate>();
            EditorToolRefresh.Setup((Texture2D)EditorGUIUtility.IconContent("d_Refresh").image, "Reset to active target", () => BuildingSettings.ResetBuildTarget());
            
            EditorToolBuild = ScriptableObject.CreateInstance<EditorToolDelegate>();
            EditorToolBuild.Setup((Texture2D)EditorGUIUtility.IconContent("d_Settings").image, "Build the project", () => BuildMenu.ShowAsContext());
            
            BuildingSettings = BuildingSettings.Singleton;
            SerializedObject = BuildingSettings.SerializedSingleton;
        }

        static void OnToolbarGUI()
        {
            SerializedObject.Update();

            GUILayout.FlexibleSpace();

            GUILayout.Space(50f);

            GUILayout.Label("Build: ", ToolbarStyles.labelStyle);
            BuildingSettings.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(null, BuildingSettings.BuildTarget,
                v => UnityHelper.IsBuildTargetSupported((BuildTarget)v), false, ToolbarStyles.popupStyle, ToolbarLayouts.popupLayout);
            
            EditorGUILayout.EditorToolbar(EditorToolRefresh);

            GUILayout.Space(5f);

            BuildingSettings.BuildType = EditorGUILayout.Popup(BuildingSettings.BuildType, BuildingSettings.TypeItems.Select(x => x.Name).ToArray(),
                ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);

            GUILayout.Space(5f);

            GUILayout.Label("Flags: ", ToolbarStyles.labelStyle);
            BuildingSettings.BuildExtras = (BuildExtras)EditorGUILayout.EnumFlagsField(BuildingSettings.BuildExtras, ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);

            GUILayout.Space(5f);

            BuildingSettings.Clean = GUILayout.Toggle(BuildingSettings.Clean, new GUIContent("Clean", "Clean complete build cache"), ToolbarStyles.toggleStyle);

            GUILayout.Space(5f);

            BuildingSettings.ShowFolder = GUILayout.Toggle(BuildingSettings.ShowFolder, new GUIContent("Show Folder", "Open the build folder"), ToolbarStyles.toggleStyle);

            GUILayout.Space(5f);

            EditorGUILayout.EditorToolbar(EditorToolBuild);

            SerializedObject.ApplyModifiedProperties();
        }

        private static void Build(UnityBuilding.BuildBehavior behavior)
        {
            AssetDatabase.SaveAssets();
            UnityBuilding.Build(behavior);
        }

        private static class ToolbarLayouts
        {
            public static readonly GUILayoutOption[] popupLayout;
            public static readonly GUILayoutOption[] popupSmallLayout;

            static ToolbarLayouts()
            {
                popupLayout = new[]
                {
                    GUILayout.Width(150f)
                };
                popupSmallLayout = new[]
                {
                    GUILayout.Width(100f)
                };
            }
        }

        private static class ToolbarStyles
        {
            public static readonly GUIStyle popupStyle;
            public static readonly GUIStyle labelStyle;
            public static readonly GUIStyle toggleStyle;

            static ToolbarStyles()
            {
                popupStyle = new GUIStyle("Popup")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    stretchWidth = false,
                    fixedHeight = 20f,
                    margin = new RectOffset(5,5,5,5)
                };

                labelStyle = new GUIStyle("Label")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 20f,
                    wordWrap = false,
                    margin = new RectOffset(5,5,5,5)
                };

                toggleStyle = new GUIStyle("Toggle")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 20f,
                    wordWrap = false,
                    margin = new RectOffset(5,5,5,5)
                };
            }
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

        private sealed class EditorToolDelegate : EditorTool
        {
            private Action _action;
            private GUIContent _guiContent = new GUIContent();
            
            public override GUIContent toolbarIcon => _guiContent;

            public void Setup(Texture2D icon, String tooltip, Action action)
            {
                _guiContent = new GUIContent(icon, tooltip);
                _action = action;
            }

            public override void OnActivated() => _action();
        }
    }
}