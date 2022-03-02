using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    public BlockType[] blocks;
    public int chunkSize = 16;
    public int chunkHeight = 16;
    public World worldRef;
    public Vector3Int worldPos;
    public ChunkSection[] sections;
    
    public bool modifiedByPlayer = false;
    public TreeData treeData;

    public ChunkData(int chunkSize, int chunkHeight, World worldRef, Vector3Int worldPos)
    {
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.worldRef = worldRef;
        this.worldPos = worldPos;
        blocks = new BlockType[chunkSize * chunkSize * chunkHeight];
        sections = new ChunkSection[worldRef.worldHeight / 16];
        for (int i = 0; i < sections.Length; i++)
        {
            sections[i] = new ChunkSection(this, i*chunkSize);
        }
    }
    
    public static void LoopThroughTheBlocks(ChunkData chunkData, Action<int, int, int> actionToPerform)
    {
        // foreach (var block in chunkData.sections.Select(section => section.blocks))
        // {
        //     
        // }
        for (var index = 0; index < chunkData.blocks.Length; index++)
        {
            var pos = GetPositionFromIndex(chunkData, index);
            actionToPerform(pos.x, pos.y, pos.z);
        }
    }
    
    // private static LocacPositionToGlobal

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
    /// Returns the block at the given position with position being in local space.
    /// </summary>
    /// <param name="pos">This is in chunk coordinates</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public BlockType GetBlock(Vector3Int pos)
    {
        if (IsInRange(this, pos))
        {
            return this.blocks[GetIndexFromPosition(this, pos)];
        }
        
        return this.worldRef.GetBlock(this, new Vector3Int(this.worldPos.x + pos.x, this.worldPos.y + pos.y, this.worldPos.z + pos.z));

    }
    
    public void SetBlock(Vector3Int localPos, BlockType block)
    {
        if (IsInRange(this, localPos))
        {
            this.blocks[GetIndexFromPosition(this, localPos)] = block;
        }
        else
        {
            WorldDataHelper.SetBlock(this.worldRef, localPos + this.worldPos, block);
        }
    }

    /// <summary>
    /// Returns the local position of the block at the given world position.
    /// </summary>
    /// <param name="chunkData"></param>
    /// <param name="worldPos">This is in world coordinates</param>
    /// <returns></returns>
    public Vector3Int GetLocalBlockCoords(Vector3Int worldPos)
    {
        return new Vector3Int
        (
            worldPos.x - this.worldPos.x, 
            worldPos.y - this.worldPos.y, 
            worldPos.z - this.worldPos.z
        );
    }
    
    public static Vector3Int GetGlobalBlockCoords(ChunkData chunkData, Vector3Int localPos)
    {
        return new Vector3Int
        (
            localPos.x + chunkData.worldPos.x, 
            localPos.y + chunkData.worldPos.y, 
            localPos.z + chunkData.worldPos.z
        );
    }

    public MeshData GetMeshData()
    {
        MeshData meshData = new MeshData(true);
        
        LoopThroughTheBlocks(this, (x, y, z) =>
        {
            meshData = BlockHelper.GetMeshData(this, new Vector3Int(x, y, z), meshData,
                this.blocks[GetIndexFromPosition(this, new Vector3Int(x, y, z))]);
        });
        
        
        return meshData;
    }

    public bool IsOnEdge(Vector3Int blockWorldPos)
    {
        var blockLocalPos = GetLocalBlockCoords(blockWorldPos);
        
        return blockLocalPos.x == 0 || blockLocalPos.x == this.chunkSize - 1 ||
               blockLocalPos.y == 0 || blockLocalPos.y == this.chunkHeight - 1 ||
               blockLocalPos.z == 0 || blockLocalPos.z == this.chunkSize - 1;
    }

    public List<ChunkData> GetNeighbourChunk(Vector3Int blockWorldPos)
    {
        var blockLocalPos = GetLocalBlockCoords(blockWorldPos);
        var neighbourChunks = new List<ChunkData>();

        if (blockLocalPos.x == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos - Vector3Int.right));
        }
        
        if (blockLocalPos.x == this.chunkSize - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos + Vector3Int.right));
        }
        
        if (blockLocalPos.y == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos - Vector3Int.up));
        }
        
        if (blockLocalPos.y == this.chunkHeight - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos + Vector3Int.up));
        }
        
        if (blockLocalPos.z == 0)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos - Vector3Int.forward));
        }
        
        if (blockLocalPos.z == this.chunkSize - 1)
        {
            neighbourChunks.Add(WorldDataHelper.GetChunkData(this.worldRef, blockWorldPos + Vector3Int.forward));
        }
        
        return neighbourChunks;
    }
}