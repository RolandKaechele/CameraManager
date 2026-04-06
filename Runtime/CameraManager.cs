using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace CameraManager.Runtime
{
    /// <summary>
    /// Centralized camera profile manager for Unity.
    /// Manages a stack of <see cref="CameraProfile"/> entries, activating the appropriate
    /// camera GameObjects or configuring Camera components accordingly.
    /// Supports JSON-driven profiles for modding.
    /// </summary>
    [AddComponentMenu("Managers/Camera Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class CameraManager : SerializedMonoBehaviour
#else
    public class CameraManager : MonoBehaviour
#endif
    {
        // ──────────────────────────────────────────────────────────
        // Inspector fields
        // ──────────────────────────────────────────────────────────

        [Header("Profiles")]
        [Tooltip("Built-in camera profiles. JSON entries are merged on top by id.")]
        [SerializeField] private List<CameraProfile> cameras = new List<CameraProfile>();

        [Tooltip("Profile id to activate on Awake.")]
        [SerializeField] private string initialCameraId = "gameplay";

        [Header("JSON / Modding")]
        [Tooltip("Load additional profiles from StreamingAssets/<jsonPath>.")]
        [SerializeField] private bool loadFromJson;

        [Tooltip("Path relative to StreamingAssets/.")]
        [SerializeField] private string jsonPath = "cameras.json";

        [Header("Stack")]
        [Tooltip("Maximum camera stack depth.")]
        [SerializeField] private int maxStackDepth = 8;

        [Header("Debug")]
        [Tooltip("Log all camera transitions to the Console.")]
        [SerializeField] private bool verboseLogging;

        // ──────────────────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────────────────

        /// <summary>Fired when the active camera changes (e.g. via SetCamera). Parameters are (previousId, newId).</summary>
        public event Action<string, string> OnCameraChanged;

        /// <summary>Fired when a camera profile is pushed onto the stack. Parameter is the pushed profile id.</summary>
        public event Action<string> OnCameraPushed;

        /// <summary>Fired when a camera profile is popped from the stack. Parameter is the id that was popped.</summary>
        public event Action<string> OnCameraPopped;

        // ──────────────────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────────────────

        private readonly Dictionary<string, CameraProfile> _map = new Dictionary<string, CameraProfile>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<CameraProfile> _stack            = new Stack<CameraProfile>();

        // ──────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────

        /// <summary>Currently active camera profile, or <c>null</c> if the stack is empty.</summary>
        public CameraProfile CurrentProfile => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>Id of the currently active profile.</summary>
        public string CurrentCameraId => CurrentProfile?.id;

        /// <summary>All registered camera profiles, keyed by id.</summary>
        public IReadOnlyDictionary<string, CameraProfile> Cameras => _map;

        /// <summary>
        /// Replaces the entire camera stack with the given profile id.
        /// </summary>
        public void SetCamera(string id)
        {
            string previousId = CurrentCameraId;
            if (!TryGetProfile(id, out var profile)) return;
            _stack.Clear();
            _stack.Push(profile);
            Apply(profile);

            if (verboseLogging)
                Debug.Log($"[CameraManager] SetCamera: {previousId} → {id}");

            OnCameraChanged?.Invoke(previousId ?? string.Empty, id);
        }

        /// <summary>
        /// Pushes a camera profile onto the stack, overlaying the current camera.
        /// </summary>
        public void PushCamera(string id)
        {
            if (!TryGetProfile(id, out var profile)) return;
            if (_stack.Count >= maxStackDepth)
            {
                Debug.LogWarning($"[CameraManager] Max stack depth ({maxStackDepth}) reached. Cannot push '{id}'.");
                return;
            }
            _stack.Push(profile);
            Apply(profile);

            if (verboseLogging)
                Debug.Log($"[CameraManager] PushCamera: {id}");

            OnCameraPushed?.Invoke(id);
        }

        /// <summary>
        /// Pops the top camera off the stack and restores the previous one.
        /// </summary>
        public void PopCamera()
        {
            if (_stack.Count <= 1)
            {
                Debug.LogWarning("[CameraManager] Cannot pop — only one camera on stack.");
                return;
            }
            string popped = _stack.Pop().id;
            Apply(_stack.Peek());

            if (verboseLogging)
                Debug.Log($"[CameraManager] PopCamera: {popped} → {_stack.Peek().id}");

            OnCameraPopped?.Invoke(popped);
        }

        /// <summary>Returns true if a profile with the given id exists.</summary>
        public bool HasCamera(string id) => !string.IsNullOrEmpty(id) && _map.ContainsKey(id);

        /// <summary>
        /// Triggers a camera shake by starting a coroutine that briefly offsets the active camera.
        /// </summary>
        /// <param name="strength">Maximum positional offset in world units.</param>
        /// <param name="duration">Total shake duration in seconds.</param>
        public void Shake(float strength, float duration)
        {
            StartCoroutine(ShakeCoroutine(strength, duration));
        }

        private System.Collections.IEnumerator ShakeCoroutine(float strength, float duration)
        {
            GameObject camGo = CurrentProfile != null && !string.IsNullOrEmpty(CurrentProfile.cameraTag)
                ? GameObject.FindGameObjectWithTag(CurrentProfile.cameraTag)
                : Camera.main?.gameObject;

            if (camGo == null) yield break;

            Vector3 origin = camGo.transform.localPosition;
            float elapsed  = 0f;
            while (elapsed < duration)
            {
                float t = 1f - (elapsed / duration);
                camGo.transform.localPosition = origin + (Vector3)UnityEngine.Random.insideUnitCircle * strength * t;
                elapsed += Time.deltaTime;
                yield return null;
            }
            camGo.transform.localPosition = origin;
        }

        // ──────────────────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildMap();
            if (loadFromJson) LoadJson();

            if (!string.IsNullOrEmpty(initialCameraId) && _map.ContainsKey(initialCameraId))
                SetCamera(initialCameraId);
        }

        // ──────────────────────────────────────────────────────────
        // Internal helpers
        // ──────────────────────────────────────────────────────────

        private void Apply(CameraProfile profile)
        {
            // Find camera by tag and apply settings
            GameObject camGo = string.IsNullOrEmpty(profile.cameraTag)
                ? null
                : GameObject.FindGameObjectWithTag(profile.cameraTag);

            if (camGo == null && !string.IsNullOrEmpty(profile.cameraTag))
            {
                Debug.LogWarning($"[CameraManager] No GameObject found with tag '{profile.cameraTag}'.");
                return;
            }

            if (camGo == null) return;

            var cam = camGo.GetComponent<Camera>();
            if (cam == null) return;

            cam.fieldOfView     = profile.fov;
            cam.orthographic    = profile.orthographic;
            cam.orthographicSize = profile.orthographicSize;
        }

        private bool TryGetProfile(string id, out CameraProfile profile)
        {
            if (string.IsNullOrEmpty(id) || !_map.TryGetValue(id, out profile))
            {
                Debug.LogWarning($"[CameraManager] Camera profile not found: '{id}'");
                profile = null;
                return false;
            }
            return true;
        }

        private void BuildMap()
        {
            _map.Clear();
            foreach (var c in cameras)
            {
                if (string.IsNullOrEmpty(c.id)) continue;
                _map[c.id] = c;
            }
        }

        private void LoadJson()
        {
            string full = Path.Combine(Application.streamingAssetsPath, jsonPath);
            if (!File.Exists(full))
            {
                Debug.LogWarning($"[CameraManager] JSON not found: {full}");
                return;
            }
            try
            {
                string json = File.ReadAllText(full);
                var manifest = JsonUtility.FromJson<CameraManifestJson>(json);
                foreach (var c in manifest.cameras)
                {
                    if (string.IsNullOrEmpty(c.id)) continue;
                    _map[c.id] = c;
                }
                if (verboseLogging)
                    Debug.Log($"[CameraManager] Loaded {manifest.cameras.Count} profiles from {jsonPath}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CameraManager] Failed to parse {jsonPath}: {ex.Message}");
            }
        }
    }
}
