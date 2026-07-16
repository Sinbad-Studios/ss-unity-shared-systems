# Sinbad Studios Shared Systems

A collection of shared networking systems, runtime services, utilities, and reusable tools for Unity projects.

- **Package name:** `com.sinbadstudios.shared-systems`
- **Version:** `1.0.0`
- **Unity:** `6000.2.8f1` (Unity 6)
- **Author:** Sinbad Studios

## Overview

This package bundles gameplay, UI, and networking building blocks that are reused across Sinbad Studios projects. Networking is built on [Photon Fusion](https://doc.photonengine.com/fusion/current/getting-started/fusion-intro) (Shared mode).

## Requirements

The scripts in this package reference the following. Make sure the corresponding packages/SDKs are installed in your project before importing:

- Unity 6 (`6000.2.8f1` or compatible)
- [Photon Fusion](https://doc.photonengine.com/fusion/current/getting-started/sdk-download) (`Fusion`, `NetworkCharacterController`)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) (`UnityEngine.InputSystem`)
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) (`TMPro`)

## Installation

You can also use **Window > Package Manager > + > Add package from git URL** in the Unity Editor.

Git URL: https://github.com/Sinbad-Studios/ss-unity-shared-systems.git

## Assembly Definitions

| Assembly | Platforms | Purpose |
| --- | --- | --- |
| `SinbadStudios.SharedSystems` | All | Runtime scripts (namespace `SinbadStudios.SharedSystems.Runtime`) |
| `SinbadStudios.SharedSystems.Editor` | Editor only | Editor tooling |

## Contents

### Common (`Runtime/Common`)

| Script | Description |
| --- | --- |
| `MonoSingleton<T>` | Generic `MonoBehaviour` singleton base. Persists across scenes via `DontDestroyOnLoad`, guards against duplicates, and exposes an overridable `Init()` hook. |
| `GameEventBus` | Lightweight, type-based publish/subscribe event bus. Subscribe, unsubscribe, and publish strongly-typed event objects through a global `Instance`. |
| `BasicPlayerController` | Simple 2D networked movement using WASD keys and legacy `Input` (Fusion `NetworkBehaviour`). |
| `Basic3DPlayerController` | 3D networked movement that drives a `Rigidbody` using the new Input System. |
| `NetworkSceneObjectTester` | Debug helper that logs spawn state and authority for networked scene objects. |
| `ScrollingUIBackground` | Scrolls a UI `Image` material's texture offset over time for a looping background effect. |
| `VersionDisplay` | Writes `Application.version` into a `TMP_Text` component, prefixing dev builds with `DEV`. |
| `WavyText` | Animates a `TextMeshProUGUI` with a per-character sine-wave motion. |

### Network (`Runtime/Network`)

| Script | Description |
| --- | --- |
| `NetworkManager` | Central Fusion session manager (a `MonoSingleton`). Handles lobby join, direct/auto session matchmaking (1v1 Shared mode), reconnection with retries, scene loading, and player join/leave. Communicates via `GameEventBus`. |
| `NetworkEvents` | Event data classes used with `GameEventBus` (e.g. `JoinLobbyEvent`, `JoinDirectSessionEvent`, `PlayerJoinedEvent`, `SessionReadyToStartEvent`, `NetworkStatusUpdateEvent`). |
| `PlayerNetworkControllerBase` | Extensible base for networked player state (`PlayerName`, `UserId`, health, dead/game-over flags), with overridable health-change and rematch behavior. Create the concrete `PlayerNetworkController` in the consuming project's `Assets` folder. |
| `Basic3DNetworkCharacterController` | 3D movement using Fusion's `NetworkCharacterController` and the new Input System. |
| `URLReader` | Reads session parameters (user id, session id, token, client id) from URL query params in WebGL builds via a JS interop call. |
| `SessionViewer/FusionSessionViewer` | Joins a lobby and renders the live session list into a UI list, creating/updating/removing entries as sessions change. |
| `SessionViewer/SessionListItem` | UI entry that displays a session's name and player count. |

### Editor (`Editor`)

| Script | Description |
| --- | --- |
| `AlwaysStartFromFirstSceneInEditor` | Forces Play Mode to always start from the first enabled scene in Build Settings. Toggle it under **Tools > Always Start From First Scene**. |

## Usage

### GameEventBus

```csharp
using SinbadStudios.SharedSystems.Runtime;

// Subscribe
GameEventBus.Instance.Subscribe<JoinLobbyEvent>(OnJoinLobby);

// Publish
GameEventBus.Instance.Publish(new JoinLobbyEvent());

// Unsubscribe (e.g. in OnDestroy)
GameEventBus.Instance.Unsubscribe<JoinLobbyEvent>(OnJoinLobby);
```

### MonoSingleton

```csharp
using SinbadStudios.SharedSystems.Runtime;

public class AudioManager : MonoSingleton<AudioManager>
{
    // Override Init instead of Awake.
    protected override void Init()
    {
        // one-time setup
    }
}

// Access anywhere
AudioManager.Instance.DoSomething();
```

### Editable player network controller

Create the concrete controller in the consuming project's `Assets` folder so it remains editable:

```csharp
using SinbadStudios.SharedSystems.Runtime;

public class PlayerNetworkController : PlayerNetworkControllerBase
{
    public override void Spawned()
    {
        base.Spawned();

        // Add project-specific initialization here.
    }

    protected override void OnHealthChanged()
    {
        base.OnHealthChanged();

        // Update project-specific UI or effects here.
    }
}
```

Attach the concrete `PlayerNetworkController` component to the player network prefab. Call `base.Spawned()` so the shared state initialization and `PlayerNetworkObjectSpawnedEvent` publication still occur.

### Networking flow

1. Add a `NetworkManager` to your bootstrap scene and assign a `NetworkRunner` prefab (with a component implementing `INetworkSceneManager`) in the inspector.
2. Publish `JoinLobbyEvent` for auto-matchmaking, or `JoinDirectSessionEvent { SessionName = "..." }` to join/create a specific session.
3. Listen for status and lifecycle events (`NetworkStatusUpdateEvent`, `SessionFoundEvent`, `SessionReadyToStartEvent`, `PlayerJoinedEvent`, `PlayerLeftEvent`) via `GameEventBus`.
4. Publish `LeaveSessionEvent` to shut the session down cleanly.

## License

Proprietary — © Sinbad Studios. All rights reserved.
