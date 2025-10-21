Templates for modding Hollow Knight: Silksong. 

Usage: navigate to the folder you want to create a mod, then run the following:

```
> dotnet new install Silksong.Modding.Templates

> dotnet new silksongplugin
```

By default, the template automatically targets the latest available version of the game. If you want to make mods
specifically targeting an older version of the game, you can use the `-gv` flag to specify the version.

Use `dotnet new silksongplugin --help` to see additional options.

## SilksongRooms: shareable room loader

This repo includes a template (`content/SilksongRooms`) that ships a reusable room loader and a tiny integrator. It lets any mod load room JSON files (`*.room.json`) and Unity AssetBundles.

Highlights:
- Drop rooms in `BepInEx/plugins/<YourMod>/Rooms/` and they get picked up automatically by the SilksongRooms plugin.
- Other mods can register their own Rooms folders at runtime via a simple API.

Use from another mod:

```csharp
using BepInEx;
using SilksongRooms._1;

[BepInDependency("io.github.silksongrooms__1")]
[BepInPlugin("your.guid.here", "Your Mod", "1.0.0")]
public class YourMod : BaseUnityPlugin
{
	private void Awake()
	{
		var myRooms = System.IO.Path.Combine(BepInEx.Paths.PluginPath, Info.Metadata.Name, "Rooms");
		RoomsApi.RegisterRoomsFolder(myRooms);
		// Or single file:
		// RoomsApi.RegisterRoomJson(System.IO.Path.Combine(myRooms, "my_room.room.json"));
	}
}
```

JSON schema and more details are in `content/SilksongRooms/README.md`.