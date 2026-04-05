using ChunkLoader.Config;
using Managers.PieceManager;

namespace ChunkLoader;

[BepInEx.BepInPlugin(Consts.ModGuid, Consts.ModName, Consts.ModVersion)]
[BepInEx.BepInDependency("com.Frogger.NoUselessWarnings", BepInEx.BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    public static Plugin? Instance { get; private set; }
    private Harmony _harmony = null!;

    internal Sprite? _iconActive = null;
    internal Sprite? _iconDisabled = null;

    private void Awake()
    {
        Instance = this;
        Log.Initialize(this);

        _harmony = new Harmony(Consts.ModGuid);
        _harmony.PatchAll();
        ConfigsContainer.InitializeConfiguration(this);

        Managers.LocalizationManager.Localizer.Load();
        AddPiece();
    }

    private void AddPiece()
    {
        Log.Info($"Adding piece {Consts.PrefabName}");
        var assetBundle = PiecePrefabManager.RegisterAssetBundle("chunkloader");
        var piece = new BuildPiece(assetBundle, Consts.PrefabName);
        piece.Prefab.AddComponent<ChunkLoaderMono>();
        piece.Category.Set(BuildPieceCategory.Misc);
        piece.Crafting.Set(CraftingTable.Forge);
        piece.RequiredItems.Add("Stone", 25, true);
        piece.RequiredItems.Add("Thunderstone", 5, true);
        piece.RequiredItems.Add("SurtlingCore", 1, true);

        _iconActive = assetBundle.LoadAsset<Sprite>("icon_active");
        _iconDisabled = assetBundle.LoadAsset<Sprite>("icon_disabled");
    }
}