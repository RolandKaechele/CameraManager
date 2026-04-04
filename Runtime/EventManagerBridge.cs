#if CAMERAMANAGER_EM
using UnityEngine;
using EventManager.Runtime;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Optional bridge between CameraManager and EventManager.
    /// Enable define <c>CAMERAMANAGER_EM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named <see cref="GameEvent"/>s:
    /// <list type="bullet">
    ///   <item><c>"camera.changed"</c> — <see cref="GameEvent.stringValue"/> = new camera profile id</item>
    ///   <item><c>"camera.pushed"</c>  — <see cref="GameEvent.stringValue"/> = pushed profile id</item>
    ///   <item><c>"camera.popped"</c>  — <see cref="GameEvent.stringValue"/> = popped profile id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("CameraManager/Event Manager Bridge")]
    [DisallowMultipleComponent]
    public class EventManagerBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on camera set.")]
        [SerializeField] private string changedEventName = "camera.changed";

        [Tooltip("Event name fired when a camera is pushed.")]
        [SerializeField] private string pushedEventName = "camera.pushed";

        [Tooltip("Event name fired when a camera is popped.")]
        [SerializeField] private string poppedEventName = "camera.popped";

        private EventManager.Runtime.EventManager _events;
        private CameraManager _cam;

        private void Awake()
        {
            _events = GetComponent<EventManager.Runtime.EventManager>()
                      ?? FindFirstObjectByType<EventManager.Runtime.EventManager>();
            _cam    = GetComponent<CameraManager>() ?? FindFirstObjectByType<CameraManager>();

            if (_events == null) Debug.LogWarning("[CameraManager/EventManagerBridge] EventManager not found.");
            if (_cam    == null) Debug.LogWarning("[CameraManager/EventManagerBridge] CameraManager not found.");
        }

        private void OnEnable()
        {
            if (_cam != null)
            {
                _cam.OnCameraChanged += OnCameraChanged;
                _cam.OnCameraPushed  += OnCameraPushed;
                _cam.OnCameraPopped  += OnCameraPopped;
            }
        }

        private void OnDisable()
        {
            if (_cam != null)
            {
                _cam.OnCameraChanged -= OnCameraChanged;
                _cam.OnCameraPushed  -= OnCameraPushed;
                _cam.OnCameraPopped  -= OnCameraPopped;
            }
        }

        private void OnCameraChanged(string previousId, string newId) =>
            _events?.Fire(new GameEvent(changedEventName, newId));

        private void OnCameraPushed(string id) =>
            _events?.Fire(new GameEvent(pushedEventName, id));

        private void OnCameraPopped(string id) =>
            _events?.Fire(new GameEvent(poppedEventName, id));
    }
}
#else
namespace CameraManager.Runtime
{
    /// <summary>No-op stub — enable define <c>CAMERAMANAGER_EM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("CameraManager/Event Manager Bridge")]
    public class EventManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[CameraManager/EventManagerBridge] Bridge disabled — add CAMERAMANAGER_EM to Scripting Define Symbols.");
    }
}
#endif
