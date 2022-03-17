using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Queue<ChunkRenderer> chunkPool = new();

    public ChunkRenderer RenderChunk(WorldData worldData, Vector3Int pos, MeshData meshData)
    {
        ChunkRenderer newChunk;
        if(chunkPool.Count > 0)
        {
            newChunk = chunkPool.Dequeue();
            newChunk.transform.position = pos+new Vector3(0.5f,0.5f,0.5f);
        }
        else
        {
            var chunkObj = Instantiate(chunkPrefab, pos+new Vector3(0.5f,0.5f,0.5f), Quaternion.identity, transform);
            newChunk = chunkObj.GetComponent<ChunkRenderer>();
            
        }
        
        newChunk.Initialize(worldData.chunkDataDict[pos]);
        newChunk.UpdateChunk(meshData);
        newChunk.gameObject.SetActive(true);
        
        return newChunk;
    }
    
    public void RemoveChunk(ChunkRenderer chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }
}