using System.Reflection.Emit;

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

    // Forced zones are added right after the visited sectors are cleared, so they are handled as near zones
    // and the distant object search skips them.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.FindSectorObjects))]
    private static IEnumerable<CodeInstruction> FindSectorObjects(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(HashSet<ZoneSystem.SectorIndex>), nameof(HashSet<ZoneSystem.SectorIndex>.Clear))))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ForceActivePatches), nameof(AddForceActiveZones))))
            .InstructionEnumeration();

    private static void AddForceActiveZones(ZDOMan manager, List<ZDO> sectorObjects)
    {
        foreach (var zone in ZoneLoadingManager.ForceActive)
            manager.FindObjects(zone, sectorObjects, manager.m_visitedSectorIndices);
    }

    // Spawners only run when a player is near, so forced zones get an unlimited trigger distance.
    private static float TriggerDistance(Component spawner, float triggerDistance)
    {
        var forceActive = ZoneLoadingManager.ForceActive;
        if (forceActive.Count == 0 || !forceActive.Contains(ZoneSystem.GetZone(spawner.transform.position)))
            return triggerDistance;
        return float.MaxValue;
    }

    private static float SpawnAreaTriggerDistance(SpawnArea spawner) => TriggerDistance(spawner, spawner.m_triggerDistance);

    private static float CreatureSpawnerTriggerDistance(CreatureSpawner spawner) => TriggerDistance(spawner, spawner.m_triggerDistance);

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SpawnArea), nameof(SpawnArea.UpdateSpawn))]
    private static IEnumerable<CodeInstruction> SpawnAreaUpdateSpawn(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SpawnArea), nameof(SpawnArea.m_triggerDistance))))
            .Set(OpCodes.Call, AccessTools.Method(typeof(ForceActivePatches), nameof(SpawnAreaTriggerDistance)))
            .InstructionEnumeration();

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.UpdateSpawner))]
    private static IEnumerable<CodeInstruction> CreatureSpawnerUpdateSpawner(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(CreatureSpawner), nameof(CreatureSpawner.m_triggerDistance))))
            .Repeat(matcher => matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(ForceActivePatches), nameof(CreatureSpawnerTriggerDistance))))
            .InstructionEnumeration();
}
