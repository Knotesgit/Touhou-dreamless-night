using UnityEditor;
using UnityEngine;

public class ClearAnimEvents : Editor
{
    [MenuItem("Tools/Animation/Clear Events On Selected Clip")]
    static void ClearEvents()
    {
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Clear Events", "��ѡ��һ�� AnimationClip ��Դ��", "OK");
            return;
        }
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        Debug.Log($"Cleared all events on clip: {clip.name}");
    }
}
