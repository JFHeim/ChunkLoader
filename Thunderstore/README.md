<img src="https://thunderstore.io/thumbnail-serve/repository/icons/Frogger-ChunkLoader-1.7.0.png/?width=128&height=128" align="right" alt="Project Logo">

# ChunkLoader

A Valheim mod that adds an Observation Column to keep the surrounding chunk active even when no players are nearby.
Similar to Chunk Loader mods in Minecraft.

## About

**ChunkLoader** solves the problem of production stopping at farms, pens, smelters, etc., when players leave the area.
The Observation Column keeps the current chunk (a 64x64 meter area) loaded.

To maintain operation, the column requires fuel (default: Thunderstone). When the fuel runs out, the glow color changes
to red and it stops holding the chunk.

**Main Features:**

* Mobs remain active, tamed animals breed, furnaces work — everything happens as if a player were present.
* When placing, you can highlight the boundaries of the chunk covered by the selected column.
* Fuel consumption, per-player limits, and building costs are adjustable in the config.

### Controls

| Action         | Key           | Description                              |
|:---------------|:--------------|:-----------------------------------------|
| **Refuel**     | `E`           | Add 1 unit of fuel from inventory.       |
| **View Chunk** | `L-Shift + E` | Highlight chunk boundaries with a flash. |

### Crafting

The column is found in the **Hammer** menu under the **Misc** category:

* **Requires:** Forge
* **Stone:** 25
* **Thunderstone:** 5
* **Surtling Core:** 1

## Configuration

Configuration file: `BepInEx/config/com.Frogger.ChunkLoader.cfg`. Parameters are synchronized with the server.

| Parameter                      | Type   | Default        | Description                                |
|:-------------------------------|:-------|:---------------|:-------------------------------------------|
| `Max fuel`                     | int    | 100            | Maximum fuel capacity.                     |
| `Start fuel`                   | int    | 1              | Amount of fuel upon construction.          |
| `Fuel item`                    | string | "Thunderstone" | Prefab of the item used as fuel.           |
| `Minutes for one fuel item`    | int    | 5              | Minutes of operation per 1 unit of fuel.   |
| `Infinite fuel`                | bool   | false          | If true, no fuel is required.              |
| `ChunkLoaders limit by player` | int    | 2              | Limit of columns per player.               |
| `Terrain flash color`          | Color  | Yellow         | Flash color when viewing chunk boundaries. |

## Install Notes

The mod must be installed on **both server and client**. The configuration is server-controlled to prevent clients from
bypassing limits or using infinite fuel.

## Credits

<img alt="Discord Logo" src="https://freelogopng.com/images/all_img/1691730813discord-icon-png.png" width="16"> Discord — `justafrogger`<br>
<img alt="GitHub Logo" src="https://github.githubassets.com/assets/pinned-octocat-093da3e6fa40.svg" width="16"/> [Source Code](https://github.com/JFHeim/ChunkLoader)<br>

<ins>If something doesn't work, you found a bug, or have suggestions — message on Discord or open a GitHub Issue.</ins>

Need a custom mod? Contact me to discuss 😉.

## Screenshots
![Screenshot1](ttps://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_1.jpeg?raw=true)
![Screenshot2](ttps://github.com/JFHeim/ChunkLoader/blob/main/Thunderstore/img/screenshot_1.jpeg?raw=true)