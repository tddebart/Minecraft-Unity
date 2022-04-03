using Mirror;
using UnityEngine;

public class WorldServer : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<SetBlockMessage>(SetBlockMessageHandler, false);
    }

    
    public void SetBlockMessageHandler(NetworkConnectionToClient conn, SetBlockMessage message)
    {
        RpcSetBlock(message.position, message.blockType);
    }
    
    [ClientRpc]
    public void RpcSetBlock(Vector3Int position, BlockType blockType)
    {
        World.Instance.SetBlock(position, blockType);
    }

    public struct SetBlockMessage : NetworkMessage
    {
        public Vector3Int position;
        public BlockType blockType;
        
        public SetBlockMessage(Vector3Int position, BlockType blockType)
        {
            this.position = position;
            this.blockType = blockType;
        }
    }
}