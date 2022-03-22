using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ChunkData
{
    public int chunkSize = 16;
    public int chunkHeight = 16;
    public World worldRef;
    public readonly Vector3Int worldPos;
    public ChunkSection[] sections;
    
    public bool modifiedByPlayer = false;
    public bool isGenerated = false;
    public TreeData treeData;

    public ChunkData(int chunkSize, int chunkHeight, World worldRef, Vector3Int worldPos)
    {
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.worldRef = worldRef;
        this.worldPos = worldPos;
        sections = new ChunkSection[worldRef.worldHeight / 16];
        for (int i = 0; i < sections.Length; i++)
        {
            sections[i] = new ChunkSection(this, i*this.chunkHeight);
        }
    }
    
    public static void LoopThroughTheBlocks(ChunkData chunkData, Action<int, int, int, BlockType> actionToPerform)
    {
        foreach (var section in chunkData.sections.Where(s => s.blocks.Cast<Block>().Any(b => b.type != BlockType.Air)))
        {
            foreach (var block in section.blocks)
            {
                actionToPerform(block.position.x, block.position.y+section.yOffset, block.position.z, block.type);
            }
        }
    }

    public ChunkSection GetSection(int y)
    {
        return sections[Mathf.FloorToInt(y / chunkHeight)];
    }


    private static bool IsInRange(ChunkData chunkData, int axisCoord)
    {
        return axisCoord >= 0 && axisCoord < chunkData.chunkSize;
    }

    private static bool IsInRange(ChunkData chunkData, Vector3Int pos)
    {
        var x = pos.x;
        var y = pos.y;
        var z = pos.z;
        
        return x>= 0 && x < chunkData.chunkSize && y >= 0 && y < chunkData.worldRef.worldHeight && z >= 0 && z < chunkData.chunkSize;
    }
    
    private static bool IsInRangeHeight(ChunkData chunkData, int axisCoord)
    {
        return axisCoord >= 0 && axisCoord < chunkData.worldRef.worldHeight;
    }

    /// <summary>
    /// Returns the block at the given position with position being in local space.
    /// </summary>
    /// <param name="localPos">This is in chunk coordinates except the y axis</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Block GetBlock(Vector3Int localPos)
    {
        var y = localPos.y;
        if (localPos.y < 0 || localPos.y >= worldRef.worldHeight)
        {
            return Blocks.NOTHING;
        }
        
        if (IsInRange(this, localPos))
        {
            return GetSection(localPos.y).GetBlock(localPos);
        }
        
        return this.worldRef.GetBlock(new Vector3Int(this.worldPos.x + localPos.x, this.worldPos.y + localPos.y, this.worldPos.z + localPos.z));
    }

    public void SetBlock(Vector3Int localPos, BlockType block)
    {
        if (localPos.y < 0 || localPos.y >= worldRef.worldHeight)
        {
            return;
        }
        
        if (IsInRange(this, localPos))
        {
            GetSection(localPos.y).SetBlock(localPos, block);
        }
        else
        {
            WorldDataHelper.SetBlock(this.worldRef, localPos + this.worldPos, block);
        }
    }

    public void SetBlock(Vector3Int localPos, Block block, bool updateChunk = false)
    {
        if (IsInRange(this, localPos))
        {
            GetSection(localPos.y).SetBlock(localPos, block);
            if (updateChunk)
            {
                var chunkPos = WorldDataHelper.GetChunkPosition(worldRef, localPos + this.worldPos);
                WorldDataHelper.GetChunk(worldRef, chunkPos)?.UpdateChunk();
            }
        }
        else
        {
            WorldDataHelper.SetBlock(this.worldRef, localPos + this.worldPos, block, updateChunk);
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
    
    public Vector3Int GetGlobalBlockCoords(Vector3Int localPos)
    {
        return new Vector3Int
        (
            localPos.x + worldPos.x, 
            localPos.y + worldPos.y, 
            localPos.z + worldPos.z
        );
    }

    public MeshData GetMeshData()
    {
        MeshData meshData = new MeshData(true);
        
        CalculateLights();
        
        LoopThroughTheBlocks(this, (x, y, z,block) =>
        {
            meshData = BlockHelper.GetMeshData(this, new Vector3Int(x, y, z), meshData, block);
        });
        
        
        return meshData;
    }

    public void CalculateLights()
    {
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        for (var x = 0; x < chunkSize; x++)
        {
            for (var z = 0; z < chunkSize; z++)
            {
                var lightRay = 1f;
                
                for (var y = worldRef.worldHeight-1; y >= 0; y--)
                {
                    var block = GetBlock(new Vector3Int(x, y, z));
                    var transparency = BlockDataManager.textureDataDictionary[(int)block.type].transparency;
                    if (block.type is not BlockType.Air and not BlockType.Nothing && transparency < lightRay)
                    {
                        lightRay = transparency;
                    }
                    if (block.type == BlockType.GlowStone)
                    {
                        lightRay = 1f;
                    }
                    
                    block.globalLightPercent = lightRay;

                    if (lightRay > worldRef.lightFalloff)
                    {
                        litBlocks.Enqueue(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        while (litBlocks.Count > 0)
        {
            var blockPos = litBlocks.Dequeue();
            
            foreach (var dir in BlockHelper.directions)
            {
                var block = GetBlock(blockPos);
                var neighborPos = blockPos + dir.GetVector();
                if (IsInRange(this, neighborPos))
                {
                    var neighborBlock = GetBlock(neighborPos);
                    if (neighborBlock.type == BlockType.Air && neighborBlock.globalLightPercent < block.globalLightPercent - worldRef.lightFalloff)
                    {
                        neighborBlock.globalLightPercent = block.globalLightPercent - worldRef.lightFalloff;
            
                        if (neighborBlock.globalLightPercent > worldRef.lightFalloff)
                        {
                            litBlocks.Enqueue(neighborPos);
                        }
                    }
                }
            }
        }
        
    }

    public bool IsOnEdge(Vector3Int blockWorldPos)
    {
        var blockLocalPos = GetLocalBlockCoords(blockWorldPos);
        
        return blockLocalPos.x == 0 || blockLocalPos.x == this.chunkSize - 1 ||
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