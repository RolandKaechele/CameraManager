using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraManager.Runtime
{
    /// <summary>
    /// Defines a single camera configuration entry managed by <see cref="CameraManager"/>.
    /// </summary>
    [Serializable]
    public class CameraProfile
    {
        [Tooltip("Unique identifier for this camera profile.")]
        public string id;

        [Tooltip("Human-readable display name.")]
        public string displayName;

        [Tooltip("GameObject tag of the camera to activate (e.g. 'MainCamera', 'CutsceneCam').")]
        public string cameraTag = "MainCamera";

        [Tooltip("Field of view in degrees (for perspective cameras).")]
        [Range(10f, 170f)]
        public float fov = 60f;

        [Tooltip("Orthographic size (for orthographic cameras).")]
        public float orthographicSize = 5f;

        [Tooltip("Use orthographic projection.")]
        public bool orthographic;

        [Tooltip("Priority — higher values are preferred when multiple cameras are on the stack.")]
        public int priority;

        [Tooltip("Category tag, e.g. 'gameplay', 'cutscene', 'dialogue'.")]
        public string category;
    }

    /// <summary>
    /// JSON root wrapper used when loading camera profiles from StreamingAssets.
    /// </summary>
    [Serializable]
    internal class CameraManifestJson
    {
        public List<CameraProfile> cameras = new List<CameraProfile>();
    }
}
