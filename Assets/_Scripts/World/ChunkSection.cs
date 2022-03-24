
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSection
{
    public Block[,,] blocks;
    private char[,,] lightMap;
    public int yOffset;
    public ChunkData dataRef;

    public ChunkSection(ChunkData dataRef, int yOffset)
    {
        this.dataRef = dataRef;
        this.yOffset = yOffset;
        blocks = new Block[dataRef.chunkSize, dataRef.chunkHeight, dataRef.chunkSize];
        lightMap = new char[dataRef.chunkSize, dataRef.chunkHeight, dataRef.chunkSize];
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
    
    public void SetBlock(Vector3Int pos, Block block, bool spreadLight = false)
    {
        pos.y -= yOffset;
        block.position = pos;
        block.section = this;

        // if (dataRef.isGenerated && block.type == BlockType.Air)
        // {
        //     // blocks[pos.x,pos.y,pos.z]?.OnBlockDestroyed();
        // }
        blocks[pos.x, pos.y, pos.z] = block;
        if (dataRef.isGenerated)
        {
            // blocks[pos.x,pos.y,pos.z].OnBlockPlaced();
        }
    }
    
    #region Lighting

    // Get the bits XXXX0000
    public int GetSkylight(Vector3Int pos)
    {
        return (lightMap[pos.x, pos.y, pos.z] >> 4) & 0xF;
    }
    
    // Set the bits XXXX0000
    public void SetSunlight(Vector3Int pos, int value)
    {
        lightMap[pos.x, pos.y, pos.z] = (char) ((lightMap[pos.x, pos.y, pos.z] & 0xF) | (value << 4));
    }
    
    // Get the bits 0000XXXX
    public int GetBlockLight(Vector3Int pos)
    {
        return lightMap[pos.x, pos.y, pos.z] & 0xF;
    }
    
    // Set the bits 0000XXXX
    public void SetBlockLight(Vector3Int pos, int value)
    {
        lightMap[pos.x, pos.y, pos.z] = (char) ((lightMap[pos.x, pos.y, pos.z] & 0xF0) | value);
    }
    
    // This will return the highest value of either the block or the sunlight
    public int GetLight(Vector3Int pos)
    {
        return Math.Max(GetSkylight(pos), GetBlockLight(pos));
    }
    
    #endregion
    
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
