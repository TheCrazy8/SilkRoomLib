using System;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using SilksongRooms._1;

namespace ArchitectRooms._1;

// Auto-plugin that watches Architect's export folder under LocalLow and registers rooms with SilksongRooms
[BepInAutoPlugin(id: "io.github.architectrooms__1")]
[BepInDependency("io.github.silksongrooms__1", BepInDependency.DependencyFlags.HardDependency)]
public partial class ArchitectRooms__1Plugin : BaseUnityPlugin
{
    private FileSystemWatcher? _watcher;
    private ConfigEntry<string>? _exportsPath;
    private ConfigEntry<bool>? _watchForChanges;

    private void Awake()
    {
        var defaultExports = ResolveArchitectExportsPath();
        _exportsPath = Config.Bind("Architect", "ExportsPath", defaultExports, "Path to Architect export folder containing *.room.json and bundles.");
        _watchForChanges = Config.Bind("Architect", "WatchForChanges", true, "Watch the exports folder for new/changed room JSON files.");

        Logger.LogInfo($"ArchitectRooms watching '{_exportsPath.Value}'.");
        EnsureDependencyPresent();
        RegisterFolder(_exportsPath.Value);
        SetupWatcher();
    }

    private void OnDestroy()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileChanged;
            _watcher.Changed -= OnFileChanged;
            _watcher.Renamed -= OnFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    private void EnsureDependencyPresent()
    {
        if (!Chainloader.PluginInfos.ContainsKey("io.github.silksongrooms__1"))
        {
            Logger.LogWarning("SilksongRooms plugin not loaded; ArchitectRooms will be inert.");
        }
    }

    private string ResolveArchitectExportsPath()
    {
        // Typical location: %UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Exports
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localLow = Path.GetFullPath(Path.Combine(local, "..", "LocalLow"));
        var exports = Path.Combine(localLow, "TeamCherry", "Silksong", "Architect", "Exports");
        return exports;
    }

    private void RegisterFolder(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            RoomsApi.RegisterRoomsFolder(folder);
            Logger.LogInfo($"Registered Architect export folder: {folder}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to register folder '{folder}': {ex}");
        }
    }

    private void SetupWatcher()
    {
        if (_watchForChanges == null || !_watchForChanges.Value) return;
        if (_exportsPath == null) return;

        try
        {
            _watcher = new FileSystemWatcher(_exportsPath.Value, "*.room.json")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Created += OnFileChanged;
            _watcher.Changed += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
            Logger.LogInfo("FileSystemWatcher armed for Architect exports.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to start watcher: {ex}");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Slight delay to avoid partial writes
        try
        {
            System.Threading.Thread.Sleep(50);
            RoomsApi.RegisterRoomJson(e.FullPath, Path.GetDirectoryName(e.FullPath));
            Logger.LogInfo($"Registered Architect room: {e.FullPath}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Watcher failed to register '{e.FullPath}': {ex.Message}");
        }
    }
}
