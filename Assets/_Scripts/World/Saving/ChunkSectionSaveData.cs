using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkSectionSaveData
{
    [SerializeReference]
    public Block[] blocks;
    public int yOffset;
    
    public ChunkSectionSaveData(ChunkSection section)
    {
        var blocksL = new List<Block>(); 
        for (var x = 0; x < section.blocks.GetLength(0); x++)
        {
            for (var y = 0; y < section.blocks.GetLength(1); y++)
            {
                for (var z = 0; z < section.blocks.GetLength(2); z++)
                {
                    if(section.blocks[x,y,z].type is BlockType.Air or BlockType.Nothing)
                        continue;
                    
                    blocksL.Add(section.blocks[x, y, z]);
                }
            }
        }
        blocks = blocksL.ToArray();
        yOffset = section.yOffset;
    }
}
