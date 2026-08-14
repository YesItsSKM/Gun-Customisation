using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Editor
{
    [CustomPropertyDrawer(typeof(WeaponStatModifier))]
    public sealed class WeaponStatModifierDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f +
                   EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            var firstLine = new Rect(position.x, position.y, position.width, lineHeight);
            var secondLine = new Rect(
                position.x,
                position.y + lineHeight + spacing,
                position.width,
                lineHeight);

            EditorGUI.PropertyField(
                firstLine,
                property.FindPropertyRelative("definition"),
                label);

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            Rect content = EditorGUI.IndentedRect(secondLine);
            EditorGUI.indentLevel = 0;

            const float gap = 4f;
            float operationWidth = content.width * 0.4f;
            float priorityWidth = content.width * 0.25f;
            float valueWidth = content.width - operationWidth - priorityWidth - gap * 2f;

            var operationRect = new Rect(
                content.x,
                content.y,
                operationWidth,
                content.height);
            var valueRect = new Rect(
                operationRect.xMax + gap,
                content.y,
                valueWidth,
                content.height);
            var priorityRect = new Rect(
                valueRect.xMax + gap,
                content.y,
                priorityWidth,
                content.height);

            EditorGUI.PropertyField(
                operationRect,
                property.FindPropertyRelative("operation"),
                GUIContent.none);
            EditorGUI.PropertyField(
                valueRect,
                property.FindPropertyRelative("value"),
                GUIContent.none);
            EditorGUI.PropertyField(
                priorityRect,
                property.FindPropertyRelative("priority"),
                new GUIContent("P"));

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }
}
