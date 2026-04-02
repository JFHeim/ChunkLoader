#nullable enable
using JetBrains.Annotations;

// ReSharper disable all
namespace Managers.PieceManager;

[PublicAPI]
public static class MaterialReplacer
{
    private static readonly Dictionary<GameObject, bool> ObjectToSwap;
    private static readonly Dictionary<string, Material> OriginalMaterials;
    private static readonly Dictionary<GameObject, ShaderType> ObjectsForShaderReplace;
    private static readonly HashSet<Shader> CachedShaders = new();
    private static bool _hasRun = false;

    static MaterialReplacer()
    {
        OriginalMaterials = new Dictionary<string, Material>();
        ObjectToSwap = new Dictionary<GameObject, bool>();
        ObjectsForShaderReplace = new Dictionary<GameObject, ShaderType>();
        var harmony = new Harmony("org.bepinex.helpers.PieceManager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZoneSystem), nameof(ZoneSystem.Start)),
            postfix: new HarmonyMethod(typeof(MaterialReplacer), nameof(ReplaceAllMaterialsWithOriginal)));
    }

    public enum ShaderType
    {
        PieceShader,
        VegetationShader,
        RockShader,
        RugShader,
        GrassShader,
        CustomCreature,
        UseUnityShader
    }

    public static void RegisterGameObjectForShaderSwap(GameObject go, ShaderType type)
    {
        if (ObjectsForShaderReplace.ContainsKey(go)) return;
        ObjectsForShaderReplace.Add(go, type);
    }

    public static void RegisterGameObjectForMatSwap(GameObject go, bool isJotunnMock = false)
    {
        if (ObjectToSwap.ContainsKey(go)) return;
        ObjectToSwap.Add(go, isJotunnMock);
    }

    private static void GetAllMaterials()
    {
        foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
            OriginalMaterials[material.name] = material;
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void ReplaceAllMaterialsWithOriginal()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null || _hasRun) return;

        if (OriginalMaterials.Count == 0) GetAllMaterials();

        foreach (var kvp in ObjectToSwap)
        {
            var go = kvp.Key;
            var isJotunnMock = kvp.Value;
            ProcessGameObjectMaterials(go, isJotunnMock);
        }

        // Get all assetbundles and find the shaders in them
        var assetBundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
        foreach (var bundle in assetBundles)
        {
            IEnumerable<Shader>? bundleShaders;
            try
            {
                bundleShaders = bundle.isStreamedSceneAssetBundle && bundle
                    ? bundle.GetAllAssetNames().Select(bundle.LoadAsset<Shader>).Where(shader => shader != null)
                    : bundle?.LoadAllAssets<Shader>();
            }
            catch (Exception)
            {
                continue;
            }

            if (bundleShaders == null) continue;
            foreach (var shader in bundleShaders) CachedShaders.Add(shader);
        }

        UnityEngine.Debug.Log($"CachedShaders = {string.Join(",", CachedShaders)}");

        foreach (var kvp in ObjectsForShaderReplace)
        {
            var go = kvp.Key;
            var shaderType = kvp.Value;
            ProcessGameObjectShaders(go, shaderType);
        }

        _hasRun = true;
    }

    private static void ProcessGameObjectMaterials(GameObject go, bool isJotunnMock)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var newMaterials = renderer.sharedMaterials.Select(material => ReplaceMaterial(material, isJotunnMock))
                .ToArray();
            renderer.sharedMaterials = newMaterials;
        }
    }

    private static Material ReplaceMaterial(Material originalMaterial, bool isJotunnMock)
    {
        var replacementPrefix = isJotunnMock ? "JVLmock_" : "_REPLACE_";
        if (!originalMaterial.name.StartsWith(replacementPrefix, StringComparison.Ordinal))
        {
            return originalMaterial;
        }

        var cleanName = originalMaterial.name.Replace(" (Instance)", "").Replace(replacementPrefix, "");
        if (OriginalMaterials.TryGetValue(cleanName, out var replacementMaterial))
        {
            return replacementMaterial;
        }

        Debug.LogWarning($"No suitable material found to replace: {cleanName}");
        return originalMaterial;
    }

    private static void ProcessGameObjectShaders(GameObject go, ShaderType shaderType)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers.Where(x => x.name != "_NOTREPLACE_").ToList())
        foreach (var material in renderer.sharedMaterials.Where(x => x != null).ToList())
        {
            UnityEngine.Debug.Log(
                "[MaterialReplacer::ProcessGameObjectShaders] " +
                    $"renderer={renderer.name}; material={material.name}; shader={material.shader.name}; shaderType={shaderType.ToString()}");
            material.shader = GetShaderForType(material.shader, shaderType, material.shader.name) ?? material.shader;
        }
    }

    private static Shader GetShaderForType(Shader orig, ShaderType shaderType, string originalShaderName)
    {
        switch (shaderType)
        {
            case ShaderType.PieceShader:        return FindShaderWithName(orig, "Custom/Piece");
            case ShaderType.VegetationShader:   return FindShaderWithName(orig, "Custom/Vegetation");
            case ShaderType.RockShader:         return FindShaderWithName(orig, "Custom/StaticRock");
            case ShaderType.RugShader:          return FindShaderWithName(orig, "Custom/Rug");
            case ShaderType.GrassShader:        return FindShaderWithName(orig, "Custom/Grass");
            case ShaderType.CustomCreature:     return FindShaderWithName(orig, "Custom/Creature");
            default:                            return FindShaderWithName(orig, "Standard");
            case ShaderType.UseUnityShader:
                return FindShaderWithName(orig, FindShaderWithName(orig, originalShaderName) != null ? originalShaderName : "ToonDeferredShading2017");
        }
    }

    public static Shader FindShaderWithName(Shader origShader, string name)
    {
        foreach (var shader in CachedShaders)
        {
            if (shader.name != name) continue;
            return shader;
        }


        return origShader;
    }
}