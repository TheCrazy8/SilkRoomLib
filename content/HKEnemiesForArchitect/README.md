# HKEnemiesForArchitect

A standalone BepInEx plugin that lets Architect use Hollow Knight enemy prefabs in Silksong. It watches a user-provided enemy bundle folder, loads every `*.bundle`, and exposes the prefabs via `HKEnemiesApi` for Architect or other mods.

> This ships **no copyrighted assets**. Supply your own Hollow Knight enemy AssetBundles in the configured folder.

## What it does
- Loads all `*.bundle` files from `%UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Enemies` by default.
- Extracts every `GameObject` prefab in those bundles and keeps a registry keyed by asset name.
- Emits `hk_enemies.manifest.json` in that folder so Architect can discover available enemy ids/bundle paths.
- Raises `HKEnemiesApi.EnemiesChanged` whenever the registry reloads and lets callers fetch prefabs by id.
- Optionally watches the folder for changes and reloads automatically.

## Configuration (BepInEx)
- `Enemies.Path` — folder containing enemy bundles (default: LocalLow path above).
- `Enemies.WatchForChanges` — enable/disable FileSystemWatcher live reload (default: true).

## Usage from another mod (e.g., Architect script)
```csharp
using HKEnemiesForArchitect._1;
using UnityEngine;

public class EnemySpawner
{
    public bool TrySpawn(string id, Vector3 position)
    {
        if (!HKEnemiesApi.TryGetEnemyPrefab(id, out var prefab) || prefab == null)
            return false;
        Object.Instantiate(prefab, position, Quaternion.identity);
        return true;
    }
}
```

Listen for updates:
```csharp
HKEnemiesApi.EnemiesChanged += enemies =>
{
    foreach (var kvp in enemies)
    {
        Logger.LogInfo($"Enemy available: {kvp.Key} from {kvp.Value.BundlePath}");
    }
};
```

## Expected folder layout
```
%UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Enemies/
  my_enemy.bundle
  another_enemy.bundle
```
Each bundle can contain multiple `GameObject` assets; their asset names (without extension) become ids (e.g., `Vengefly`, `HuskGuard`).

`hk_enemies.manifest.json` is regenerated on every load/reload and lists `{ id, asset, bundle }` entries (bundle is relative to the enemies folder) for Architect-side consumption.

## Building & copying
- Configure `SilksongPath.props` if you want the build to copy into your Silksong install automatically.
- `dotnet build content/HKEnemiesForArchitect/HKEnemiesForArchitect.1.csproj -c Release`
- Output copies to `BepInEx/plugins/HKEnemiesForArchitect/` when `SilksongPath.props` is present; zip at `bin/Publish/HKEnemiesForArchitect.zip`.

## Notes
- Bundles are kept loaded; reload calls unload previous bundles. If you change large bundles often, keep `WatchForChanges` on only when needed.
- Id collisions are last-write-wins; avoid duplicate asset names across bundles.
- If a bundle contains no `GameObject` assets, it is skipped.
