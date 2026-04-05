using ChunkLoader.Config;

namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
internal static class AssignFuelItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake), [])]
    private static void ObjectDB_Postfix() => ConfigsContainer.AssignFuelItem();
}