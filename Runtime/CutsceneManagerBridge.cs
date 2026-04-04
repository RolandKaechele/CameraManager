#if CAMERAMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Optional bridge between CameraManager and CutsceneManager.
    /// Enable define <c>CAMERAMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Pushes the <c>"cutscene"</c> camera when a sequence starts and pops it on completion or skip,
    /// automatically restoring the gameplay camera.
    /// </para>
    /// </summary>
    [AddComponentMenu("CameraManager/Cutscene Manager Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneManagerBridge : MonoBehaviour
    {
        [Tooltip("Camera profile id to push for cutscene sequences.")]
        [SerializeField] private string cutsceneCameraId = "cutscene";

        private CameraManager _cam;
        private CutsceneManager.Runtime.CutsceneManager _csm;

        private void Awake()
        {
            _cam = GetComponent<CameraManager>() ?? FindFirstObjectByType<CameraManager>();
            _csm = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                   ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_cam == null) Debug.LogWarning("[CameraManager/CutsceneManagerBridge] CameraManager not found.");
            if (_csm == null) Debug.LogWarning("[CameraManager/CutsceneManagerBridge] CutsceneManager not found.");
        }

        private void OnEnable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   += OnSequenceStarted;
                _csm.OnSequenceCompleted += OnSequenceEnded;
                _csm.OnSequenceSkipped   += OnSequenceEnded;
            }
        }

        private void OnDisable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   -= OnSequenceStarted;
                _csm.OnSequenceCompleted -= OnSequenceEnded;
                _csm.OnSequenceSkipped   -= OnSequenceEnded;
            }
        }

        private void OnSequenceStarted(string sequenceId) => _cam?.PushCamera(cutsceneCameraId);
        private void OnSequenceEnded(string sequenceId)   => _cam?.PopCamera();
    }
}
#else
namespace CameraManager.Runtime
{
    /// <summary>No-op stub — enable define <c>CAMERAMANAGER_CSM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("CameraManager/Cutscene Manager Bridge")]
    public class CutsceneManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[CameraManager/CutsceneManagerBridge] Bridge disabled — add CAMERAMANAGER_CSM to Scripting Define Symbols.");
    }
}
#endif
