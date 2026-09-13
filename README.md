# Danger Zone Overlay

A BepInEx client mod for **SPT** (Single Player Tarkov / SPT-AKI) that visualizes map kill-zones as semi-transparent 3D volumes in-game.

A modern rework + extension of the classic **ShowLandMines** mod, adapted to SPT 4.1.

## Features

- **Mine zones** — red semi-transparent boxes for `EFT.Interactive.Minefield` and global `MineDirectionalColliders`
- **Sniper zones** — blue semi-transparent boxes for `EFT.Interactive.SniperFiringZone`
- **Map borders** — white translucent boxes / spheres / capsules for each map's `*_LevelBorders` objects
- Independent per-category visibility toggles and opacity sliders (0–1) that apply live, no rescan required
- In-game hotkeys: `F8` toggle overlay, `F9` force re-scan of the current scene

## Requirements

- SPT (tested on 4.1.x, game build `2026/8/2`)
- BepInEx 5 (`5.4.23.x`)
- Optional: **ConfigurationManager** for in-game settings UI

## Install

1. Download the package from [Releases](../../releases) and unzip into your SPT root (e.g. `E:\SPT\`), merging folders.
2. Start the game — zones are scanned automatically once a raid scene loads.
3. `F8` toggles the whole overlay; `F9` re-scans the scene.

All options live in `BepInEx\config\com.local.dangerzoneoverlay.cfg` — edit it directly or tweak live with ConfigurationManager (`Ctrl+Shift+C`):

| Setting                       | Description                           |
|-------------------------------|---------------------------------------|
| `Toggle overlay` (F8)         | Global visibility switch              |
| `Rescan zones` (F9)           | Force re-scan of the current scene    |
| `Show mine/sniper/border zones` | Per-category visibility checkboxes  |
| `Mine/Sniper/Border opacity`  | Per-category opacity sliders (0–1)    |

## Building from source

The game assemblies are licensed and **not** committed to this repository. Point the `<Reference HintPath>` entries in `src/DangerZoneOverlay.csproj` at your own SPT install before building:

- `EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll`
- `EscapeFromTarkov_Data\Managed\UnityEngine.dll` (+ CoreModule / PhysicsModule / IMGUIModule / TextRenderingModule / JSONSerializeModule)
- `BepInEx\core\BepInEx.dll`

```
dotnet build -c Release src/DangerZoneOverlay.csproj
```

Output: `src/bin/Release/DangerZoneOverlay.dll`. Copy it to `BepInEx\plugins\DangerZoneOverlay\`.

## Acknowledgements

- Original ideas taken from the classic `ShowLandMines` mod.

## Disclaimer

Fan project for the SPT community. Not affiliated with or endorsed by Battlestate Games. EFT assets are the property of Battlestate Games.