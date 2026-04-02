using ChunkLoader.Config;

namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(Player))]
internal static class LimitByPlayerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
    private static void PreventInvalidPlacement_OnPlacementGhost(Player __instance) => TheCheck(__instance, false);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.PlacePiece))]
    private static void PreventInvalidPlacement_OnPlace(Player __instance) => TheCheck(__instance, true);

    private static void TheCheck(Player __instance, bool showMessage)
    {
        var player = __instance;

        if (!Player.m_localPlayer || Player.m_localPlayer != player) return;
        if (Player.m_debugMode) return;
        if (Utils.GetPrefabName(player.m_placementGhost) != Consts.PrefabName) return;
        var piece = player.m_placementGhost?.GetComponent<Piece>();
        if (!piece) return;

        if (ZoneLoadingManager.ForceActive.Contains(ZoneSystem.GetZone(Player.m_localPlayer.transform.position)))
        {
            player.m_placementStatus = Player.PlacementStatus.Invalid;
            player.SetPlacementGhostValid(false);
            if (showMessage) player.Message(MessageHud.MessageType.Center, "$chunk_loader_already_placed_in_zone");
            return;
        }

        var zdos = ZoneLoadingManager.LoadersInWold.Where(x => x.GetLong(ZDOVars.s_creator) == Player.m_localPlayer.GetPlayerID()).ToList();
        if (zdos.Count >= ConfigsContainer.LimitByPlayer)
        {
            player.m_placementStatus = Player.PlacementStatus.Invalid;
            player.SetPlacementGhostValid(false);
            if (showMessage) player.Message(MessageHud.MessageType.Center, "$you_have_too_many_chunk_loaders_placed");
        }
    }
}