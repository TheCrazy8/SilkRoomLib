using System;
using System.Collections.Generic;
using System.IO;
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
        if (!Directory.Exists(roomsFolder))
        {
            _log.LogWarning($"Rooms folder does not exist: {roomsFolder}");
            return;
        }

        foreach (var jsonPath in Directory.EnumerateFiles(roomsFolder, "*.room.json", SearchOption.AllDirectories))
        {
            try
            {
                var json = File.ReadAllText(jsonPath);
                var def = JsonUtility.FromJson<RoomDefinition>(json);
                if (def == null || string.IsNullOrWhiteSpace(def.id))
                {
                    _log.LogWarning($"Skipping invalid room: {jsonPath}");
                    continue;
                }

                var loaded = new LoadedRoom { Definition = def, SourceJsonPath = jsonPath };

                // Resolve bundle path relative to Rooms folder
                if (!string.IsNullOrWhiteSpace(def.bundle))
                {
                    var bundlePath = Path.Combine(roomsFolder, def.bundle.Replace('/', Path.DirectorySeparatorChar));
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

                LoadedRooms.Add(loaded);
                _log.LogDebug($"Loaded definition for room '{def.id}'.");
            }
            catch (Exception ex)
            {
                _log.LogError($"Error loading room JSON '{jsonPath}': {ex}");
            }
        }
    }
}
