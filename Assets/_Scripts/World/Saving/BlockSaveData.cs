using System;
using UnityEngine;

[Serializable]
public class BlockSaveData
{
    public Vector3Int position;
    public BlockType type;
    
    public BlockSaveData(Vector3Int position, BlockType type)
    {
        this.position = position;
        this.type = type;
    }
    
    public BlockSaveData()
    {
    }
}
