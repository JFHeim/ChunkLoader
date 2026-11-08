namespace ChunkLoader.Patch;

[HarmonyPatch]
public class ForceActivePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.CreateLocalZones))]
    private static void CreateLocalZones(ZoneSystem __instance, ref bool __result)
    {
        if (__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        foreach (var zone in forceActive)
        {
            if (!__instance.PokeLocalZone(zone)) continue;
            __result = true;
            break;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.IsActiveAreaLoaded))]
    private static void IsActiveAreaLoaded(ZoneSystem __instance, ref bool __result)
    {
        if (!__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        foreach (var zone in forceActive)
        {
            if (__instance.m_zones.ContainsKey(zone)) continue;
            __result = false;
            break;
        }
    }

    // InActiveArea and OutsideActiveArea both end up here, so forced zones are always active.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.PointInsideActiveArea))]
    private static void PointInsideActiveArea(Vector3 point, ref bool __result)
    {
        if (__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        if (forceActive.Contains(ZoneSystem.GetZone(point))) __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindSectorObjects))]
    private static void FindSectorObjects(ZDOMan __instance, Vector2s sector, int area, List<ZDO> sectorObjects)
    {
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        foreach (var zone in forceActive)
        {
            if (zone.x >= sector.x - area && zone.x <= sector.x + area && zone.y <= sector.y + area && zone.y >= sector.y - area) continue;
            __instance.FindObjects(zone, sectorObjects, __instance.m_visitedSectorIndices);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindDistantObjects))]
    private static bool FindDistantObjects(Vector2s sector)
    {
        if (ZoneLoadingManager.ForceActive.Contains(sector)) return false;
        return true;
    }
}