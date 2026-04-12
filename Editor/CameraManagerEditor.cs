using UnityEditor;
using UnityEngine;

namespace CameraManager.Editor
{
    [CustomEditor(typeof(CameraManager.Runtime.CameraManager))]
    public class CameraManagerEditor : UnityEditor.Editor
    {
        private string _previewId = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Open JSON Editor")) CameraJsonEditorWindow.ShowWindow();

            var manager = (CameraManager.Runtime.CameraManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use live controls.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Active Camera", manager.CurrentCameraId ?? "(none)");

            EditorGUILayout.Space();
            _previewId = EditorGUILayout.TextField("Camera Profile Id", _previewId);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Camera"))  manager.SetCamera(_previewId);
            if (GUILayout.Button("Push Camera")) manager.PushCamera(_previewId);
            if (GUILayout.Button("Pop Camera"))  manager.PopCamera();
            EditorGUILayout.EndHorizontal();

            if (manager.Cameras.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Profiles", EditorStyles.boldLabel);
                foreach (var kvp in manager.Cameras)
                {
                    bool active = kvp.Key == manager.CurrentCameraId;
                    EditorGUILayout.LabelField($"  {(active ? "►" : "○")} {kvp.Key}", kvp.Value.displayName ?? kvp.Key);
                }
            }

            Repaint();
        }
    }
}
