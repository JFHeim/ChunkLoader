using System.Text.RegularExpressions;
using BepInEx.Configuration;

namespace ChunkLoader.Config;

public partial class ConfigsContainer
{
    public static ItemDrop? FuelItem { get; private set; }

    public static int    LimitByPlayer;
    public static int    MaxFuel;
    public static int    StartFuel;
    public static string FuelItemName = null!;
    public static int    MinutesForOneFuelItem;
    public static bool   InfiniteFuel;
    public static Color  TerrainFlashColor;

    private readonly ConfigEntry<int> _limitByPlayerConfig;
    private readonly ConfigEntry<int> _maxFuelConfig;
    private readonly ConfigEntry<int> _startFuelConfig;
    private readonly ConfigEntry<string> _fuelItemConfig;
    private readonly ConfigEntry<int> _minutesForOneFuelItemConfig;
    private readonly ConfigEntry<bool> _infiniteFuelConfig;
    private readonly ConfigEntry<Color> _terrainFlashColorConfig;

    private ConfigsContainer()
    {
        _limitByPlayerConfig         = config("Main",     "ChunkLoaders limit by player", 2,                  "");
        _terrainFlashColorConfig     = config("Main",     "Terrain flash color",          Color.blue,      "");
        _maxFuelConfig               = config("Fuelling", "Max fuel",                     100,                "");
        _startFuelConfig             = config("Fuelling", "Start fuel",                   1,                  "");
        _fuelItemConfig              = config("Fuelling", "Fuel item",                    Consts.DefaultFuel, "");
        _minutesForOneFuelItemConfig = config("Fuelling", "Minutes for one fuel item",    5,                  "");
        _infiniteFuelConfig          = config("Fuelling", "Infinite fuel",                false,              "");
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

        InfiniteFuel          = _infiniteFuelConfig.Value;
        TerrainFlashColor     = _terrainFlashColorConfig.Value;
        FuelItemName              = _fuelItemConfig.Value;

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
