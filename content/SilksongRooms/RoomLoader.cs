using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

namespace SilksongRooms._1;

public class LoadedRoom
{
    public RoomDefinition Definition = new();
    public AssetBundle? Bundle;
    public GameObject? Prefab;
    public string SourceJsonPath = string.Empty;
}

public sealed class RoomLoader
{
    private readonly ManualLogSource _log;
    public List<LoadedRoom> LoadedRooms { get; } = new();

    public RoomLoader(ManualLogSource log) { _log = log; }

    public void LoadAll(string roomsFolder)
    {
        LoadedRooms.Clear();
        AddFromFolder(roomsFolder);
    }

    // Incrementally load rooms from a folder, returning the list of newly added/updated rooms
    public List<LoadedRoom> AddFromFolder(string roomsFolder)
    {
        var added = new List<LoadedRoom>();
        if (!Directory.Exists(roomsFolder))
        {
            _log.LogWarning($"Rooms folder does not exist: {roomsFolder}");
            return added;
        }

        foreach (var jsonPath in Directory.EnumerateFiles(roomsFolder, "*.room.json", SearchOption.AllDirectories))
        {
            var loaded = TryLoadFromJsonFile(jsonPath, roomsFolder);
            if (loaded == null) continue;

            // de-duplicate by id (latest wins)
            var existingIndex = LoadedRooms.FindIndex(r => string.Equals(r.Definition.id, loaded.Definition.id, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                _log.LogWarning($"Room id '{loaded.Definition.id}' already loaded; replacing with definition from '{jsonPath}'.");
                LoadedRooms[existingIndex] = loaded;
            }
            else
            {
                LoadedRooms.Add(loaded);
            }
            added.Add(loaded);
        }

        return added;
    }

    // Load a single room JSON file. resolveRoot controls how to resolve bundle relative paths.
    public LoadedRoom? AddFromJsonFile(string jsonPath, string? resolveRoot = null)
    {
        var root = resolveRoot ?? Path.GetDirectoryName(jsonPath) ?? Environment.CurrentDirectory;
        var loaded = TryLoadFromJsonFile(jsonPath, root);
        if (loaded == null) return null;

        var existingIndex = LoadedRooms.FindIndex(r => string.Equals(r.Definition.id, loaded.Definition.id, StringComparison.Ordinal));
        if (existingIndex >= 0)
        {
            _log.LogWarning($"Room id '{loaded.Definition.id}' already loaded; replacing with definition from '{jsonPath}'.");
            LoadedRooms[existingIndex] = loaded;
        }
        else
        {
            LoadedRooms.Add(loaded);
        }
        return loaded;
    }

    public bool TryGetRoom(string id, out LoadedRoom? room)
    {
        room = LoadedRooms.FirstOrDefault(r => string.Equals(r.Definition.id, id, StringComparison.Ordinal));
        return room != null;
    }

    private LoadedRoom? TryLoadFromJsonFile(string jsonPath, string resolveRoot)
    {
        try
        {
            var json = File.ReadAllText(jsonPath);
            var def = JsonUtility.FromJson<RoomDefinition>(json);
            if (def == null || string.IsNullOrWhiteSpace(def.id))
            {
                _log.LogWarning($"Skipping invalid room: {jsonPath}");
                return null;
            }

            var loaded = new LoadedRoom { Definition = def, SourceJsonPath = jsonPath };

            // Resolve bundle path relative to provided root
            if (!string.IsNullOrWhiteSpace(def.bundle))
            {
                var bundlePath = Path.Combine(resolveRoot, def.bundle.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(bundlePath))
                {
                    loaded.Bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (loaded.Bundle == null)
                    {
                        _log.LogError($"Failed to load AssetBundle for room '{def.id}' at '{bundlePath}'.");
                    }
                    else
                    {
                        // Try to get the prefab by name if specified, otherwise first GameObject
                        if (!string.IsNullOrWhiteSpace(def.prefab))
                        {
                            loaded.Prefab = loaded.Bundle.LoadAsset<GameObject>(def.prefab);
                            if (loaded.Prefab == null)
                                _log.LogWarning($"Prefab '{def.prefab}' not found in bundle for room '{def.id}'.");
                        }

                        if (loaded.Prefab == null)
                        {
                            foreach (var assetName in loaded.Bundle.GetAllAssetNames())
                            {
                                var go = loaded.Bundle.LoadAsset<GameObject>(assetName);
                                if (go != null) { loaded.Prefab = go; break; }
                            }
                            if (loaded.Prefab == null)
                                _log.LogWarning($"No GameObject prefab found in bundle for room '{def.id}'.");
                        }
                    }
                }
                else
                {
                    _log.LogError($"Bundle path not found for room '{def.id}': {bundlePath}");
                }
            }

            _log.LogDebug($"Loaded definition for room '{def.id}'.");
            return loaded;
        }
        catch (Exception ex)
        {
            _log.LogError($"Error loading room JSON '{jsonPath}': {ex}");
            return null;
        }
    }
}
