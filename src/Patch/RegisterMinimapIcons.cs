namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
internal static class RegisterMinimapIconsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start), [])]
    private static void AddIcons(Minimap __instance)
    {
        if (!Plugin.Instance || !Plugin.Instance._iconActive || !Plugin.Instance._iconDisabled) return;

        __instance.m_icons.Add(new Minimap.SpriteData
        {
            m_icon = Plugin.Instance._iconActive,
            m_name = Consts.ChunkLoaderPinTypeBurning = (Minimap.PinType)__instance.m_icons.Count + 1
        });
        __instance.m_icons.Add(new Minimap.SpriteData
        {
            m_icon = Plugin.Instance._iconDisabled,
            m_name = Consts.ChunkLoaderPinTypeNotBurning = (Minimap.PinType)__instance.m_icons.Count + 1
        });

        Array.Resize(ref __instance.m_visibleIconTypes, __instance.m_visibleIconTypes.Length + 2);

        __instance.m_visibleIconTypes[(int)Consts.ChunkLoaderPinTypeBurning] = true;
        __instance.m_visibleIconTypes[(int)Consts.ChunkLoaderPinTypeNotBurning] = true;
    }
}