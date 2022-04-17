using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkSectionSaveData
{
    // [SerializeReference]
    public BlockSaveData[] blocks;
    public int yOffset;

    public ChunkSectionSaveData()
    {
        
    }
    
    public ChunkSectionSaveData(ChunkSection section)
    {
        var blocksL = new List<BlockSaveData>(); 
        // Here we iterate through all the blocks in the chunk section
        // and save them with run length encoding
        
        for (var i = 0; i < section.blocks.Length; i++)
        {
            var count = 1;
            while (i + count < section.blocks.Length &&
                   section.blocks[i % section.dataRef.chunkSize, (i/section.dataRef.chunkSize) % section.dataRef.chunkHeight, i/ (section.dataRef.chunkSize * section.dataRef.chunkHeight)].type == 
                   section.blocks[(i+count) % section.dataRef.chunkSize, ((i+count)/section.dataRef.chunkSize) % section.dataRef.chunkHeight, (i+count)/ (section.dataRef.chunkSize * section.dataRef.chunkHeight)].type)
            {
                count++;
            }
            
            blocksL.Add(new BlockSaveData(section.blocks[i % section.dataRef.chunkSize, (i/section.dataRef.chunkSize) % section.dataRef.chunkHeight, i/ (section.dataRef.chunkSize * section.dataRef.chunkHeight)].type, count));
            i += count - 1;
        }
        
        blocks = blocksL.ToArray();
        yOffset = section.yOffset;
    }
}
