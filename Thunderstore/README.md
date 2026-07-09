<img src="https://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/icon_128.png?raw=true" align="right" alt="Project Logo">

# ChunkLoader

A Valheim mod that adds an Observation Column capable of keeping chunks loaded even when no players are nearby — similar to Chunk Loader blocks from Minecraft.

## About

**ChunkLoader** allows farms, breeders, smelters, windmills, and other world systems to continue operating while every player is away.

Each Observation Column keeps its surrounding area loaded. By default it loads a **3×3 grid of chunks** (9 chunks total), but this can be changed in the configuration to load only the center chunk.

The column consumes fuel while active (default: **Thunderstone**). Once fuel runs out—or the loader is manually disabled—it stops keeping chunks loaded and its emission changes to indicate the inactive state.

### Features

- Keep production running while players are away.
- Continue processing crops, animals, furnaces, windmills, and other world objects.
- Configurable fuel system.
- Configurable per-player placement limit.
- Supports loading either **1 chunk** or **9 surrounding chunks**.
- Toggle Chunk Loaders on and off without destroying them.
- Visualize the loaded area with configurable color and duration.
- Remaining operating time is shown directly on the minimap pin.

## Controls

| Action | Key | Description |
|:------|:----|:------------|
| **Enable / Disable** | `E` | Toggle the Chunk Loader on or off (if enabled in config). |
| **Add Fuel** | `1–8` (Use Item) | Use the configured fuel item on the loader to add one fuel. |
| **Show Loaded Area** | `L-Shift + E` | Highlight all chunks currently loaded by this Chunk Loader. |

## Crafting

The Observation Column can be crafted using the **Hammer** under the **Misc** category.

**Requirements**

- Forge
- Stone ×25
- Thunderstone ×5
- Surtling Core ×1

## Configuration

Configuration file:

`BepInEx/config/com.Frogger.ChunkLoader.cfg`

All configuration values are synchronized from the server.

| Parameter | Type | Default | Description |
|:----------|:-----|:--------|:------------|
| `ChunkLoaders limit by player` | int | `2` | Maximum number of Chunk Loaders each player may own. Use `-1` for unlimited. |
| `Terrain flash color` | Color | `Blue` | Color used when highlighting loaded chunks. |
| `Terrain flash time` | float | `5` | Duration (seconds) that highlighted chunks remain visible. |
| `Load surrounding chunks` | bool | `true` | Load a 3×3 area (9 chunks). If disabled, only the center chunk is loaded. |
| `Can turn off` | bool | `true` | Allows players to manually enable or disable Chunk Loaders. |
| `Max fuel` | int | `100` | Maximum fuel capacity. |
| `Start fuel` | int | `0` | Fuel automatically inserted when a Chunk Loader is first built. |
| `Fuel item` | string | `"Thunderstone"` | Prefab name of the item used as fuel. |
| `Minutes for one fuel item` | int | `15` | Real-time minutes provided by one fuel item. |
| `Infinite fuel` | bool | `false` | Chunk Loaders never consume fuel. |

## How Fuel Works

Each fuel item powers the Chunk Loader for the configured amount of real time.

A Chunk Loader remains active only when:

- it is **enabled**, and
- it has fuel (unless **Infinite fuel** is enabled).

Disabled Chunk Loaders consume no fuel and do not keep chunks loaded.

## Minimap

Every Chunk Loader is displayed on the minimap.

- Active Chunk Loaders use the active icon.
- Disabled or out-of-fuel Chunk Loaders use the inactive icon.
- Active loaders display their remaining operating time as the pin label.

## Installation

Install the mod on **both the server and all clients**.

Configuration is controlled by the server to ensure all players use the same settings.

## Credits

<img alt="Discord Logo" src="https://freelogopng.com/images/all_img/1691730813discord-icon-png.png" width="16"> Discord — `justafrogger`<br>
<img alt="Thunderstore Logo" src="https://gcdn.thunderstore.io/live/community/valheim/PNG_color_logo_only_1_transparent.png" width="14"/> [Thunderstore](https://valheim.thunderstore.io/package/Frogger/ChunkLoader/)
<img alt="GitHub Logo" src="https://github.githubassets.com/assets/pinned-octocat-093da3e6fa40.svg" width="16"/> [Source Code](https://github.com/JFHeim/ChunkLoader)<br>

If you encounter a bug, have a suggestion, or want to request a feature, feel free to contact me on Discord or open a GitHub Issue.

Need a custom mod? Contact me to discuss 😉

## Screenshots

![Screenshot1 - crafting](https://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_1.jpeg?raw=true)
![Screenshot2 - overview](https://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_2.jpeg?raw=true)
![Screenshot3 - working area hightlight](https://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_3.jpg?raw=true)
![Screenshot4 - on map](https://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_4.jpg?raw=true)