using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Editor
{
    [CustomPropertyDrawer(typeof(WeaponStatValue))]
    public sealed class WeaponStatValueDrawer : PropertyDrawer
    {
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect content = EditorGUI.PrefixLabel(position, label);
            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const float spacing = 4f;
            float definitionWidth = content.width * 0.62f;
            var definitionRect = new Rect(
                content.x,
                content.y,
                definitionWidth - spacing,
                content.height);
            var valueRect = new Rect(
                content.x + definitionWidth,
                content.y,
                content.width - definitionWidth,
                content.height);

            EditorGUI.PropertyField(
                definitionRect,
                property.FindPropertyRelative("definition"),
                GUIContent.none);
            EditorGUI.PropertyField(
                valueRect,
                property.FindPropertyRelative("value"),
                GUIContent.none);

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }
}
