# ArchitectRooms Plugin Template

A standalone BepInEx plugin that bridges the Architect mod’s exported rooms into the SilksongRooms loader. It watches the Architect export folder under `%UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Exports`, registers any `*.room.json` it finds (and their bundles), and keeps listening for new exports.

## What it does
- On startup, resolves the Architect exports folder and calls `RoomsApi.RegisterRoomsFolder`.
- Optionally watches the folder for changes and registers new/updated `*.room.json` automatically.
- Depends on `io.github.silksongrooms__1` (SilksongRooms) being installed and loaded first.

## Configuration (BepInEx)
- `Architect.ExportsPath` — override the folder to watch; defaults to LocalLow path above.
- `Architect.WatchForChanges` — toggle the FileSystemWatcher (default: true).

## Expected folder layout
```
%UserProfile%/AppData/LocalLow/TeamCherry/Silksong/Architect/Exports/
  MyFirstRoom.room.json
  Bundles/
    my_first_room.bundle
```
Bundle paths inside JSON are resolved relative to the folder being watched, so keep the `Bundles/` subfolder next to your room JSON files.

## Building & copying
- Configure `SilksongPath.props` (see other templates in this repo) if you want the build to copy the DLL to your Silksong install.
- `dotnet build content/ArchitectRooms/ArchitectRooms.1.csproj -c Release`
- Output copies to `BepInEx/plugins/ArchitectRooms/` when `SilksongPath.props` is present. A zip lands in `bin/Publish/ArchitectRooms.zip`.

## Notes
- If SilksongRooms is missing, this plugin logs a warning and does nothing.
- If Architect uses a different export directory on your machine, set `Architect.ExportsPath` accordingly.
