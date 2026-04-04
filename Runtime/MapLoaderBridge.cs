#if CAMERAMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Optional bridge between CameraManager and MapLoaderFramework.
    /// Enable define <c>CAMERAMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Resets the camera to the initial profile when a chapter/map change occurs,
    /// preventing stale camera states from carrying over across scene loads.
    /// </para>
    /// </summary>
    [AddComponentMenu("CameraManager/Map Loader Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderBridge : MonoBehaviour
    {
        [Tooltip("Camera profile id to set on map load.")]
        [SerializeField] private string defaultCameraId = "gameplay";

        private CameraManager _cam;
        private MapLoaderManager _mlf;

        private void Awake()
        {
            _cam = GetComponent<CameraManager>() ?? FindFirstObjectByType<CameraManager>();
            _mlf = GetComponent<MapLoaderManager>() ?? FindFirstObjectByType<MapLoaderManager>();

            if (_cam == null) Debug.LogWarning("[CameraManager/MapLoaderBridge] CameraManager not found.");
            if (_mlf == null) Debug.LogWarning("[CameraManager/MapLoaderBridge] MapLoaderManager not found.");
        }

        private void OnEnable()
        {
            if (_mlf != null) _mlf.OnChapterChanged += OnChapterChanged;
        }

        private void OnDisable()
        {
            if (_mlf != null) _mlf.OnChapterChanged -= OnChapterChanged;
        }

        private void OnChapterChanged(int previous, int current)
        {
            if (_cam == null || string.IsNullOrEmpty(defaultCameraId)) return;
            _cam.SetCamera(defaultCameraId);
        }
    }
}
#else
namespace CameraManager.Runtime
{
    /// <summary>No-op stub — enable define <c>CAMERAMANAGER_MLF</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("CameraManager/Map Loader Bridge")]
    public class MapLoaderBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[CameraManager/MapLoaderBridge] Bridge disabled — add CAMERAMANAGER_MLF to Scripting Define Symbols.");
    }
}
#endif
