/*
using UnityEditor;

namespace RR.Common.Editor
{
    [CustomEditor(typeof(Spring))]
    public class SpringEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var t = target as Spring;
            Undo.RecordObject(t, "Updated spring target state");
            Handles.TransformHandle(ref t.restPos, ref t.restRot);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
*/
