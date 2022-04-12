using System;
using UnityEngine;

[Serializable]
public class ChunkSaveData
{
    public Vector3Int position;
    
    public ChunkSectionSaveData[] sections;

    public static ChunkSaveData Serialize(ChunkData chunk) 
    {
        var chunkSaveData = new ChunkSaveData();
        chunkSaveData.position = chunk.worldPos;
        chunkSaveData.sections = new ChunkSectionSaveData[chunk.sections.Length];
        for (int i = 0; i < chunkSaveData.sections.Length; i++)
        {
            chunkSaveData.sections[i] = new ChunkSectionSaveData(chunk.sections[i]);
        }
        
        chunk.modifiedAfterSave = false;
        
        return chunkSaveData;
    }
}
