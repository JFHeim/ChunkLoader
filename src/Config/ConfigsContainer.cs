using BepInEx.Configuration;

namespace ChunkLoader.Config;

public partial class ConfigsContainer
{
    public static ItemDrop? FuelItem { get; private set; }

    public static int      LimitByPlayer;
    public static int      MaxFuel;
    public static int      StartFuel;
    public static string   FuelItemName = null!;
    public static int      MinutesForOneFuelItem;
    public static bool     InfiniteFuel;
    public static Color    TerrainFlashColor;
    public static TimeSpan TerrainFlashTime;
    public static bool     LoadSurroundingZones;  // Loads 9 chunks intead of 1

    private readonly ConfigEntry<int> _limitByPlayerConfig;
    private readonly ConfigEntry<int> _maxFuelConfig;
    private readonly ConfigEntry<int> _startFuelConfig;
    private readonly ConfigEntry<string> _fuelItemConfig;
    private readonly ConfigEntry<int> _minutesForOneFuelItemConfig;
    private readonly ConfigEntry<bool> _infiniteFuelConfig;
    private readonly ConfigEntry<Color> _terrainFlashColorConfig;
    private readonly ConfigEntry<float> _terrainFlashTimeConfig;
    private readonly ConfigEntry<bool> _loadSurroundingZones;

    private ConfigsContainer()
    {
        _limitByPlayerConfig = config(
            group: "Main",
            name: "ChunkLoaders limit by player",
            value: 2,
            description: "Maximum number of active ChunkLoaders allowed per player. Set to -1 for no limit."
        );

        _terrainFlashColorConfig = config(
            group: "Main",
            name: "Terrain flash color",
            value: Color.blue,
            description: "Color of the visual area indicator when showing loaded boundaries."
        );

        _terrainFlashTimeConfig = config(
            group: "Main",
            name: "Terrain flash time",
            value: 5f,
            description: "For how long to keep chunks highlighted. In seconds."
        );

        _loadSurroundingZones = config(
            group: "Main",
            name: "Load surrounding chunks",
            value: true,
            description: "If true, loads 9 chunks (3x3 grid). If false, only the central chunk is loaded."
        );

        _maxFuelConfig = config(
            group: "Fuelling",
            name: "Max fuel",
            value: 100,
            description: "The maximum fuel capacity of the ChunkLoader."
        );

        _startFuelConfig = config(
            group: "Fuelling",
            name: "Start fuel",
            value: 0,
            description: "Initial fuel amount provided when the ChunkLoader is first constructed."
        );

        _fuelItemConfig = config(
            group: "Fuelling",
            name: "Fuel item",
            value: Consts.DefaultFuel,
            description: "The prefab name of the item consumed as fuel (e.g., Coal, GreydwarfEye)."
        );

        _minutesForOneFuelItemConfig = config(
            group: "Fuelling",
            name: "Minutes for one fuel item",
            value: 5,
            description: "Real-time minutes of operation provided by a single fuel unit."
        );

        _infiniteFuelConfig = config(
            group: "Fuelling",
            name: "Infinite fuel",
            value: false,
            description: "If enabled will operate indefinitely without consuming fuel."
        );
    }

    private void ApplyConfiguration()
    {
        if (_limitByPlayerConfig.Value < 0) LimitByPlayer = int.MaxValue;
        LimitByPlayer         = _limitByPlayerConfig.Value;

        if (_maxFuelConfig.Value <= 0) Log.Warning("Configuration invalid, MaxFuel should be > 0");
        else MaxFuel               = _maxFuelConfig.Value;

        if (_startFuelConfig.Value < 0) Log.Warning("Configuration invalid, StartFuel should be >= 0");
        else StartFuel             = _startFuelConfig.Value;

        if (_minutesForOneFuelItemConfig.Value <= 0) Log.Warning("Configuration invalid, MinutesForOneFuelItem should be > 0");
        else MinutesForOneFuelItem = _minutesForOneFuelItemConfig.Value;

        if (_terrainFlashTimeConfig.Value <= 0) Log.Warning("Configuration invalid, TerrainFlashTime should be > 0 seconds");
        else if (_terrainFlashTimeConfig.Value > 300) Log.Warning("Configuration invalid, TerrainFlashTime should be < 300 seconds. Why the heck would you highlight terrain for more than 5 minutes?");
        else TerrainFlashTime = TimeSpan.FromSeconds(_terrainFlashTimeConfig.Value);

        InfiniteFuel          = _infiniteFuelConfig.Value;
        TerrainFlashColor     = _terrainFlashColorConfig.Value;
        FuelItemName          = _fuelItemConfig.Value;
        LoadSurroundingZones  = _loadSurroundingZones.Value;

        AssignFuelItem();
    }

    internal static void AssignFuelItem()
    {
        if (ObjectDB.instance)
        {
            FuelItem = ObjectDB.instance.GetItemPrefab(FuelItemName)?.GetComponent<ItemDrop>();
            if (!FuelItem)
            {
                Log.Warning($"Item '{FuelItemName}' not found. Using default '{Consts.DefaultFuel}'.");
                FuelItem = ObjectDB.instance.GetItemPrefab(Consts.DefaultFuel)?.GetComponent<ItemDrop>();
            }
        }
    }
}
