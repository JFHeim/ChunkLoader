using BepInEx;
using BepInEx.Configuration;
using Managers.ServerSync;

// ReSharper disable InconsistentNaming
namespace ChunkLoader.Config;

public partial class ConfigsContainer
{
    private static ConfigsContainer Instance {
        get => _isInitialized ? field : throw new NullReferenceException("ConfigsContainer is not yet initialized");
        set;
    } = null!;
    private static bool _isInitialized = false;
    private static BaseUnityPlugin _plugin = null!;
    private static ConfigSync configSync = null!;
    private static event Action? OnConfigurationChanged;
    private static event Action<FileSystemEventArgs>? OnConfigurationFileDeleted;
    private static DateTime _lastConfigChange = DateTime.MinValue;
    private static readonly HashSet<string> ConfigFilesToWatch = [];

    public static void InitializeConfiguration(BaseUnityPlugin plugin)
    {
        if (_isInitialized)
        {
            Log.Error("ConfigsContainer is already initialized");
            return;
        }

        configSync = new ConfigSync(Consts.ModGuid)
            { DisplayName = Consts.ModName, CurrentVersion = Consts.ModVersion, MinimumRequiredVersion = Consts.ModVersion };

        _plugin = plugin;
        plugin.Config.SaveOnConfigSet = false;
        Instance = new ConfigsContainer();
        ConfigFilesToWatch.Add($"{_plugin.Info.Metadata.GUID}.cfg");
        SetupWatcher();
        plugin.Config.SaveOnConfigSet = true;
        plugin.Config.ConfigReloaded += (_, _) => UpdateConfiguration();
        plugin.Config.Save();

        OnConfigurationChanged += () =>
        {
            Log.Info("Configuration Received");
            Instance.ApplyConfiguration();
            Log.Info("Configuration applied");
        };

        _isInitialized = true;
    }

    private static void SetupWatcher()
    {
        foreach (var fileName in ConfigFilesToWatch.Where(x=> !string.IsNullOrEmpty(x)))
        {
            FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, fileName);
            fileSystemWatcher.Changed += ConfigChanged;
            fileSystemWatcher.Created += ConfigChanged;
            fileSystemWatcher.Deleted += ConfigDeleted;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
    }

    private static void ConfigDeleted(object sender, FileSystemEventArgs eventArgs)
    {
        try { OnConfigurationFileDeleted?.Invoke(eventArgs); }
        catch (Exception ex) { Log.Error(ex, "Configuration error", false); }
    }

    private static void ConfigChanged(object sender, FileSystemEventArgs eventArgs)
    {
        if ((DateTime.Now - _lastConfigChange).TotalSeconds <= 2) return;
        _lastConfigChange = DateTime.Now;

        try { _plugin.Config.Reload(); }
        catch (Exception ex) { Log.Error(ex,"Unable reload config"); }
    }

    private static void UpdateConfiguration()
    {
        try { OnConfigurationChanged?.Invoke(); }
        catch (Exception e) { Log.Error(e, "Configuration error", false); }
    }

    // ReSharper disable once InconsistentNaming
    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        var configEntry = _plugin.Config.Bind(group, name, value, description);

        var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    // ReSharper disable once InconsistentNaming
    public static ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
}