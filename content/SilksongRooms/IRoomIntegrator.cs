using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongRooms._1;

public interface IRoomIntegrator
{
    // Called after rooms are loaded
    void RegisterRooms(IReadOnlyList<LoadedRoom> rooms);

    // Scene hook so implementors can integrate when relevant
    void OnSceneLoaded(Scene scene, LoadSceneMode mode, IReadOnlyList<LoadedRoom> rooms);
}
