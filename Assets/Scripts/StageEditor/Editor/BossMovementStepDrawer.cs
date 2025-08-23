using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BossMovementStep))]
public class BossMovementStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float spacing = 2f;

        void Draw(string field)
        {
            SerializedProperty prop = property.FindPropertyRelative(field);
            float height = EditorGUI.GetPropertyHeight(prop, true);
            Rect r = new Rect(position.x, y, position.width, height);
            EditorGUI.PropertyField(r, prop, true);
            y += height + spacing;
        }

        Draw("moveType");
        Draw("targetPoint");
        Draw("duration");
        Draw("maxMoveRange");
        Draw("randomAreaSize");
        Draw("coolDown");
        Draw("startSpeed");
        Draw("targetSpeed");
        Draw("accelTime");

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float total = 0f;
        float spacing = 2f;

        foreach (string field in new[]
        {
            "moveType",
            "targetPoint",
            "duration",
            "maxMoveRange",
            "randomAreaSize",
            "coolDown",
            "startSpeed",
            "targetSpeed",
            "accelTime"
        })
        {
            var prop = property.FindPropertyRelative(field);
            total += EditorGUI.GetPropertyHeight(prop, true) + spacing;
        }

        return total;
    }
}



