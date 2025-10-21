#if USE_UNITY_STUBS
using System;
using System.Collections.Generic;

// Minimal Unity API stubs to allow CI builds without Unity assemblies.
// DO NOT SHIP with this define enabled in actual mods.

namespace UnityEngine
{
    public class Object { }

    public class Transform
    {
        public Transform? parent { get; set; }
        public Transform? Find(string name) => null;
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
    }

    public struct Quaternion
    {
        public static Quaternion identity => new Quaternion();
    }

    public class GameObject : Object
    {
        public string name = string.Empty;
        public Transform transform { get; } = new Transform();
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public static GameObject? Find(string name) => null;
        public void SetActive(bool value) { }
    }

    public static class ObjectUtility
    {
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform? parent) where T : class => original;
        public static void DontDestroyOnLoad(Object o) { }
    }

    public class AssetBundle
    {
        public string name => string.Empty;
        public static AssetBundle? LoadFromFile(string path) => new AssetBundle();
        public string[] GetAllAssetNames() => Array.Empty<string>();
        public T? LoadAsset<T>(string name) where T : class => null;
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { }
    public enum LoadSceneMode { Single, Additive }
}
#endif
