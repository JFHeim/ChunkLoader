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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.InActiveArea), typeof(Vector2i), typeof(Vector2i))]
    private static void InActiveArea(Vector2i zone, Vector2i refCenterZone, ref bool __result)
    {
        if (__result) return;
        if (ZoneLoadingManager.ForceActive.Contains(zone)) __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.InActiveArea), [typeof(Vector2i), typeof(Vector2i), typeof(int)])]
    private static void InActiveArea2(Vector2i zone, ref bool __result)
    {
        if (__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        if (forceActive.Contains(zone)) __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.OutsideActiveArea), typeof(Vector3), typeof(Vector3))]
    private static void OutsideActiveArea1(Vector3 point, ref bool __result)
    {
        if (!__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        if (forceActive.Contains(ZoneSystem.GetZone(point))) __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.OutsideActiveArea), typeof(Vector3), typeof(Vector2i), typeof(int))]
    private static void OutsideActiveArea2(Vector3 point, ref bool __result)
    {
        if (!__result) return;
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        if (forceActive.Contains(ZoneSystem.GetZone(point))) __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindSectorObjects))]
    private static void FindSectorObjects(ZDOMan __instance, Vector2i sector, int area, List<ZDO> sectorObjects)
    {
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0) return;
        foreach (var zone in forceActive)
        {
            if (zone.x >= sector.x - area && zone.x <= sector.x + area && zone.y <= sector.y + area && zone.y >= sector.y - area) continue;
            __instance.FindObjects(zone, sectorObjects);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindDistantObjects))]
    private static bool FindDistantObjects(Vector2i sector) => !ZoneLoadingManager.ForceActive.Contains(sector);
}