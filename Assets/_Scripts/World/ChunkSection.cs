using System;
using UnityEngine;
using Random = System.Random;

public class ChunkSection
{
    public Block[,,] blocks;
    private char[,,] lightMap;
    public int yOffset;
    public ChunkData dataRef;

    public ChunkSection(ChunkData dataRef, int yOffset, BlockType populate = BlockType.Nothing)
    {
        this.dataRef = dataRef;
        this.yOffset = yOffset;
        blocks = new Block[dataRef.chunkSize, dataRef.chunkHeight, dataRef.chunkSize];
        lightMap = new char[dataRef.chunkSize, dataRef.chunkHeight, dataRef.chunkSize];
        Populate(populate);
    }

    public static ChunkSection Deserialize(ChunkSectionSaveData saveData, ChunkData dataRef)
    {
        var chunkSection = new ChunkSection(dataRef, saveData.yOffset, BlockType.Air);
        
        foreach (var blockData in saveData.blocks)
        {
            try
            {
                var pos = blockData.position;
                chunkSection.blocks[pos.x, pos.y, pos.z] =  new Block(blockData.type, blockData.position, chunkSection);
                chunkSection.blocks[pos.x, pos.y, pos.z].Loaded();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        return chunkSection;
    }
    
    // This will populate the chunk with nothing blocks
    private void Populate(BlockType type)
    {
        for (int x = 0; x < dataRef.chunkSize; x++)
        {
            for (int y = 0; y < dataRef.chunkHeight; y++)
            {
                for (int z = 0; z < dataRef.chunkSize; z++)
                {
                    blocks[x, y, z] = new Block(type, new Vector3Int(x, y, z), this);
                }
            }
        }
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
    /// <param name="type">The block type to set</param>
    public void SetBlock(Vector3Int pos, BlockType type)
    {
        pos.y -= yOffset;
        blocks[pos.x, pos.y, pos.z].SetType(type);
    }
    
    // public void SetBlock(Vector3Int pos, Block block)
    // {
    //     pos.y -= yOffset;
    //     block.section = this;
    //     block.position = pos;
    //
    //     // if (dataRef.isGenerated && block.type == BlockType.Air)
    //     // {
    //     //     // blocks[pos.x,pos.y,pos.z]?.OnBlockDestroyed();
    //     // }
    //     blocks[pos.x, pos.y, pos.z] = block;
    //     if (dataRef.isGenerated)
    //     {
    //         // blocks[pos.x,pos.y,pos.z].OnBlockPlaced();
    //     }
    // }
    
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
