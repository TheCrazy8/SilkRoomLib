using System;
using UnityEngine;

namespace SilksongRooms._1;

[Serializable]
public class RoomDefinition
{
    public string id = string.Empty;
    public string bundle = string.Empty; // relative path within Rooms/
    public string prefab = string.Empty; // name of prefab inside the bundle
    public Vector3 entryPoint;           // where to spawn player when entering this room
    public string[] tags = Array.Empty<string>();
    public Connection[] connections = Array.Empty<Connection>();
}

[Serializable]
public class Connection
{
    public string toRoomId = string.Empty; // id of destination room
    public string doorName = string.Empty; // optional name of a door/exit in this room
}
