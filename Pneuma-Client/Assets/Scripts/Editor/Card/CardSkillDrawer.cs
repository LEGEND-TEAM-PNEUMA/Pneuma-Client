using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CardSkill))]
public class CardSkillDrawer : PropertyDrawer
{
    private const float LineSpacing = 2f;
    private const int LineCount = 4;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "스킬 항목", true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90f;

            DrawProperty(ref position, property, "skillType", "스킬 타입");
            DrawProperty(ref position, property, "value", "수치");
            DrawProperty(ref position, property, "target", "대상");
            DrawProperty(ref position, property, "targetCount", "대상 수");

            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lineCount = property.isExpanded ? LineCount + 1 : 1;
        return lineCount * EditorGUIUtility.singleLineHeight + (lineCount - 1) * LineSpacing;
    }

    private static void DrawProperty(ref Rect position, SerializedProperty parent, string propertyName, string label)
    {
        position.y += EditorGUIUtility.singleLineHeight + LineSpacing;
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        EditorGUI.PropertyField(position, property, new GUIContent(label));
    }
}
