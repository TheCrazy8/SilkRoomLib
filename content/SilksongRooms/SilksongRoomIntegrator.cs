using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongRooms._1;

// Minimal, safe integrator that logs and can optionally spawn rooms into a sandbox scene space.
public sealed class SilksongRoomIntegrator : IRoomIntegrator
{
    private readonly ManualLogSource _log;

    public SilksongRoomIntegrator(ManualLogSource log) { _log = log; }

    public void RegisterRooms(IReadOnlyList<LoadedRoom> rooms)
    {
        _log.LogInfo($"Registering {rooms.Count} loaded room(s).");
        foreach (var r in rooms)
        {
            _log.LogInfo($"Room: {r.Definition.id} | Bundle: {(r.Bundle ? r.Bundle.name : "(none)")} | Prefab: {(r.Prefab ? r.Prefab.name : "(none)")}");
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode, IReadOnlyList<LoadedRoom> rooms)
    {
        // This is a stub. Actual integration is game-specific and should place room prefabs into the
        // appropriate scene graph, connect transitions, etc. To help development, we optionally spawn
        // room prefabs as inactive objects under a sandbox parent so creators can validate bundles load.

        const string SandboxRootName = "__RoomsSandbox";
        var root = GameObject.Find(SandboxRootName) ?? new GameObject(SandboxRootName);
        Object.DontDestroyOnLoad(root);

        foreach (var r in rooms)
        {
            if (r.Prefab == null) continue;

            var existing = root.transform.Find(r.Definition.id);
            if (existing != null) continue; // already spawned once

            var instance = Object.Instantiate(r.Prefab, r.Definition.entryPoint, Quaternion.identity, root.transform);
            instance.name = r.Definition.id;
            instance.SetActive(false); // leave inactive by default to avoid interfering with gameplay
            _log.LogInfo($"Spawned prefab for room '{r.Definition.id}' into sandbox (inactive). Enable it manually to preview.");
        }
    }
}
