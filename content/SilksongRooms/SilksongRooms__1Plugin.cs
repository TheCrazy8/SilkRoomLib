using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongRooms._1;

// TODO - adjust the plugin guid as needed
[BepInAutoPlugin(id: "io.github.silksongrooms__1")] 
public partial class SilksongRooms__1Plugin : BaseUnityPlugin
{
    internal static ManualLogSource LogS => _log ??= Logger.CreateLogSource("SilksongRooms");
    private static ManualLogSource? _log;

    internal static SilksongRooms__1Plugin? InstanceOrNull { get; private set; }

    private string RoomsFolder => Path.Combine(Paths.PluginPath, Info.Metadata.Name, "Rooms");

    private RoomLoader? _loader;
    private IRoomIntegrator? _integrator;

    private void Awake()
    {
        InstanceOrNull = this;
        // Ensure rooms folder exists
        Directory.CreateDirectory(RoomsFolder);
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded! Rooms folder: {RoomsFolder}");

        // Prepare loader and integrator
        _loader = new RoomLoader(Logger);
        _integrator = new SilksongRoomIntegrator(Logger);

    // Bind the public API so other mods calling early can be processed
    RoomsApi.Bind(this);

        // Load all rooms immediately
        SafeLoadAllRooms();

        // Hook scene load to allow integration points when scenes change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        InstanceOrNull = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_loader == null || _integrator == null) return;
        try
        {
            _integrator.OnSceneLoaded(scene, mode, _loader.LoadedRooms);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Room integrator OnSceneLoaded failed: {ex}");
        }
    }

    private void SafeLoadAllRooms()
    {
        if (_loader == null || _integrator == null) return;
        try
        {
            _loader.LoadAll(RoomsFolder);
            _integrator.RegisterRooms(_loader.LoadedRooms);
            Logger.LogInfo($"Loaded {_loader.LoadedRooms.Count} room(s).");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load rooms: {ex}");
        }
    }

    // Called by RoomsApi: load additional rooms from an external folder and register them
    internal IReadOnlyList<LoadedRoom> RegisterRoomsFolder(string folder)
    {
        if (_loader == null || _integrator == null) return Array.Empty<LoadedRoom>();
        var added = _loader.AddFromFolder(folder);
        if (added.Count > 0)
        {
            _integrator.RegisterRooms(_loader.LoadedRooms);
            Logger.LogInfo($"Added {added.Count} room(s) from '{folder}'. Total: {_loader.LoadedRooms.Count}.");
        }
        else
        {
            Logger.LogInfo($"No rooms found in '{folder}'.");
        }
        return added;
    }

    // Called by RoomsApi: load a single room JSON
    internal LoadedRoom? RegisterRoomJson(string jsonFilePath, string? resolveRoot)
    {
        if (_loader == null || _integrator == null) return null;
        var added = _loader.AddFromJsonFile(jsonFilePath, resolveRoot);
        if (added != null)
        {
            _integrator.RegisterRooms(_loader.LoadedRooms);
            Logger.LogInfo($"Added room '{added.Definition.id}' from '{jsonFilePath}'. Total: {_loader.LoadedRooms.Count}.");
        }
        return added;
    }
}
