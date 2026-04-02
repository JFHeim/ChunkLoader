using static ChunkLoader.Consts;

namespace ChunkLoader;

public static class MeshFlasher
{
    private static readonly CoroutineHandler _coroutineHandler;
    private static readonly List<Renderer> _renderersInFlashState = [];

    static MeshFlasher()
    {
        _coroutineHandler = new GameObject("CoroutineHandler").AddComponent<CoroutineHandler>();
        DontDestroyOnLoad(_coroutineHandler);
    }

    public static void Flash(this Renderer renderer, Color color, Color returnColor, float time = 0.3f)
    {
        _renderersInFlashState.RemoveAll(x => !x);
        if (_renderersInFlashState.Contains(renderer)) return;
        _coroutineHandler.StartCoroutine(HighlightObject(renderer, color, returnColor, time));
    }

    private static IEnumerator HighlightObject(Renderer obj, Color color, Color returnColor, float time)
    {
        _renderersInFlashState.Add(obj);

        var renderersInChildren = obj.GetComponentsInChildren<Renderer>();
        Material? heightmapMaterial = null;
        if (obj.name == "Terrain")
        {
            var heightmap = obj.GetComponent<Heightmap>();
            heightmapMaterial = Instantiate(new Material(heightmap.m_meshRenderer.material));
            heightmap.m_meshRenderer.material.shader
                = Shader.Find("Standard"); // TODO: this always fails, we need to find shader ref in some other way
        }

        foreach (var renderer in renderersInChildren)
        foreach (var material in renderer.materials)
        {
            if (material.HasProperty(EmissionColorShaderPropertyID))
                material.SetColor(EmissionColorShaderPropertyID, color * 0.7f);
            material.color = color;
        }

        yield return new WaitForSeconds(time);

        if (obj.name == "Terrain" && heightmapMaterial) obj.material = heightmapMaterial;
        foreach (var renderer in renderersInChildren)
        foreach (var material in renderer.materials)
        {
            if (material.HasProperty(EmissionColorShaderPropertyID))
                material.SetColor(EmissionColorShaderPropertyID, returnColor * 0f);
            material.color = returnColor;
        }

        _renderersInFlashState.Remove(obj);
    }

    private class CoroutineHandler : MonoBehaviour;
}