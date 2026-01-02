using System;
using System.Collections.Generic;
using UnityEngine;

namespace HKEnemiesForArchitect._1;

// Public API for other mods (e.g., Architect) to query available enemy prefabs loaded from bundles
public static class HKEnemiesApi
{
    public static event Action<IReadOnlyDictionary<string, EnemyEntry>>? EnemiesChanged;

    private static EnemyLibrary? _library;

    internal static void Bind(EnemyLibrary library)
    {
        _library = library;
        NotifyChanged(_library.Enemies);
    }

    internal static void NotifyChanged(IReadOnlyDictionary<string, EnemyEntry> enemies)
    {
        EnemiesChanged?.Invoke(enemies);
    }

    public static IReadOnlyDictionary<string, EnemyEntry> GetAll()
    {
        return _library?.Enemies ?? new Dictionary<string, EnemyEntry>();
    }

    public static bool TryGetEnemyPrefab(string id, out GameObject? prefab)
    {
        prefab = null;
        if (_library == null) return false;
        if (_library.TryGetEnemy(id, out var entry) && entry?.Prefab != null)
        {
            prefab = entry.Prefab;
            return true;
        }
        return false;
    }
}
