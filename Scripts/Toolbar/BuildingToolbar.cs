using System;
using System.Linq;
using PcSoft.Scripts.Provider;
using PcSoft.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace PcSoft.Scripts.Toolbar
{
    [InitializeOnLoad]
    public static class BuildingToolbar
    {
        static BuildingToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var buildingSettings = BuildingSettings.Singleton;
            var serializedObject = BuildingSettings.SerializedSingleton;
            serializedObject.Update();
            
            GUILayout.FlexibleSpace();

            GUILayout.Space(15f);

            GUILayout.Label("Target: ", ToolbarStyles.labelStyle);
            buildingSettings.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(null, buildingSettings.BuildTarget, 
                v => UnityHelper.IsBuildTargetSupported((BuildTarget)v), false, ToolbarStyles.popupStyle, ToolbarLayouts.popupLayout);
            if (GUILayout.Button("Reset", ToolbarStyles.commandButtonStyle))
            {
                buildingSettings.ResetBuildTarget();
            }
 
            GUILayout.Space(5f);

            GUILayout.Label("Type: ", ToolbarStyles.labelStyle);
            buildingSettings.BuildType = EditorGUILayout.Popup(buildingSettings.BuildType, buildingSettings.TypeItems.Select(x => x.Name).ToArray(),
                ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);

            GUILayout.Space(5f);
            
            GUILayout.Label("Extras: ", ToolbarStyles.labelStyle);
            buildingSettings.BuildExtras = (BuildExtras)EditorGUILayout.EnumFlagsField(buildingSettings.BuildExtras, ToolbarStyles.popupStyle, ToolbarLayouts.popupSmallLayout);
            
            GUILayout.Space(5f);

            buildingSettings.Clean = GUILayout.Toggle(buildingSettings.Clean, new GUIContent("Clean", "Clean complete build cache"), ToolbarStyles.toggleStyle);
            
            GUILayout.Space(5f);

            if (GUILayout.Button(new GUIContent("Build", "Build Project"), ToolbarStyles.commandButtonStyle))
            {
                AssetDatabase.SaveAssets();
                UnityBuilding.Build(false);
            }

            if (GUILayout.Button(new GUIContent("Run", "Build and Run project"), ToolbarStyles.commandButtonStyle))
            {
                AssetDatabase.SaveAssets();
                UnityBuilding.Build(true);
            }

            serializedObject.ApplyModifiedProperties();
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
                commandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal,
                    fixedWidth = 50f
                };

                popupStyle = new GUIStyle("Popup")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    stretchWidth = false
                };

                labelStyle = new GUIStyle("Label")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 20f
                };
                
                toggleStyle = new GUIStyle("Toggle")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    imagePosition = ImagePosition.TextOnly,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 20f
                };
            }
        }
        
        [Flags]
        public enum BuildExtras
        {
            None = 0x00,
            CodeCoverage = 0x01,
            UseProfiler = 0x02,
            StrictMode = 0x04,
            OpenFolder = 0x08,
        }
    }
}