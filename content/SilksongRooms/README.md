# Silksong Rooms Plugin Template

This template creates a BepInEx mod that makes it easy to add new rooms via JSON + AssetBundle drops.

- Drop room metadata files (`*.room.json`) and Unity AssetBundles into the `Rooms/` folder in your plugin directory.
- The plugin will scan and load them at startup, and spawn prefabs into a sandbox GameObject for safe preview.
- Hook up a custom integrator later to stitch rooms into the game's world and transitions.

## Folder layout

- `Rooms/`
  - `Bundles/` — put your Unity AssetBundles here
  - `*.room.json` — metadata pointing at a bundle and prefab name

## Room JSON schema (Unity JsonUtility)

```
{
  "id": "example_room",
  "bundle": "Bundles/example_room.bundle",
  "prefab": "ExampleRoomPrefab",
  "entryPoint": { "x": 0, "y": 0, "z": 0 },
  "connections": [ { "toRoomId": "some_room", "doorName": "ExitA" } ],
  "tags": ["optional", "labels"]
}
```

Notes:
- `bundle` is relative to the `Rooms/` folder.
- `prefab` is the name of the prefab inside the AssetBundle. If omitted, the first GameObject found is used.
- `entryPoint` is where the prefab is positioned when previewed via the sandbox.

## How it works

- `RoomLoader` scans `Rooms/**/*.room.json`, parses metadata, loads AssetBundles, and resolves a prefab.
- `SilksongRoomIntegrator` (default) logs what was loaded and spawns each prefab under a persistent `__RoomsSandbox` object, inactive by default.
- To properly stitch rooms into the game, replace `SilksongRoomIntegrator` with your own implementation of `IRoomIntegrator` and handle scene-specific placement and transitions.

## Use from another mod (register your own rooms)

If you want to ship rooms from a separate mod and have this plugin load them, add a dependency and call the public API.

1) Add a dependency attribute to your plugin so this one loads first:

```csharp
using BepInEx;
using SilksongRooms._1;

[BepInDependency("io.github.silksongrooms__1")]
[BepInPlugin("your.guid.here", "Your Mod", "1.0.0")]
public class YourMod : BaseUnityPlugin
{
  private void Awake()
  {
    // Register an entire Rooms folder
    var myRooms = System.IO.Path.Combine(BepInEx.Paths.PluginPath, Info.Metadata.Name, "Rooms");
    RoomsApi.RegisterRoomsFolder(myRooms);

    // Or register a single JSON file
    // RoomsApi.RegisterRoomJson(System.IO.Path.Combine(myRooms, "my_room.room.json"));
  }
}
```

Notes:
- `bundle` paths inside your JSON are resolved relative to the folder you register (or the JSON file directory if you use `RegisterRoomJson`).
- You can call the API any time; early calls are queued until this plugin finishes starting.
- You can subscribe to `RoomsApi.RoomsAdded` to react when rooms are loaded.

## Building & copying

- The build copies your plugin DLL, PDB, and the `Rooms/` contents to `BepInEx/plugins/<YourModName>/` if `SilksongPath.props` is configured.
- A distributable zip is placed under `bin/Publish/<YourModName>.zip` containing the DLL and `Rooms/`.

## Tips

- Use deterministic prefab names across bundles to keep your JSON stable.
- Keep rooms small and modular; prefer one room per JSON for clarity.
- Validate that prefab dependencies are included in the bundle.

Silksong Install Path is configured via `SilksongPath.props`. You can generate it with:

```
 dotnet new silksongpath --silksong-install-path "C:/.../Hollow Knight Silksong"
```