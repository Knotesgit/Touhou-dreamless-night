using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float spacing = 2f;

        void Draw(string field)
        {
            var prop = property.FindPropertyRelative(field);
            float height = EditorGUI.GetPropertyHeight(prop, true);
            Rect r = new Rect(position.x, y, position.width, height);
            EditorGUI.PropertyField(r, prop, true);
            y += height + spacing;
        }
        Draw("BGM");
        Draw("BGMName");
        Draw("speakerName");
        Draw("nameColor");
        Draw("content");
        Draw("portraitSprite");
        Draw("isLeftSide");
        Draw("lineTimeInterval");
        Draw("cannotSkip");

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float total = 0f;
        float spacing = 2f;

        foreach (string field in new[]
        {
            "BGM",
            "BGMName",
            "speakerName",
            "nameColor",
            "content",
            "portraitSprite",
            "isLeftSide",
            "lineTimeInterval",
            "cannotSkip"
        })
        {
            var prop = property.FindPropertyRelative(field);
            total += EditorGUI.GetPropertyHeight(prop, true) + spacing;
        }

        return total;
    }
}


