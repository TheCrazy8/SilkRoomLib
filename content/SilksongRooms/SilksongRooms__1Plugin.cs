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

    private string RoomsFolder => Path.Combine(Paths.PluginPath, Info.Metadata.Name, "Rooms");

    private RoomLoader? _loader;
    private IRoomIntegrator? _integrator;

    private void Awake()
    {
        // Ensure rooms folder exists
        Directory.CreateDirectory(RoomsFolder);
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded! Rooms folder: {RoomsFolder}");

        // Prepare loader and integrator
        _loader = new RoomLoader(Logger);
        _integrator = new SilksongRoomIntegrator(Logger);

        // Load all rooms immediately
        SafeLoadAllRooms();

        // Hook scene load to allow integration points when scenes change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
}
