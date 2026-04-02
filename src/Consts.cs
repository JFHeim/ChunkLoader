namespace ChunkLoader;

public static class Consts
{
    public const string
        ModName = "ChunkLoader",
        ModAuthor = "Frogger",
        ModVersion = "1.7.0",
        ModGuid = $"com.{ModAuthor}.{ModName}";

    public const string MainSceneName = "main";
    public const string PrefabName = "ChunkLoader_stone";
    public static readonly int PrefabNameHash = "ChunkLoader_stone".GetStableHashCode();
    public const string DefaultFuel = "Thunderstone";
    public const string PieceLocalNameKey = "$piece_ChunkLoader_stone";
    public static readonly int EmissionColorShaderPropertyID = Shader.PropertyToID("_EmissionColor");
    public static readonly Color ChunkLoaderDeactivatedColor = Color.red;
    public const float ChunkLoaderDeactivatedEmission = 6;
}
