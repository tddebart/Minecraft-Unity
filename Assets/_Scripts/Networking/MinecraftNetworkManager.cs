using System;
using System.IO;
using kcp2k;
using Mirror;
using Steamworks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinecraftNetworkManager : NetworkManager
{
    [Space] 
    public GameObject WorldServerPrefab;


    public override void OnClientConnect()
    {
        base.OnClientConnect();
        NetworkClient.Send(new WorldServer.StartPlayerMessage(SteamClient.SteamId));
        
        // NetworkClient.Send(new WorldServer.SpawnPlayerMessage(SteamClient.SteamId));
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (SceneManager.GetActiveScene().name == "World")
        {
            OnServerSceneChanged("World");
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.Contains("World"))
        {
            var worldServer = Instantiate(WorldServerPrefab);
            NetworkServer.Spawn(worldServer);
        }
    }

    public override void OnClientDisconnect()
    {
        if (LobbyManager.currentLobby.HasValue)
        {
            LobbyManager.currentLobby.Value.Leave();
        }
        base.OnClientDisconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        WorldServer.instance.SavePlayerMessageHandler(conn, conn.identity.GetComponent<Player>().SavePlayer());
        
        base.OnServerDisconnect(conn);
    }
}
