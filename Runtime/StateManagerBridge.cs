#if CAMERAMANAGER_STM
using System.Collections.Generic;
using UnityEngine;
using StateManager.Runtime;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Optional bridge between CameraManager and StateManager.
    /// Enable define <c>CAMERAMANAGER_STM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Maps each <see cref="AppState"/> to a camera profile id and calls
    /// <see cref="CameraManager.SetCamera(string)"/> whenever the state changes.
    /// </para>
    /// </summary>
    [AddComponentMenu("CameraManager/State Manager Bridge")]
    [DisallowMultipleComponent]
    public class StateManagerBridge : MonoBehaviour
    {
        [System.Serializable]
        public class StateCameraMapping
        {
            [Tooltip("Application state.")]
            public AppState state;

            [Tooltip("Camera profile id to set when this state becomes active.")]
            public string cameraId;
        }

        [SerializeField] private List<StateCameraMapping> stateMappings = new List<StateCameraMapping>();

        private CameraManager _cam;
        private StateManager.Runtime.StateManager _state;

        private void Awake()
        {
            _cam   = GetComponent<CameraManager>() ?? FindFirstObjectByType<CameraManager>();
            _state = GetComponent<StateManager.Runtime.StateManager>()
                     ?? FindFirstObjectByType<StateManager.Runtime.StateManager>();

            if (_cam   == null) Debug.LogWarning("[CameraManager/StateManagerBridge] CameraManager not found.");
            if (_state == null) Debug.LogWarning("[CameraManager/StateManagerBridge] StateManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null) _state.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (_state != null) _state.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_cam == null) return;
            foreach (var mapping in stateMappings)
            {
                if (mapping.state == next && !string.IsNullOrEmpty(mapping.cameraId))
                {
                    _cam.SetCamera(mapping.cameraId);
                    return;
                }
            }
        }
    }
}
#else
namespace CameraManager.Runtime
{
    /// <summary>No-op stub — enable define <c>CAMERAMANAGER_STM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("CameraManager/State Manager Bridge")]
    public class StateManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[CameraManager/StateManagerBridge] Bridge disabled — add CAMERAMANAGER_STM to Scripting Define Symbols.");
    }
}
#endif
