using System.Linq;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Utils;
using UnityEditor;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor;
using UnityEngine;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Provider
{
    [CustomPropertyDrawer(typeof(BuildingData))]
    public sealed class BuildingDataDrawer : ExtendedDrawer
    {
        private const float TargetLabelWidth = 50f;
        private const float TargetPopupWidth = 200f;
        private const float TypePopupWidth = 150f;
        private const float ExtraLabelWidth = 50f;
        private const float ExtraPopupWidth = 150f;

        private const float Margin = 5f;
        private const float LabelYOffset = -2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetProperty = property.FindPropertyRelative("buildTarget");
            var typeProperty = property.FindPropertyRelative("buildType");
            var extraProperty = property.FindPropertyRelative("buildExtras");

            var left = position.x;
            EditorGUI.LabelField(new Rect(left, position.y + LabelYOffset, TargetLabelWidth, position.height), "Target:");
            left += TargetLabelWidth;
            targetProperty.enumValueIndex = (int)(BuildTarget)EditorGUI.EnumPopup(new Rect(left, position.y, TargetPopupWidth, position.height),
                null, (BuildTarget)targetProperty.enumValueIndex, v => UnityHelper.IsBuildTargetSupported((BuildTarget)v));
            left += TargetPopupWidth + Margin;
            typeProperty.intValue = EditorGUI.Popup(new Rect(left, position.y, TypePopupWidth, position.height),
                typeProperty.intValue, BuildingSettings.Singleton.TypeItems.Select(x => x.Name).ToArray());
            left += TypePopupWidth + Margin;
            EditorGUI.LabelField(new Rect(left, position.y + LabelYOffset, ExtraLabelWidth, position.height), "Extras:");
            left += ExtraLabelWidth;
            extraProperty.enumValueIndex = (int)(BuildExtras)EditorGUI.EnumPopup(new Rect(left, position.y, ExtraPopupWidth, position.height),
                (BuildExtras)extraProperty.enumValueIndex);
            left += ExtraPopupWidth + Margin;
        }
    }
}