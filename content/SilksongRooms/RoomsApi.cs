using System;
using System.Collections.Generic;
using System.IO;

namespace SilksongRooms._1;

// Public static API that other mods can call to register their own Rooms folders or JSON files
public static class RoomsApi
{
    // Fired whenever rooms are added via the API
    public static event Action<IReadOnlyList<LoadedRoom>>? RoomsAdded;

    // In case the plugin hasn't started yet, we keep a queue to process on Awake
    private static readonly List<Action<SilksongRooms__1Plugin>> _pending = new();

    internal static void Bind(SilksongRooms__1Plugin plugin)
    {
        // drain pending requests safely
        if (_pending.Count > 0)
        {
            foreach (var act in _pending.ToArray())
            {
                try { act(plugin); }
                catch (Exception) { /* swallow to avoid blocking subsequent actions */ }
            }
            _pending.Clear();
        }
    }

    // Register an entire folder containing *.room.json and bundles; bundle paths are resolved relative to this folder
    public static void RegisterRoomsFolder(string folder)
    {
        void Do(SilksongRooms__1Plugin p)
        {
            var added = p.RegisterRoomsFolder(folder);
            RoomsAdded?.Invoke(added);
        }

        if (SilksongRooms__1Plugin.InstanceOrNull is { } plugin) Do(plugin);
        else _pending.Add(Do);
    }

    // Register a single room.json file; bundle relative paths are resolved to the file's directory unless resolveRoot is provided
    public static void RegisterRoomJson(string jsonFilePath, string? resolveRoot = null)
    {
        void Do(SilksongRooms__1Plugin p)
        {
            var added = p.RegisterRoomJson(jsonFilePath, resolveRoot);
            if (added != null) RoomsAdded?.Invoke(new[] { added });
        }

        if (SilksongRooms__1Plugin.InstanceOrNull is { } plugin) Do(plugin);
        else _pending.Add(Do);
    }

    // Convenience helper to compute a default Rooms folder for a given BepInEx plugin name
    public static string GetDefaultRoomsFolder(string pluginName)
    {
        return Path.Combine(BepInEx.Paths.PluginPath, pluginName, "Rooms");
    }
}
