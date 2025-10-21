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

## CI and colored releases

This repo ships a GitHub Actions workflow that:
- Builds the `SilksongRooms.1` DLL on pushes/PRs
- On tag push (tags like `v0.1.0`), creates a GitHub Release attaching the build artifacts
- Adds a color swatch to the release title/body for quick visual control

Color control:
- Include a 6-digit hex in your tag to set the color, e.g. `v0.1.0-ff00aa` or `v0.1.0-#ff00aa`
- The release title becomes `SilksongRooms.1 v0.1.0 — #ff00aa`
- The description shows a badge rendered with that color

Manual builds:

```powershell
# Build locally (Release); SkipCopyMod avoids zip/copy to game install in CI
dotnet build content/SilksongRooms/SilksongRooms.1.csproj -c Release -p:SkipCopyMod=true -p:game-version=latest
```