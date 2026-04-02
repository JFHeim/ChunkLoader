using JetBrains.Annotations;

namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
internal static class InitZoneLoadingManagerPatch
{
    [HarmonyPostfix, UsedImplicitly]
    private static void AddComp(ZNetScene __instance)
    {
        if (__instance.gameObject.TryGetComponent<ZoneLoadingManager>(out var _)) return;
        __instance.gameObject.AddComponent<ZoneLoadingManager>();
    }
}