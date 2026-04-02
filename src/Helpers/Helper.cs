using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChunkLoader.Helpers;

public static class Helper
{
    public static bool IsMainScene()
    {
        var scene = SceneManager.GetActiveScene();
        var isMainScene = scene.IsValid() && scene.name == Consts.MainSceneName;
        return isMainScene;
    }

    public static bool IsServer()
    {
        var gameState = GetGameServerClientState();
        return gameState is GameServerClientState.Server;
    }

    public static bool IsDedicatedServer()
    {
        var gameState = GetGameServerClientState();
        return gameState is GameServerClientState.DedicatedServer;
    }

    public static GameServerClientState GetGameServerClientState()
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return GameServerClientState.DedicatedServer;

        return ZNet.instance?.IsServer() switch
        {
            null => GameServerClientState.Unknown,
            false => GameServerClientState.Client,
            true => GameServerClientState.Server
        };
    }
}

public enum GameServerClientState
{
    Unknown,
    Client,
    Server,
    DedicatedServer
}