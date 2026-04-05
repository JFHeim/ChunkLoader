using ChunkLoader.Config;

namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(Player))]
internal static class LimitByPlayerPatch
{
    // [HarmonyPostfix]
    // [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
    // private static void PreventInvalidPlacement_OnPlacementGhost(Player __instance) => TheCheck(__instance, false);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.TryPlacePiece))]
    private static bool PreventInvalidPlacement_PlacePiece(Player __instance, ref bool __result)
    {
        var result = TheCheck(__instance, true);
        if (result is Result.Invalid)
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.TryPlacePiece))]
    private static void ForceUpdateLoadersListAfterPlacement(Player __instance, Piece piece, ref bool __result)
    {
        if(!__result) return;
        ZoneLoadingManager.ForceUpdateLoadersList();
    }

    private static Result TheCheck(Player player, bool showMessage)
    {
        if (!Player.m_localPlayer || Player.m_localPlayer != player) return Result.Skip;
        if (Player.m_debugMode) return Result.Skip;

        var placementGhost = player.m_placementGhost;
        if (!placementGhost || Utils.GetPrefabName(placementGhost) != Consts.PrefabName) return Result.Skip;
        var piece = placementGhost.GetComponent<Piece>();
        if (!piece) return Result.Skip;

        var placementZone = ZoneSystem.GetZone(placementGhost.transform.position);
        if (ZoneLoadingManager.ForceActive.Contains(placementZone))
        {
            player.m_placementStatus = Player.PlacementStatus.Invalid;
            player.SetPlacementGhostValid(false);
            if (showMessage) player.Message(MessageHud.MessageType.Center, "$chunk_loader_already_placed_in_zone");
            return Result.Invalid;
        }

        var localPlayerId = Player.m_localPlayer.GetPlayerID();
        var zdos = ZoneLoadingManager.LoadersInWorld.Where(x => x.GetLong(ZDOVars.s_creator) == localPlayerId).ToList();
        if (zdos.Count >= ConfigsContainer.LimitByPlayer)
        {
            player.m_placementStatus = Player.PlacementStatus.Invalid;
            player.SetPlacementGhostValid(false);
            if (showMessage) player.Message(MessageHud.MessageType.Center, "$you_have_too_many_chunk_loaders_placed");
            return Result.Invalid;
        }

        return Result.Valid;
    }

    private enum Result
    {
        Skip,
        Valid,
        Invalid
    }
}