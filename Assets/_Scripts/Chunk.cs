
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Chunk
{
    public static void LoopThroughTheBlocks(ChunkData chunkData, Action<int, int, int> actionToPerform)
    {
        for (var index = 0; index < chunkData.blocks.Length; index++)
        {
            var pos = GetPositionFromIndex(chunkData, index);
            actionToPerform(pos.x, pos.y, pos.z);
        }
    }

    private static Vector3Int GetPositionFromIndex(ChunkData chunkData, int index)
    {
        var x = index % chunkData.chunkSize;
        var y = (index / chunkData.chunkSize) % chunkData.chunkHeight;
        var z = index / (chunkData.chunkSize * chunkData.chunkHeight);
        return new Vector3Int(x, y, z);
    }
    
    private static int GetIndexFromPosition(ChunkData chunkData, Vector3Int localPos)
    {
        return localPos.x + chunkData.chunkSize * localPos.y + chunkData.chunkSize * chunkData.chunkHeight * localPos.z;
    }
    

    private static bool IsInRange(ChunkData chunkData, int axisCoord)
    {
        return axisCoord >= 0 && axisCoord < chunkData.chunkSize;
    }

    private static bool IsInRange(ChunkData chunkData, Vector3Int pos)
    {
        return IsInRange(chunkData, pos.x) && IsInRangeHeight(chunkData, pos.y) && IsInRange(chunkData, pos.z);
    }
    
    private static bool IsInRangeHeight(ChunkData chunkData, int axisCoord)
    {
        return axisCoord >= 0 && axisCoord < chunkData.chunkHeight;
    }
    
    /// <summary>
    /// Returns the block at the given position.
    /// </summary>
    /// <param name="chunkData"></param>
    /// <param name="pos">This is in chunk coordinates</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static BlockType GetBlock(ChunkData chunkData, Vector3Int pos)
    {
        if (IsInRange(chunkData, pos))
        {
            return chunkData.blocks[GetIndexFromPosition(chunkData, pos)];
        }
        
        return chunkData.worldRef.GetBlock(chunkData, new Vector3Int(chunkData.worldPos.x + pos.x, chunkData.worldPos.y + pos.y, chunkData.worldPos.z + pos.z));

    }
    
    public static void SetBlock(ChunkData chunkData, Vector3Int localPos, BlockType block)
    {
        if (IsInRange(chunkData, localPos))
        {
            chunkData.blocks[GetIndexFromPosition(chunkData, localPos)] = block;
        }
        else
        {
            WorldDataHelper.SetBlock(chunkData.worldRef, localPos, block);
        }
    }

    /// <summary>
    /// Returns the local position of the block at the given world position.
    /// </summary>
    /// <param name="chunkData"></param>
    /// <param name="pos">This is in world coordinates</param>
    /// <returns></returns>
    public static Vector3Int GetLocalBlockCoords(ChunkData chunkData, Vector3Int pos)
    {
        return new Vector3Int
        (
            pos.x - chunkData.worldPos.x, 
            pos.y - chunkData.worldPos.y, 
            pos.z - chunkData.worldPos.z
        );
    }

    public static MeshData GetChunkMeshData(ChunkData chunkData)
    {
        MeshData meshData = new MeshData(true);
        
        LoopThroughTheBlocks(chunkData, (x, y, z) =>
        {
            meshData = BlockHelper.GetMeshData(chunkData, new Vector3Int(x,y,z), meshData, chunkData.blocks[GetIndexFromPosition(chunkData, new Vector3Int(x,y,z))]);
        });
        
        
        return meshData;
    }

    public static Vector3Int ChunkPosFromBlockCoords(World world, Vector3Int pos)
    {
        Vector3Int chunkPos = new Vector3Int(
            Mathf.FloorToInt(pos.x / (float)world.chunkSize) * world.chunkSize,
            Mathf.FloorToInt(pos.y / (float)world.chunkHeight) * world.chunkHeight,
            Mathf.FloorToInt(pos.z / (float)world.chunkSize) * world.chunkSize
        );
        return chunkPos;
    }

    public static bool IsOnEdge(ChunkData chunkData, Vector3Int blockWorldPos)
    {
        var blockLocalPos = GetLocalBlockCoords(chunkData, blockWorldPos);
        
        return blockLocalPos.x == 0 || blockLocalPos.x == chunkData.chunkSize - 1 ||
               blockLocalPos.y == 0 || blockLocalPos.y == chunkData.chunkHeight - 1 ||
               blockLocalPos.z == 0 || blockLocalPos.z == chunkData.chunkSize - 1;
    }

    public static List<ChunkData> GetNeighbourChunk(ChunkData chunkData, Vector3Int blockWorldPos)
    {
        var blockLocalPos = GetLocalBlockCoords(chunkData, blockWorldPos);
        var neighbourChunks = new List<ChunkData>();

        if (blockLocalPos.x == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos - Vector3Int.right));
        }
        
        if (blockLocalPos.x == chunkData.chunkSize - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos + Vector3Int.right));
        }
        
        if (blockLocalPos.y == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos - Vector3Int.up));
        }
        
        if (blockLocalPos.y == chunkData.chunkHeight - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos + Vector3Int.up));
        }
        
        if (blockLocalPos.z == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos - Vector3Int.forward));
        }
        
        if (blockLocalPos.z == chunkData.chunkSize - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(chunkData.worldRef, blockWorldPos + Vector3Int.forward));
        }
        
        return neighbourChunks;
    }
}
