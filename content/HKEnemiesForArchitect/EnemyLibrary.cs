using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

namespace HKEnemiesForArchitect._1;

public sealed class EnemyEntry
{
    public string Id { get; init; } = string.Empty;          // normalized identifier (asset name sans extension)
    public string AssetName { get; init; } = string.Empty;   // full asset name inside bundle
    public string BundlePath { get; init; } = string.Empty;  // filesystem path of the bundle
    public GameObject? Prefab { get; init; }
}

public sealed class EnemyLibrary : IDisposable
{
    private readonly ManualLogSource _log;
    private readonly Dictionary<string, EnemyEntry> _enemies = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<AssetBundle> _bundles = new();

    public EnemyLibrary(ManualLogSource log) { _log = log; }

    public IReadOnlyDictionary<string, EnemyEntry> Enemies => _enemies;

    public void Dispose() => UnloadAll();

    public int LoadAll(string folder)
    {
        UnloadAll();
        if (!Directory.Exists(folder))
        {
            _log.LogWarning($"Enemies folder does not exist: {folder}");
            return 0;
        }

        var count = 0;
        foreach (var bundlePath in Directory.EnumerateFiles(folder, "*.bundle", SearchOption.AllDirectories))
        {
            count += LoadBundle(bundlePath);
        }
        _log.LogInfo($"EnemyLibrary loaded {count} prefab(s) from '{folder}'.");
        return count;
    }

    public bool TryGetEnemy(string id, out EnemyEntry? entry)
    {
        return _enemies.TryGetValue(id, out entry);
    }

    private int LoadBundle(string bundlePath)
    {
        try
        {
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                _log.LogWarning($"Failed to load bundle: {bundlePath}");
                return 0;
            }

            _bundles.Add(bundle);
            var loaded = 0;
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var prefab = bundle.LoadAsset<GameObject>(assetName);
                if (prefab == null) continue;

                var id = Path.GetFileNameWithoutExtension(assetName);
                _enemies[id] = new EnemyEntry
                {
                    Id = id,
                    AssetName = assetName,
                    BundlePath = bundlePath,
                    Prefab = prefab
                };
                loaded++;
            }

            _log.LogInfo($"Loaded {loaded} enemy prefab(s) from bundle '{Path.GetFileName(bundlePath)}'.");
            return loaded;
        }
        catch (Exception ex)
        {
            _log.LogError($"Error loading bundle '{bundlePath}': {ex}");
            return 0;
        }
    }

    private void UnloadAll()
    {
        _enemies.Clear();
        foreach (var b in _bundles)
        {
            try { b.Unload(unloadAllLoadedObjects: false); }
            catch (Exception) { /* best-effort */ }
        }
        _bundles.Clear();
    }
}
