#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using CameraManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace CameraManager.Editor
{
    // ────────────────────────────────────────────────────────────────────────────
    // Camera JSON Editor Window
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Editor window for creating and editing <c>cameras.json</c> in StreamingAssets.
    /// Open via <b>JSON Editors → Camera Manager</b> or via the Manager Inspector button.
    /// </summary>
    public class CameraJsonEditorWindow : EditorWindow
    {
        private const string JsonFileName = "cameras.json";

        private CameraProfileEditorBridge _bridge;
        private UnityEditor.Editor        _bridgeEditor;
        private Vector2                   _scroll;
        private string                    _status;
        private bool                      _statusError;

        [MenuItem("JSON Editors/Camera Manager")]
        public static void ShowWindow() =>
            GetWindow<CameraJsonEditorWindow>("Camera JSON");

        private void OnEnable()
        {
            _bridge = CreateInstance<CameraProfileEditorBridge>();
            Load();
        }

        private void OnDisable()
        {
            if (_bridgeEditor != null) DestroyImmediate(_bridgeEditor);
            if (_bridge      != null) DestroyImmediate(_bridge);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);

            if (_bridge == null) return;
            if (_bridgeEditor == null)
                _bridgeEditor = UnityEditor.Editor.CreateEditor(_bridge);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _bridgeEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(
                Path.Combine("StreamingAssets", JsonFileName),
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
            try
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, JsonUtility.ToJson(new CameraManifestEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }

                var w = JsonUtility.FromJson<CameraManifestEditorWrapper>(File.ReadAllText(path));
                _bridge.cameras = new List<CameraProfile>(
                    w.cameras ?? Array.Empty<CameraProfile>());

                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }

                _status     = $"Loaded {_bridge.cameras.Count} camera profiles.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Load error: {e.Message}";
                _statusError = true;
            }
        }

        private void Save()
        {
            try
            {
                var w    = new CameraManifestEditorWrapper { cameras = _bridge.cameras.ToArray() };
                var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status     = $"Saved {_bridge.cameras.Count} profiles to {JsonFileName}.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Save error: {e.Message}";
                _statusError = true;
            }
        }
    }

    // ── ScriptableObject bridge ──────────────────────────────────────────────
    internal class CameraProfileEditorBridge : ScriptableObject
    {
        public List<CameraProfile> cameras = new List<CameraProfile>();
    }

    // ── Local wrapper mirrors the internal CameraManifestJson ────────────────
    [Serializable]
    internal class CameraManifestEditorWrapper
    {
        public CameraProfile[] cameras = Array.Empty<CameraProfile>();
    }
}
#endif
