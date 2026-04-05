#if CAMERAMANAGER_DOTWEEN
using UnityEngine;
using DG.Tweening;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Optional bridge that replaces the instant camera-property application in
    /// <see cref="CameraManager"/> with smooth DOTween-driven transitions.
    /// Enable define <c>CAMERAMANAGER_DOTWEEN</c> in Player Settings › Scripting Define Symbols.
    /// Requires <b>DOTween Pro</b>.
    /// <para>
    /// When a profile change event fires the camera has already been snapped to the new values by
    /// <see cref="CameraManager"/>. This bridge caches the previous FOV / orthographic size and
    /// immediately snaps the physical <see cref="Camera"/> back before tweening it to the target.
    /// </para>
    /// </summary>
    [AddComponentMenu("CameraManager/DOTween Bridge")]
    [DisallowMultipleComponent]
    public class DotweenCameraBridge : MonoBehaviour
    {
        [Header("Transition")]
        [Tooltip("Duration of the camera property tween on each profile change.")]
        [SerializeField] private float transitionDuration = 0.4f;

        [Tooltip("DOTween ease applied to FOV and orthographic size transitions.")]
        [SerializeField] private Ease transitionEase = Ease.InOutSine;

        // -------------------------------------------------------------------------

        private CameraManager _cam;
        private float         _prevFov          = 60f;
        private float         _prevOrthoSize    = 5f;
        private bool          _prevOrthographic = false;

        private void Awake()
        {
            _cam = GetComponent<CameraManager>() ?? FindFirstObjectByType<CameraManager>();
            if (_cam == null)
            {
                Debug.LogWarning("[CameraManager/DotweenCameraBridge] CameraManager not found.");
                return;
            }

            // Snapshot the current camera so the first transition has a valid start value.
            var activeCam = GetActiveCamera();
            if (activeCam != null)
            {
                _prevFov          = activeCam.fieldOfView;
                _prevOrthoSize    = activeCam.orthographicSize;
                _prevOrthographic = activeCam.orthographic;
            }
        }

        private void OnEnable()
        {
            if (_cam != null) _cam.OnCameraChanged += OnCameraChanged;
        }

        private void OnDisable()
        {
            if (_cam != null) _cam.OnCameraChanged -= OnCameraChanged;
        }

        // -------------------------------------------------------------------------

        private void OnCameraChanged(string previousId, string newId)
        {
            if (!_cam.Cameras.TryGetValue(newId, out var newProfile)) return;

            var cam = GetCameraByTag(newProfile.cameraTag);
            if (cam == null) return;

            float toFov   = newProfile.fov;
            float toOrtho = newProfile.orthographicSize;

            // Snap back to previous, then tween to the new target values.
            cam.fieldOfView      = _prevFov;
            cam.orthographicSize = _prevOrthoSize;

            DOTween.Kill(cam);

            if (!newProfile.orthographic)
                cam.DOFieldOfView(toFov, transitionDuration).SetEase(transitionEase);
            else
                cam.DOOrthoSize(toOrtho, transitionDuration).SetEase(transitionEase);

            // Cache values for the next transition.
            _prevFov          = toFov;
            _prevOrthoSize    = toOrtho;
            _prevOrthographic = newProfile.orthographic;
        }

        // -------------------------------------------------------------------------

        private Camera GetActiveCamera()
        {
            var profile = _cam.CurrentProfile;
            if (profile == null) return Camera.main;
            return GetCameraByTag(profile.cameraTag) ?? Camera.main;
        }

        private static Camera GetCameraByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;
            var go = GameObject.FindGameObjectWithTag(tag);
            return go != null ? go.GetComponent<Camera>() : null;
        }
    }
}
#else
namespace CameraManager.Runtime
{
    /// <summary>No-op stub — enable define <c>CAMERAMANAGER_DOTWEEN</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("CameraManager/DOTween Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DotweenCameraBridge : UnityEngine.MonoBehaviour { }
}
#endif
