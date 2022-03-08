
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSection
{
    public Block[,,] blocks;
    public int yOffset;
    public ChunkData dataRef;

    public ChunkSection(ChunkData dataRef, int yOffset)
    {
        this.dataRef = dataRef;
        this.yOffset = yOffset;
        blocks = new Block[dataRef.chunkSize, dataRef.chunkSize, dataRef.chunkSize];
    }

    /// <summary>
    /// Returns the block at the given position
    /// </summary>
    /// <param name="pos">The x and z should be local and the y global</param>
    /// <returns></returns>
    public Block GetBlock(Vector3Int pos)
    {
        return blocks[pos.x, pos.y - yOffset, pos.z];
    }
    
    
    /// <summary>
    ///
    /// </summary>
    /// <param name="pos">The x and z should be local and the y global</param>
    /// <param name="block">The block type to set</param>
    public void SetBlock(Vector3Int pos, BlockType block)
    {
        SetBlock(pos, new Block(block));
    }
    
    public void SetBlock(Vector3Int pos, Block block)
    {
        pos.y -= yOffset;
        block.position = pos;
        block.section = this;
        if (block.type == BlockType.Air)
        {
            blocks[pos.x,pos.y,pos.z]?.OnBlockDestroyed();
        }
        blocks[pos.x, pos.y, pos.z] = block;
        blocks[pos.x,pos.y,pos.z].OnBlockPlaced();
    }
    
    public Vector3Int GetGlobalBlockCoords(Vector3Int localPos)
    {
        return new Vector3Int
        (
            localPos.x + dataRef.worldPos.x, 
            localPos.y + yOffset, 
            localPos.z + dataRef.worldPos.z
        );
    }
    
}
