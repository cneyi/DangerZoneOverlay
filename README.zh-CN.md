# Danger Zone Overlay

[简体中文](README.zh-CN.md) | [English](README.md)

为单机版 **Tushonka**（SPT，原 SPT-AKI）编写的 BepInEx 客户端插件，可在游戏内把地雷区、狙击区、地图边界等隐形杀伤区域以半透明 3D 体积实时显示出来。

经典 **ShowLandMines** 插件的现代化重构与扩展，已适配 SPT 4.1。

## 功能

- **雷区**：`EFT.Interactive.Minefield` 与全局命名空间的 `MineDirectionalColliders` → 红色半透明盒
- **狙击区**：`EFT.Interactive.SniperFiringZone` → 蓝色半透明盒
- **地图边界**：各地图 `*_LevelBorders` → 半透明白色盒 / 球 / 胶囊
- 每个分类独立开关 + 透明度滑块（0~1），实时生效，无需重新扫描
- 游戏内快捷键：`F8` 开/关整体显示，`F9` 强制重扫当前场景

## 环境要求

- SPT（已在 4.1.x、游戏版本 `2026/8/2` 测试）
- BepInEx 5（`5.4.23.x`）
- 可选：**ConfigurationManager**，用于游戏内设置界面

## 安装

1. 从 [Releases](../../releases) 下载安装包，解压到 SPT 根目录（如 `E:\SPT\`）并合并文件夹。
2. 启动游戏，进入战局后自动扫描并显示各类区域。
3. `F8` 开/关整体显示；`F9` 强制重扫当前场景。

所有选项集中在 `BepInEx\config\com.local.dangerzoneoverlay.cfg`（`Settings` 节）。可直接编辑该文件，或用 ConfigurationManager（`Ctrl+Shift+C`）在游戏内实时调节：

| 设置项                          | 说明                     |
|---------------------------------|--------------------------|
| `Toggle overlay` (F8)          | 整体显示开关             |
| `Rescan zones` (F9)            | 强制重新扫描当前场景     |
| `Show mine/sniper/border zones` | 按分类开关（雷区/狙击区/边界） |
| `Mine/Sniper/Border opacity`   | 按分类透明度（0~1）      |

## 从源码构建

游戏的程序集受版权保护，**未纳入本仓库**。构建前需要将 `src/DangerZoneOverlay.csproj` 中的 `<Reference HintPath>` 指向你自己的 SPT 安装目录：

- `EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll`
- `EscapeFromTarkov_Data\Managed\UnityEngine.dll`（以及 CoreModule / PhysicsModule / IMGUIModule / TextRenderingModule / JSONSerializeModule）
- `BepInEx\core\BepInEx.dll`

> 说明：`EscapeFromTarkov_Data` 是游戏的实际目录名（固定路径），无法也不应改名，以免构建失效。

```
dotnet build -c Release src/DangerZoneOverlay.csproj
```

产物：`src/bin/Release/DangerZoneOverlay.dll`，复制到 `BepInEx\plugins\DangerZoneOverlay\` 即可。

## 致谢

- 功能灵感来自经典 `ShowLandMines` 插件。

## 免责声明

面向 SPT 玩家社区的爱好者项目，与 Battlestate Games 无关联、未获其认可。EFT 资源版权归 Battlestate Games 所有。