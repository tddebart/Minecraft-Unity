using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public partial class World
{
    public Queue<Block> blockToUpdate = new Queue<Block>();
    [HideInInspector]
    public List<ChunkRenderer> chunksToUpdate = new List<ChunkRenderer>();
    public Queue<ChunkRenderer.MeshChunkObject> chunksToUpdateMesh = new();
    public object chunkUpdateThreadLock = new object();
    private Thread updateThread;

    public void UpdateLoop()
    {
        while (!disabled)
        {
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    public void UpdateChunks()
    {
        lock (chunkUpdateThreadLock)
        {
            chunksToUpdate[0]?.UpdateChunkAsync();
            chunksToUpdate.RemoveAt(0);
        }
    }
    
    public void AddChunkToUpdate(ChunkRenderer chunk, bool first = false)
    {
        lock (chunkUpdateThreadLock)
        {
            if(!chunksToUpdate.Contains(chunk))
            {
                if (first)
                {
                    chunksToUpdate.Insert(0, chunk);
                }
                else
                {
                    chunksToUpdate.Add(chunk);
                }
                
            }
        }
    }

    private void Update()
    {
        //TODO: remove this because it is slow
        Camera.main.backgroundColor = Color.Lerp(nightColor,dayColor , skyLightMultiplier);
        
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     WorldServer.instance.RequestChunk(Vector3Int.zero);
        // }

        if (chunksToUpdateMesh.Count > 0)
        {
            var data = chunksToUpdateMesh.Dequeue();
            data.chunk.RenderMesh(data.meshData);
        }
    }

    public void UpdateBlocks()
    {
        while (blockToUpdate.Count > 0)
        {
            Block block = blockToUpdate.Dequeue();
            block.OnBlockUpdate();
        }
    }
}
