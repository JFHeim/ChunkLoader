using ChunkLoader.Config;
using ChunkLoader.Helpers;

namespace ChunkLoader;

public class ZoneLoadingManager : MonoBehaviour
{
    public static HashSet<Vector2i> ForceActive => _instance?._forceActive ?? [];
    public static List<ZDO> LoadersInWorld => _instance?._loadersInWorld ?? [];

    private readonly HashSet<Vector2i> _forceActive = [];
    private readonly List<ZDO> _loadersInWorld = [];
    private readonly List<Minimap.PinData> _loadersMinimapPins = [];

    private static ZoneLoadingManager? _instance = null;

    private void Awake()
    {
        _instance = this;
        StartCoroutine(MonitorZonesList());
    }

    private IEnumerator MonitorZonesList()
    {
        List<ZDO> tempLoaders = [];
        while (true)
        {
            const int UPDATE_INTERVAL = 4;
            yield return new WaitForSeconds(UPDATE_INTERVAL);
            if (ZDOMan.instance == null) continue;

            tempLoaders.Clear();
            yield return FindLoadersInWorld(tempLoaders);
            UpdateFuelInLoaders();
            UpdateZonesList();

            yield return null;
            UpdateLoaderPins();
        }

        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator FindLoadersInWorld(List<ZDO> tempLoaders)
    {
        int index = 0;

        while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(Consts.PrefabName, tempLoaders, ref index))
            yield return null;

        _loadersInWorld.Clear();
        _loadersInWorld.AddRange(tempLoaders);
    }

    private void UpdateFuelInLoaders()
    {
        foreach (var zdo in _loadersInWorld)
        {
            if (zdo.HasOwner() && !zdo.IsOwner()) continue;
            if (!zdo.HasOwner())
            {
                Log.Warning("Chunk loader doesn't have an owner which is strange");
                if (ZNet.instance.IsServer()) zdo.SetOwner(ZDOMan.GetSessionID());
            }

            UpdateFireplaceFuel(zdo);
        }
    }

    private static void UpdateFireplaceFuel(ZDO zdo)
    {
        var secPerFuel = TimeSpan.FromMinutes(ConfigsContainer.MinutesForOneFuelItem).TotalSeconds;
        if (secPerFuel <= 0) return;

        double currentFuelLevel = (double)zdo.GetFloat(ZDOVars.s_fuel, ConfigsContainer.StartFuel);
        var timeSinceLastUpdate = RetrieveTimeSinceLastFuelUpdate(zdo);
        if (ChunkLoaderMono.IsBurning(zdo) && !ConfigsContainer.InfiniteFuel)
        {
            double fuelSpent = timeSinceLastUpdate / secPerFuel;
            double newFuel = currentFuelLevel - fuelSpent;
            if (newFuel <= 0.0) newFuel = 0;
            zdo.Set(ZDOVars.s_fuel, (float)newFuel);
        }
    }

    private void UpdateZonesList()
    {
        _forceActive.Clear();
        foreach (var zdo in _loadersInWorld)
        {
            if (!ChunkLoaderMono.IsBurning(zdo)) continue;
            _forceActive.Add(ZoneSystem.GetZone(zdo.GetPosition()));
        }
    }

    private void UpdateLoaderPins()
    {
        var minimap = Minimap.instance;
        if (!minimap) return;

        foreach (var pinData in _loadersMinimapPins) minimap.RemovePin(pinData);
        _loadersMinimapPins.Clear();
        foreach (var zdo in _loadersInWorld)
        {
            if (!zdo.IsValid()) continue;

            var fuel = zdo.GetFloat(ZDOVars.s_fuel, 0);
            var isBurning = ChunkLoaderMono.IsBurning(zdo);
            var position = zdo.GetPosition();

            var pinType = isBurning ? Consts.ChunkLoaderPinTypeBurning : Consts.ChunkLoaderPinTypeNotBurning;
            var pinData = minimap.AddPin(position, pinType, string.Empty, false, false);
            _loadersMinimapPins.Add(pinData);

            var timeRemaining = TimeSpan.FromMinutes(ConfigsContainer.MinutesForOneFuelItem * fuel);
            var oldName = pinData.m_name;
            pinData.m_name = isBurning ? timeRemaining.ToHumanTimeString() : string.Empty;
            if (oldName != pinData.m_name) minimap.m_pinUpdateRequired = true;
            if (pinData.m_NamePinData == null)
            {
                pinData.m_NamePinData = new Minimap.PinNameData(pinData);
                minimap.CreateMapNamePin(pinData, minimap.m_pinNameRootLarge);
            }
        }
    }


    private static double RetrieveTimeSinceLastFuelUpdate(ZDO zdo)
    {
        var time = ZNet.instance.GetTime();
        DateTime dateTime = new(zdo.GetLong(ZDOVars.s_lastTime, defaultValue: time.Ticks));
        var timeSpan = time - dateTime;
        var timeSinceLastUpdate = timeSpan.TotalSeconds;
        if (timeSinceLastUpdate < 0) timeSinceLastUpdate = 0;
        zdo.Set(ZDOVars.s_lastTime, time.Ticks); // s_lastTime = last time checked

        return timeSinceLastUpdate;
    }
}