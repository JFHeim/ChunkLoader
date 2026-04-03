namespace ChunkLoader.Patch;

[HarmonyWrapSafe]
internal static class FindShaderReferences
{
    public static IReadOnlyDictionary<string, Shader> Shaders => _shaders;
    private static readonly Dictionary<string, Shader> _shaders = [];

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Start), [])]
    private static void LogShadersRefs()
    {
        if(!ZNetScene.instance) return;

        foreach (var meshRenderer in ZNetScene.instance.m_prefabs.SelectMany(x => x.GetComponentsInChildren<MeshRenderer>()).Where(x=>x))
        foreach (var material in meshRenderer.sharedMaterials.Where(x=>x))
        {
            if (_shaders.ContainsKey(material.shader.name)) continue;
            _shaders.Add(material.shader.name, material.shader);
        }
    }
}