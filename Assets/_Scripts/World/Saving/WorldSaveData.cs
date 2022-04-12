using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class WorldSaveData
{
    public string worldName;
    
    public Vector3Int seedOffset;
    
    [NonSerialized]
    public ChunkSaveData[] chunks;
    
    public Dictionary<Vector3Int, ChunkSaveData> chunksDic = new ();
    
    public ChunkSaveData RequestChunkData(Vector3Int chunkPos)
    {
        if (chunksDic.ContainsKey(chunkPos))
        {
            return chunksDic[chunkPos];
        }
        else
        {
            return null;
        }
    }
}
