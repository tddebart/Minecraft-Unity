using Mirror;
using Steamworks;
using UnityEngine;

public class MinecraftNetworkManager : NetworkManager
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
        MainLobbyManager.currentLobby.Leave();
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
}
