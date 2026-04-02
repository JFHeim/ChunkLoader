// TODO: we don't know what it is and is it needed

// namespace ChunkLoader.Patch;
//
// [HarmonyWrapSafe]
// internal static class SetupPlacementGhost
// {
//     [HarmonyPostfix]
//     [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
//     private static void Postfix(Player __instance)
//     {
//         if (!__instance || !__instance.m_placementGhost) return;
//         var monos = __instance.m_placementGhost.GetComponentsInChildren<ChunkLoaderMono>();
//         foreach (var child in monos) Destroy(child);
//     }
// }