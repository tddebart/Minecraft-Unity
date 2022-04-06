using kcp2k;
using Mirror;
using Steamworks;
#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

public class MinecraftNetworkManager : NetworkManager 
#if UNITY_EDITOR
    ,IPreprocessBuildWithReport
#endif
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(SpawnPlayer);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    public void SpawnPlayer(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        var player = Instantiate(playerPrefab, message.position, message.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
        conn.identity.AssignClientAuthority(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        if (LobbyManager.instance.currentLobby.HasValue) LobbyManager.instance.currentLobby.Value.Leave();
    }

    public struct CreatePlayerMessage : NetworkMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        
        public CreatePlayerMessage(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

#if UNITY_EDITOR
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if(transport is KcpTransport)
        {
            Debug.LogError("KcpTransport does not work online make sure to set the transport to FizzyTransport");
        }
    }
    
    #endif
}
