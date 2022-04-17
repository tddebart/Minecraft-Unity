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
        for (var x = 0; x < section.blocks.GetLength(0); x++)
        {
            for (var y = 0; y < section.blocks.GetLength(1); y++)
            {
                for (var z = 0; z < section.blocks.GetLength(2); z++)
                {
                    if(section.blocks[x,y,z].type is BlockType.Air or BlockType.Nothing)
                        continue;

                    var block = section.blocks[x, y, z];
                    blocksL.Add(new BlockSaveData(block.position, block.type));
                }
            }
        }
        blocks = blocksL.ToArray();
        yOffset = section.yOffset;
    }
}
