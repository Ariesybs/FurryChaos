#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIRaycastReceiver))]
[CanEditMultipleObjects]
public class UIRaycastReceiverEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif