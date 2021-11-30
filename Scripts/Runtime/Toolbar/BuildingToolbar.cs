using System;
using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityToolbarExtender;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Toolbar
{
    [InitializeOnLoad]
    public static class BuildingToolbar
    {
        private static readonly GenericMenu BuildMenu = new GenericMenu();

        private static readonly BuildingSettings BuildingSettings;
        private static readonly SerializedObject SerializedObject;

        static BuildingToolbar()
        {
            BuildingSettings = BuildingSettings.Singleton;
            SerializedObject = BuildingSettings.SerializedSingleton;
            
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            BuildMenu.AddItem(new GUIContent("Build"), false, () => Build(UnityBuilding.BuildBehavior.BuildOnly));
            BuildMenu.AddItem(new GUIContent("Build && Run"), false, () => Build(UnityBuilding.BuildBehavior.BuildAndRun));
            BuildMenu.AddSeparator(null);
            BuildMenu.AddItem(new GUIContent("Build Scripts Only"), false, () => Build(UnityBuilding.BuildBehavior.BuildScriptsOnly));
        }

        static void OnToolbarGUI()
        {
            SerializedObject.Update();

            GUILayout.FlexibleSpace();

            GUILayout.Space(30f);

            GUILayout.Label("Build: ", ToolbarStyles.labelStyle);
            BuildingSettings.BuildingData.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(null, BuildingSettings.BuildingData.BuildTarget,
                v => UnityHelper.IsBuildTargetSupported((BuildTarget)v), false, ToolbarStyles.popupStyle, ToolbarLayouts.popupLayout);
            
            if (GUILayout.Button(new GUIContent("", (Texture2D)EditorGUIUtility.IconContent("d_Refresh").image, "Reset to active target"), ToolbarStyles.commandButtonStyle))
            {
                BuildingSettings.ResetBuildTarget();
            }

            BuildingSettings.BuildingData.BuildType = EditorGUILayout.Popup(BuildingSettings.BuildingData.BuildType, BuildingSettings.TypeItems.Select(x => x.Name).ToArray(),
                ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);

            GUILayout.Space(5f);

            GUILayout.Label("Flags: ", ToolbarStyles.labelStyle);
            BuildingSettings.BuildingData.BuildExtras = (BuildExtras)EditorGUILayout.EnumFlagsField(BuildingSettings.BuildingData.BuildExtras, ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);

            GUILayout.Space(5f);

            BuildingSettings.Clean = GUILayout.Toggle(BuildingSettings.Clean, new GUIContent("Clean", "Clean complete build cache"), ToolbarStyles.toggleStyle);

            GUILayout.Space(5f);

            BuildingSettings.ShowFolder = GUILayout.Toggle(BuildingSettings.ShowFolder, new GUIContent("Show Folder", "Open the build folder"), ToolbarStyles.toggleStyle);

            if (GUILayout.Button(new GUIContent("", (Texture2D)EditorGUIUtility.IconContent("d_Settings").image, "Build the project"), ToolbarStyles.commandButtonStyle))
            {
                BuildMenu.ShowAsContext();
            }
            if (GUILayout.Button(new GUIContent("", (Texture2D)EditorGUIUtility.IconContent("_Menu").image, "Build a group"), ToolbarStyles.commandButtonStyle))
            {
                var groupMenu = new GenericMenu();
                foreach (var groupItem in BuildingSettings.GroupItems)
                {
                    groupMenu.AddItem(new GUIContent(groupItem.Name), false, () => UnityBuilding.Build(groupItem));
                }
                groupMenu.ShowAsContext();
            }

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
            public static readonly GUIStyle commandButtonStyle;
            public static readonly GUIStyle popupStyle;
            public static readonly GUIStyle labelStyle;
            public static readonly GUIStyle toggleStyle;

            static ToolbarStyles()
            {
                commandButtonStyle = "AppCommand";

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

            public override void OnActivated() => _action?.Invoke();
        }
    }
}