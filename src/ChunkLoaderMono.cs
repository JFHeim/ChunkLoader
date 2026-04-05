using System.Text;
using ChunkLoader.Config;
using static ChunkLoader.Consts;

namespace ChunkLoader;

public class ChunkLoaderMono : SlowUpdate, Hoverable, Interactable
{
    private const float HoldRepeatInterval = 0.2f;
    private const float LastUseTime = 0;

    private ZNetView? _nview;
    internal EffectList m_fuelAddedEffects = new();
    private Renderer? _renderer;

    private Color _workingColor = Color.clear;
    private float _updateTime;
    private readonly StringBuilder _sb = new();

    private static ItemDrop? c_fuelItem => ConfigsContainer.FuelItem;
    private static int c_startFuel => ConfigsContainer.StartFuel;
    private static bool c_infiniteFuel => ConfigsContainer.InfiniteFuel;
    private static int m_maxFuel => ConfigsContainer.MaxFuel;
    private static Color c_flashColor => ConfigsContainer.TerrainFlashColor;

    private void Start() => _updateTime = Time.time + 10f;

    public override void Awake()
    {
        base.Awake();
        _nview = gameObject.GetComponent<ZNetView>();
        if (!_nview || _nview.m_ghost) return;
        if (_nview.GetZDO() == null) return;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_nview.IsOwner() && _nview.GetZDO().GetFloat(ZDOVars.s_fuel, -1f) == -1.0)
        {
            _nview.GetZDO().Set(ZDOVars.s_fuel, (float)c_startFuel);
            if (c_startFuel > 0) SpawnFuelAddedEffect();
        }

        _nview.Register("AddFuel", RPC_AddFuel);

        _renderer = Utils.FindChild(transform, "SM_cloumn").GetComponent<Renderer>();
        if (!_renderer) Log.Error($"Failed to find renderer inside of ChunkLoader object. Have you modified '{Consts.PrefabName}' prefab?");
        else if (_renderer.material.HasProperty(EmissionColorShaderPropertyID)) _workingColor = _renderer.material.GetColor(EmissionColorShaderPropertyID);
    }

    public override void SUpdate(float time, Vector2i referenceZone)
    {
        if(!_nview) return;

        if (!_nview.IsValid() || time > _updateTime) return;
        _updateTime = time + 10f;
        UpdateVisuals();
    }

    private void RPC_AddFuel(long sender)
    {
        if(!_nview) return;
        if (!_nview.IsOwner()) return;

        var currentFuel = _nview.GetZDO().GetFloat(ZDOVars.s_fuel);
        if (Mathf.CeilToInt(currentFuel) >= m_maxFuel) return;

        var newFuel = Mathf.Clamp(Mathf.Clamp(currentFuel, 0, m_maxFuel) + 1f, 0, m_maxFuel);
        _nview.GetZDO().Set(ZDOVars.s_fuel, newFuel);

        SpawnFuelAddedEffect();
        UpdateVisuals();
    }

    public string GetHoverName() => PieceLocalNameKey;

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (!_nview)
        {
            Log.Error($"ZNetView is null, this should not happen. Have you modified '{PrefabName}' prefab in a some way?");
            return false;
        }

        if (c_fuelItem == null)
        {
            Log.Error($"FuelItem is null, this should not happen. Have you deleted '{DefaultFuel}' item from ObjectDB");
            return false;
        }

        if (hold && (HoldRepeatInterval <= 0.0 || Time.time - LastUseTime < HoldRepeatInterval)) return false;
        if (!_nview.HasOwner()) _nview.ClaimOwnership();
        if (alt)
        {
            var centerPosition = transform.position;
            var centerZone = ZoneSystem.GetZone(centerPosition);
            var flashColor = c_flashColor;
            Heightmap.FindHeightmap(centerPosition).m_meshRenderer.Flash(flashColor, Color.white, 1.5f);
            if (ConfigsContainer.LoadSurroundingZones)
            {
                foreach (var pos in ((Vector2i[])
                         [
                             new Vector2i(1, 0), new Vector2i(0, 1), new Vector2i(1, 1), new Vector2i(0, -1),
                             new Vector2i(-1, 0), new Vector2i(-1, -1), new Vector2i(1, -1), new Vector2i(-1, 1)
                         ]).Select(x => ZoneSystem.GetZonePos(centerZone + x)))
                {
                    Heightmap.FindHeightmap(pos).m_meshRenderer.Flash(flashColor, Color.white, 1.5f);
                }
            }

            return true;
        }

        var inventory = user.GetInventory();
        if (inventory == null) return true;
        if (c_infiniteFuel) return false;
        if (inventory.HaveItem(c_fuelItem.m_itemData.m_shared.m_name))
        {
            if (Mathf.CeilToInt(_nview.GetZDO().GetFloat(ZDOVars.s_fuel)) >= m_maxFuel)
            {
                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", c_fuelItem.m_itemData.m_shared.m_name));
                return false;
            }

            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", c_fuelItem.m_itemData.m_shared.m_name));
            inventory.RemoveItem(c_fuelItem.m_itemData.m_shared.m_name, 1);
            _nview.InvokeRPC("AddFuel");
            return true;
        }

        user.Message(MessageHud.MessageType.Center, "$msg_outof " + c_fuelItem.m_itemData.m_shared.m_name);
        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        if (!_nview)
        {
            Log.Error($"ZNetView is null, this should not happen. Have you modified '{PrefabName}' prefab in a some way?");
            return false;
        }

        if (c_fuelItem == null)
        {
            Log.Error($"FuelItem is null, should not happen. Have you deleted '{DefaultFuel}' item from ObjectDB?");
            return false;
        }

        if (c_infiniteFuel) return false;
        if (item.m_shared.m_name != c_fuelItem.m_itemData.m_shared.m_name) return false;

        if (Mathf.CeilToInt(_nview.GetZDO().GetFloat(ZDOVars.s_fuel)) >= m_maxFuel)
        {
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", item.m_shared.m_name));
            return true;
        }

        var inventory = user.GetInventory();
        user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", item.m_shared.m_name));
        inventory.RemoveItem(item, 1);
        _nview.InvokeRPC("AddFuel");
        return true;

    }

    private bool IsBurning()
    {
        if (!_nview || _nview.m_ghost) return false;
        return c_infiniteFuel || IsBurning(_nview.GetZDO());
    }

    public static bool IsBurning(ZDO zdo) => c_infiniteFuel || zdo.GetFloat(ZDOVars.s_fuel) > 0.0;

    public string GetHoverText()
    {
        if (c_fuelItem == null) return string.Empty;
        if (!_nview || _nview.m_ghost || !_nview.IsValid()) return string.Empty;

        _sb.Clear();
        _sb.Append(PieceLocalNameKey);
        if (!c_infiniteFuel)
        {
            _sb.Append(" ( $piece_fire_fuel ");
            _sb.Append(Mathf.Ceil(_nview.GetZDO().GetFloat(ZDOVars.s_fuel)));
            _sb.Append('/');
            _sb.Append(m_maxFuel);
            _sb.Append(" )\n");
            _sb.Append("[<color=yellow><b>$KEY_Use</b></color>] $piece_use ");
            _sb.Append(c_fuelItem.m_itemData.m_shared.m_name);
            _sb.Append("\n");
            _sb.Append("[<color=yellow><b>1-8</b></color>] $piece_useitem");
        }

        _sb.AppendLine();
        _sb.Append("[<color=yellow><b>$KEY_AltPlace</b></color>] $showChunkArea");
        return Localization.instance.Localize(_sb.ToString());
    }

    private void UpdateVisuals()
    {
        if(!_renderer) return;
        if(!_renderer.material.HasProperty(EmissionColorShaderPropertyID)) return;

        if (!IsBurning()) _renderer.material.SetColor(EmissionColorShaderPropertyID, ChunkLoaderDeactivatedColor * ChunkLoaderDeactivatedEmission);
        else if (_workingColor != Color.clear) _renderer.material.SetColor(EmissionColorShaderPropertyID, _workingColor);
    }

    private void SpawnFuelAddedEffect() => m_fuelAddedEffects.Create(transform.position, transform.rotation);
}