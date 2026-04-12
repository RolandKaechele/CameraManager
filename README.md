# CameraManager

Centralized camera profile manager for Unity.  
Manages a stack of named camera profiles, activating the correct camera GameObjects and configuring `Camera` component settings accordingly.  
Supports JSON-driven profiles for modding and full optional integration with CutsceneManager, StateManager, MapLoaderFramework, and EventManager.


## Features

- **SetCamera / PushCamera / PopCamera** — stack-based camera management for modal camera contexts
- **Profile application** — configures `fieldOfView`, `orthographic`, and `orthographicSize` on the target `Camera` (located by tag)
- **JSON / Modding** — define profiles in `StreamingAssets/cameras/`; merged by `id` on top of Inspector data
- **Events** — `OnCameraChanged(previousId, newId)`, `OnCameraPushed(id)`, `OnCameraPopped(id)` for reactive integration
- **CutsceneManager integration** — auto-push cutscene camera on sequence start; auto-pop on complete/skip (activated via `CAMERAMANAGER_CSM`)
- **StateManager integration** — auto-switch camera profile on `AppState` change (activated via `CAMERAMANAGER_STM`)
- **MapLoaderFramework integration** — reset camera to default on chapter change (activated via `CAMERAMANAGER_MLF`)
- **EventManager integration** — broadcast `camera.changed/pushed/popped` events (activated via `CAMERAMANAGER_EM` or `EVENTMANAGER_CAM`)
- **Custom Inspector** — live camera controls, active profile display, and registered profile list in Play Mode
- **DOTween Pro integration** — `DOTween.To` smoothly tweens `fieldOfView` and `orthographicSize` between profiles; DOTween Pro Visual Path editor can drive cinematic camera paths (activated via `CAMERAMANAGER_DOTWEEN`)
- **Odin Inspector integration** — `SerializedMonoBehaviour` base for full Inspector serialization of complex types; runtime-display fields marked `[ReadOnly]` in Play Mode (activated via `ODIN_INSPECTOR`)


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/CameraManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/CameraManager.git Assets/CameraManager
```

### Option C — npm / postinstall

```bash
cd Assets/CameraManager
npm install
```


## Scene Setup

1. Create a persistent manager GameObject (or reuse your existing manager object).
2. Attach `CameraManager`.
3. Set `initialCameraId` to the default profile (e.g. `"gameplay"`).
4. Add camera profile definitions in the Inspector or via `cameras.json`.
5. Ensure each camera profile's `cameraTag` matches a tagged GameObject in your scene(s).
6. Attach any bridge components (see Bridge Components below).


## Quick Start

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `cameras` | *(empty)* | Built-in camera profiles |
| `initialCameraId` | `"gameplay"` | Profile activated on Awake |
| `loadFromJson` | `false` | Merge profiles from `cameras/` |
| `jsonPath` | `"cameras/"` | Folder relative to `StreamingAssets/` containing `.json` files to merge. Falls back to single-file mode if the value points to an existing file. |
| `maxStackDepth` | `8` | Maximum camera stack depth |
| `verboseLogging` | `false` | Log all transitions to Console |

### CameraProfile fields

| Field | Description |
| ----- | ----------- |
| `id` | Unique id, e.g. `"gameplay"`, `"cutscene"`, `"dialogue"` |
| `displayName` | Human-readable label |
| `cameraTag` | Tag of the camera GameObject (e.g. `"MainCamera"`, `"CutsceneCam"`) |
| `fov` | Field of view in degrees (perspective cameras) |
| `orthographicSize` | Orthographic size (orthographic cameras) |
| `orthographic` | Use orthographic projection |
| `priority` | Informational — higher values are preferred when multiple cameras exist |
| `category` | Tag, e.g. `"gameplay"`, `"cinematic"` |

### Code usage

```csharp
var cam = FindFirstObjectByType<CameraManager.Runtime.CameraManager>();

cam.SetCamera("gameplay");
cam.PushCamera("cutscene");  // overlay cutscene camera
cam.PopCamera();             // return to gameplay camera

bool exists = cam.HasCamera("dialogue");

// Subscribe to events
cam.OnCameraChanged += (prev, next) => Debug.Log($"Camera: {prev} → {next}");
cam.OnCameraPushed  += id => Debug.Log($"Pushed: {id}");
cam.OnCameraPopped  += id => Debug.Log($"Popped: {id}");
```


## Bridge Components

| Component | Define | Effect |
| --------- | ------ | ------ |
| `StateManagerBridge` | `CAMERAMANAGER_STM` | Set camera profile mapped to `AppState` |
| `CutsceneManagerBridge` | `CAMERAMANAGER_CSM` | Push `"cutscene"` on sequence start; pop on complete/skip |
| `MapLoaderBridge` | `CAMERAMANAGER_MLF` | Reset to default camera on chapter change |
| `EventManagerBridge` | `CAMERAMANAGER_EM` | Fire `camera.changed/pushed/popped` via EventManager |

EventManager can also re-broadcast CameraManager events using `CameraEventBridge` (define: `EVENTMANAGER_CAM`).


## JSON / Modding

Place one or more `.json` files in `StreamingAssets/cameras/` (path is configurable).
All `*.json` files in the folder are loaded and merged by `id` at startup.

**Example:** `StreamingAssets/cameras/main.json`

```json
{
  "cameras": [
    {
      "id": "shop",
      "displayName": "Shop Camera",
      "cameraTag": "MainCamera",
      "fov": 50,
      "orthographic": false,
      "priority": 10,
      "category": "gameplay"
    }
  ]
}
```

JSON entries are **merged by id** — mods can add new profiles or override Inspector definitions without reimporting.


## Optional Integrations

| Define | Integration |
| ------ | ----------- |
| `CAMERAMANAGER_STM` | CameraManager ←→ StateManager |
| `CAMERAMANAGER_CSM` | CameraManager ←→ CutsceneManager |
| `CAMERAMANAGER_MLF` | CameraManager ←→ MapLoaderFramework |
| `CAMERAMANAGER_EM` | CameraManager → EventManager (fire events) |
| `EVENTMANAGER_CAM` | EventManager ← CameraManager (re-broadcast) |
| `PHYSICSMANAGER_CAM` | PhysicsManager → CameraManager (`Shake()` on significant impacts) |
| `ODIN_INSPECTOR` | CameraManager ↔→ Odin Inspector (`SerializedMonoBehaviour` + `[ReadOnly]`) |


## Editor Tools

Open via **JSON Editors → Camera Manager** in the Unity menu bar, or via the **Open JSON Editor** button in the CameraManager Inspector.

| Action | Result |
| ------ | ------ |
| **Load** | Reads all `*.json` from `StreamingAssets/cameras/`; creates the folder if missing |
| **Edit** | Add / remove / reorder entries using the Inspector list |
| **Save** | Writes to `StreamingAssets/cameras/cameras.json` and calls `AssetDatabase.Refresh()` |

With **ODIN_INSPECTOR** active, the list uses Odin's enhanced drawer (drag-to-sort, collapsible entries).
