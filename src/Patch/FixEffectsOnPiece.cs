using JetBrains.Annotations;

namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
internal static class FixEffectsOnPiece
{
    [HarmonyPostfix, UsedImplicitly]
    private static void Postfix(ZNetScene __instance)
    {
        var chunkLoader = __instance.GetPrefab(Consts.PrefabNameHash)?.GetComponent<WearNTear>();
        if (!chunkLoader) return;

        var guardStone = __instance.GetPrefab("guard_stone");
        Fix(chunkLoader.transform, guardStone.transform, "sparcs");
        Fix(chunkLoader.transform, guardStone.transform, "flare");

        var stonePile = __instance.GetPrefab("stone_pile")?.GetComponent<WearNTear>();
        if (stonePile)
        {
            chunkLoader.GetComponent<Piece>().m_placeEffect = stonePile.GetComponent<Piece>().m_placeEffect;
            chunkLoader.m_destroyedEffect = stonePile.m_destroyedEffect;
            chunkLoader.m_hitEffect = stonePile.m_hitEffect;
        }

        var smelter = __instance.GetPrefab("smelter")?.GetComponent<WearNTear>();
        if (smelter) chunkLoader.GetComponent<ChunkLoaderMono>().m_fuelAddedEffects = smelter.GetComponent<Smelter>().m_fuelAddedEffects;
        Fix(chunkLoader.transform, guardStone.transform, "SM_cloumn", "default");
    }

    private static void Fix(Transform? origTransform, Transform? donorTransform, string origName, string? donorName = null)
    {
        donorName ??= origName;
        if (!origTransform || !donorTransform) return;
        var orig = Utils.FindChild(origTransform, origName)?.GetComponent<Renderer>();
        var donor = Utils.FindChild(donorTransform, donorName)?.GetComponent<Renderer>();
        if (!orig || !donor) return;
        orig.sharedMaterial.shader = donor.sharedMaterial.shader;
    }
}