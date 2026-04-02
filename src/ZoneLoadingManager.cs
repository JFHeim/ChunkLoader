using System.Diagnostics;
using ChunkLoader.Config;

namespace ChunkLoader;

public class ZoneLoadingManager : MonoBehaviour
{
    public static readonly HashSet<Vector2i> ForceActive = [];
    public static readonly List<ZDO> LoadersInWold = [];

    private void Awake()
    {
        StartCoroutine(MonitorZonesList());
    }

    private static IEnumerator MonitorZonesList()
    {
        var sw = new Stopwatch();
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (ZDOMan.instance == null) continue;

            LoadersInWold.Clear();
            sw.Restart();
            List<ZDO> allZdos = ZDOMan.instance.m_objectsByID.Values.ToList();

            for (var index = 0; index < allZdos.Count; index++)
            {
                var zdo = allZdos[index];
                if (zdo is null || zdo.GetPrefab() != Consts.PrefabNameHash) continue;
                LoadersInWold.Add(zdo);
                if (index % 1000 == 0) yield return null;
            }

            sw.Stop();
            // Log.Info($"Found {LoadersInWold.Count} loaders in {sw.Elapsed.TotalSeconds}sec");


            ForceActive.Clear();
            foreach (var zdo in LoadersInWold)
            {
                if (!zdo.HasOwner() || (zdo.HasOwner() && zdo.IsOwner()))
                {
                    if(!zdo.HasOwner()) zdo.SetOwner(ZDOMan.GetSessionID());
                    UpdateFireplaceFuel(zdo);
                }
                if (!ChunkLoaderMono.IsBurning(zdo)) continue;
                ForceActive.Add(ZoneSystem.GetZone(zdo.GetPosition()));
            }
        }

        // ReSharper disable once IteratorNeverReturns
    }

    private static void UpdateFireplaceFuel(ZDO zdo)
    {
        if(ConfigsContainer.InfiniteFuel) return;

        var secPerFuel = TimeSpan.FromMinutes(ConfigsContainer.MinutesForOneFuelItem).TotalSeconds;
        if (secPerFuel <= 0) return;

        var currentFuelLevel = zdo.GetFloat(ZDOVars.s_fuel);
        var timeSinceLastUpdate = RetrieveTimeSinceLastFuelUpdate(zdo);
        if (ChunkLoaderMono.IsBurning(zdo) && !ConfigsContainer.InfiniteFuel)
        {
            var fuelSpent = timeSinceLastUpdate / secPerFuel;
            var newFuel = currentFuelLevel - fuelSpent;
            if (newFuel <= 0.0) newFuel = 0;
            zdo.Set(ZDOVars.s_fuel, (float)newFuel);
        }
    }

    private static double RetrieveTimeSinceLastFuelUpdate(ZDO zdo)
    {
        var time = ZNet.instance.GetTime();
        DateTime dateTime = new(zdo.GetLong(ZDOVars.s_lastTime, time.Ticks));
        var timeSpan = time - dateTime;
        var timeSinceLastUpdate = timeSpan.TotalSeconds;
        if (timeSinceLastUpdate < 0) timeSinceLastUpdate = 0;
        zdo.Set(ZDOVars.s_lastTime, time.Ticks);

        return timeSinceLastUpdate;
    }
}