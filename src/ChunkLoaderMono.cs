using System.Text;
using ChunkLoader.Config;
using static ChunkLoader.Consts;

namespace ChunkLoader;

public class ChunkLoaderMono : SlowUpdate, Hoverable, Interactable
{
    private const float HoldRepeatInterval = 0.2f;
    private float _lastUseTime = 0;

    private ZNetView? _nview;
    internal EffectList _fuelAddedEffects = new();
    private Renderer? _renderer;

    private Color _workingColor = Color.clear;
    private float _updateTime = -1;
    private bool? _lastVisualState;

    private static ItemDrop? c_fuelItem => ConfigsContainer.FuelItem;
    private static int c_startFuel => ConfigsContainer.StartFuel;
    private static bool c_infiniteFuel => ConfigsContainer.InfiniteFuel;
    private static int m_maxFuel => ConfigsContainer.MaxFuel;
    private static Color c_flashColor => ConfigsContainer.TerrainFlashColor;
    private static bool c_canTurnOff => ConfigsContainer.CanTurnOffLoaders;

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

        _nview.Register(nameof(RPC_AddFuelAmount), RPC_AddFuelAmount);
        _nview.Register<int>(nameof(RPC_ToggleActiveState), RPC_ToggleActiveState);

        _renderer = Utils.FindChild(transform, "SM_cloumn").GetComponent<Renderer>();
        if (!_renderer) Log.Error($"Failed to find renderer inside of ChunkLoader object. Have you modified '{Consts.PrefabName}' prefab?");
        else if (_renderer.material.HasProperty(EmissionColorShaderPropertyID)) _workingColor = _renderer.material.GetColor(EmissionColorShaderPropertyID);
    }

    public override void SUpdate(float time, Vector2i referenceZone)
    {
        if(!_nview || !_nview.IsValid()) return;
        if (Mathf.Approximately(_updateTime, -1) || time >= _updateTime)
        {
            _updateTime = time + 3f;
            UpdateVisuals();
        }
    }

    private void RPC_AddFuelAmount(long sender)
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
    
    public void RPC_ToggleActiveState(long sender, int maybeBool)
    {
        Log.Info($"RPC_ToggleActiveState sender={sender} maybeBoolRaw={maybeBool}");
        if (_nview && _nview.IsValid() && _nview.IsOwner())
        {
            bool isCurrentlyEnabled = _nview.GetZDO().GetInt(ZDOVars.s_state, ZdoEnabled) == ZdoEnabled;
            bool nextState = maybeBool == -1 ? !isCurrentlyEnabled : (maybeBool == 1);
            _nview.GetZDO().Set(ZDOVars.s_state, nextState == true ? ZdoEnabled : ZdoDisabled);
            Log.Info($"RPC_ToggleActiveState isCurrentlyEnabled={isCurrentlyEnabled} nextState={nextState}");

            // TODO: add vfx on active state toogle
            // _toggleOnEffects.Create(transform.position, Quaternion.identity, variant: flag ? 2 : 1);
        }
        UpdateVisuals();
    }

    public string GetHoverName() => PieceLocalNameKey;

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold) return false;

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

        if (c_canTurnOff && !alt)
        {
            _nview.InvokeRPC(nameof(RPC_ToggleActiveState), -1);
            return true;
        }

        if (alt)
        {
            HighlightWorkingZone();
            return true;
        }

        return false;
    }

    private void HighlightWorkingZone()
    {
        var centerPosition = transform.position;
        var centerZone = ZoneSystem.GetZone(centerPosition);
        var flashColor = c_flashColor;
        var flashTime = (float)ConfigsContainer.TerrainFlashTime.TotalSeconds;
        Heightmap.FindHeightmap(centerPosition)?.m_meshRenderer?.Flash(flashColor, Color.white, flashTime);
        if (ConfigsContainer.LoadSurroundingZones)
        {
            foreach (var pos in ((Vector2i[])
                     [
                         new Vector2i(1, 0), new Vector2i(0, 1), new Vector2i(1, 1), new Vector2i(0, -1),
                         new Vector2i(-1, 0), new Vector2i(-1, -1), new Vector2i(1, -1), new Vector2i(-1, 1)
                     ]).Select(x => ZoneSystem.GetZonePos(centerZone + x)))
            {
                Heightmap.FindHeightmap(pos)?.m_meshRenderer?.Flash(flashColor, Color.white, flashTime);
            }
        }
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
        if (item.m_shared.m_name != c_fuelItem.m_itemData.m_shared.m_name)
        {
            user.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize(
                    "$chunkloader_you_better_use_right_fuel", c_fuelItem.m_itemData.m_shared.m_name));
            return false;
        }

        if (Mathf.CeilToInt(_nview.GetZDO().GetFloat(ZDOVars.s_fuel)) >= m_maxFuel)
        {
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", item.m_shared.m_name));
            return true;
        }

        var inventory = user.GetInventory();
        user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", item.m_shared.m_name));
        inventory.RemoveItem(item, 1);
        _nview.InvokeRPC(nameof(RPC_AddFuelAmount));
        return true;

    }

    private bool IsBurning()
    {
        if (!_nview || _nview.m_ghost) return false;
        return IsBurning(_nview.GetZDO());
    }

    public static bool IsBurning(ZDO zdo)
    {
        if (!zdo.IsValid()) return false;
        return IsEnabled(zdo) && (c_infiniteFuel || zdo.GetFloat(ZDOVars.s_fuel) > 0.0);
    }

    private bool IsEnabled()
    {
        if (!_nview || _nview.m_ghost || !_nview.IsValid()) return true;
        return IsEnabled(_nview.GetZDO());
    }

    public static bool IsEnabled(ZDO zdo)
    {
        if (!c_canTurnOff) return true;
        if (!zdo.IsValid()) return true;
        return zdo.GetInt(ZDOVars.s_state, ZdoEnabled) == ZdoEnabled;
    }

    public string GetHoverText()
    {
        if (c_fuelItem == null) return string.Empty;
        if (!_nview || _nview.m_ghost || !_nview.IsValid()) return string.Empty;

        var currentFuel = _nview.GetZDO().GetFloat(ZDOVars.s_fuel);

        string str = Localization.instance.Localize(PieceLocalNameKey);
        if (!c_infiniteFuel) str += $" ($piece_fire_fuel {Mathf.Ceil(currentFuel)}/{m_maxFuel} )";
        if (c_canTurnOff) str += $"\n[<color=yellow><b>$KEY_Use</b></color>] {(IsEnabled() ? "$chunkloader_deactivate" : "$chunkloader_activate")}\n";
        if (!c_infiniteFuel) str += $"[<color=yellow><b>1-8</b></color>] $piece_useitem ({c_fuelItem.m_itemData.m_shared.m_name})";
        str += "\n[<color=yellow><b>$KEY_AltPlace</b></color>] $showChunkArea";

        return Localization.instance.Localize(str);
    }

    private void UpdateVisuals()
    {
        var isBurning = IsBurning();
        if(_lastVisualState == isBurning) return;

        if(!_renderer) return;
        if(!_renderer.material.HasProperty(EmissionColorShaderPropertyID)) return;

        Log.Info("UpdateVisuals");
        if (!isBurning) _renderer.material.SetColor(EmissionColorShaderPropertyID, ChunkLoaderDeactivatedColor * ChunkLoaderDeactivatedEmission);
        else if (_workingColor != Color.clear) _renderer.material.SetColor(EmissionColorShaderPropertyID, _workingColor);
        _lastVisualState = isBurning;
    }

    private void SpawnFuelAddedEffect() => _fuelAddedEffects.Create(transform.position, transform.rotation);
}