
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
    /// <param name="block">The block to set</param>
    public void SetBlock(Vector3Int pos, BlockType block)
    {
        var localBlockPos = new Vector3Int(pos.x, pos.y - yOffset, pos.z);
        blocks[localBlockPos.x,localBlockPos.y,localBlockPos.z] = new Block(block, localBlockPos, this);
    }
}
