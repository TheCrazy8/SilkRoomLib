using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HKEnemiesForArchitect._1;

namespace HKEnemiesForArchitect._1;

// Watches a user-supplied Hollow Knight enemy bundle folder and exposes prefabs for Architect usage.
[BepInAutoPlugin(id: "io.github.hkenemiesforarchitect__1")]
public partial class HKEnemiesForArchitect__1Plugin : BaseUnityPlugin
{
    private EnemyLibrary? _library;
    private FileSystemWatcher? _watcher;
    private ConfigEntry<string>? _enemiesPath;
    private ConfigEntry<bool>? _watchForChanges;

    private void Awake()
    {
        _library = new EnemyLibrary(Logger);
        HKEnemiesApi.Bind(_library);

        _enemiesPath = Config.Bind("Enemies", "Path", ResolveDefaultEnemiesPath(), "Folder containing Hollow Knight enemy AssetBundles (*.bundle).");
        _watchForChanges = Config.Bind("Enemies", "WatchForChanges", true, "Watch the folder for new/updated bundles and reload automatically.");

        Logger.LogInfo($"HKEnemiesForArchitect using '{_enemiesPath.Value}'.");
        LoadAll();
        SetupWatcher();
    }

    private void OnDestroy()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnBundleChanged;
            _watcher.Changed -= OnBundleChanged;
            _watcher.Renamed -= OnBundleChanged;
            _watcher.Dispose();
            _watcher = null;
        }
        _library?.Dispose();
    }

    private string ResolveDefaultEnemiesPath()
    {
        // Default: %UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Enemies
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localLow = Path.GetFullPath(Path.Combine(local, "..", "LocalLow"));
        return Path.Combine(localLow, "TeamCherry", "Silksong", "Architect", "Enemies");
    }

    private void LoadAll()
    {
        if (_library == null || _enemiesPath == null) return;
        try
        {
            Directory.CreateDirectory(_enemiesPath.Value);
            var count = _library.LoadAll(_enemiesPath.Value);
            HKEnemiesApi.NotifyChanged(_library.Enemies);
            WriteManifest(_enemiesPath.Value);
            Logger.LogInfo($"Loaded {count} enemy prefab(s) from '{_enemiesPath.Value}'.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load enemies: {ex}");
        }
    }

    private void SetupWatcher()
    {
        if (_watchForChanges == null || !_watchForChanges.Value) return;
        if (_enemiesPath == null) return;

        try
        {
            _watcher = new FileSystemWatcher(_enemiesPath.Value, "*.bundle")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Created += OnBundleChanged;
            _watcher.Changed += OnBundleChanged;
            _watcher.Renamed += OnBundleChanged;
            Logger.LogInfo("FileSystemWatcher armed for enemy bundles.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to start watcher: {ex}");
        }
    }

    private void OnBundleChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            Thread.Sleep(50); // allow file write to finish
            LoadAll();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Reload after change failed ({e.FullPath}): {ex.Message}");
        }
    }

    private void WriteManifest(string root)
    {
        if (_library == null) return;
        try
        {
            var manifestPath = Path.Combine(root, "hk_enemies.manifest.json");
            var payload = new
            {
                generatedAt = DateTime.UtcNow.ToString("o"),
                enemies = _library.Enemies.Values.Select(e => new
                {
                    id = e.Id,
                    asset = e.AssetName,
                    bundle = MakeRelative(root, e.BundlePath)
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(manifestPath, json);
            Logger.LogInfo($"Wrote enemy manifest with {_library.Enemies.Count} entries: {manifestPath}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to write enemy manifest: {ex.Message}");
        }
    }

    private string MakeRelative(string root, string path)
    {
        try
        {
            var uriRoot = new Uri(Path.GetFullPath(root) + Path.DirectorySeparatorChar);
            var uriPath = new Uri(Path.GetFullPath(path));
            return uriRoot.MakeRelativeUri(uriPath).ToString().Replace('/', Path.DirectorySeparatorChar);
        }
        catch
        {
            return path;
        }
    }
}
