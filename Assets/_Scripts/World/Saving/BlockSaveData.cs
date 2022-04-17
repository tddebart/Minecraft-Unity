using System;
using UnityEngine;

[Serializable]
public class BlockSaveData
{
    public BlockType type;
    public int length;
    
    public BlockSaveData(BlockType type, int length)
    {
        this.type = type;
        this.length = length;
    }
    
    public BlockSaveData()
    {
    }
}
