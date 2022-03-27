using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

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
    [CanBeNull] public ChunkRenderer renderer => WorldDataHelper.GetChunk(worldRef, worldPos);
    
    public readonly Queue<BlockLightNode> blockLightUpdateQueue = new(); 
    public readonly Queue<BlockLightNode> blockLightRemoveQueue = new(); 
    public List<ChunkRenderer> chunkToUpdateAfterLighting = new List<ChunkRenderer>();

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

    public static bool IsInRange(Vector3Int pos)
    {
        var x = pos.x;
        var y = pos.y;
        var z = pos.z;

        var world = World.Instance;
        
        return x>= 0 && x < world.chunkSize && y >= 0 && y < world.worldHeight && z >= 0 && z < world.chunkSize;
    }
    
    private static bool IsInRangeHeight(int axisCoord)
    {
        return axisCoord >= 0 && axisCoord < World.Instance.worldHeight;
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
            return BlockHelper.NOTHING;
        }
        
        if (IsInRange(localPos))
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
        
        if (IsInRange(localPos))
        {
            GetSection(localPos.y).SetBlock(localPos, block);
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
        
        Lighting.CalculateLight(this);

        LoopThroughTheBlocks(this, (x, y, z,block) =>
        {
            meshData = BlockHelper.GetMeshData(this, new Vector3Int(x, y, z), meshData, block);
        });
        
        
        return meshData;
    }

    public void CalculateBlockLight()
    {
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();
    
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < worldRef.worldHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    var block = GetBlock(new Vector3Int(x, y, z));
                    if (block.BlockData.lightEmission > 0)
                    {
                        block.SetBlockLight(block.BlockData.lightEmission);
                        litBlocks.Enqueue(new Vector3Int(x, y, z));
                    }
                    else
                    {
                        block.SetBlockLight(0);
                    }
                }
            }
        }
        
        while (litBlocks.Count > 0)
        {
            var blockPos = litBlocks.Dequeue();
            var block = GetBlock(blockPos);
            var lightLevel = block.GetBlockLight();
            
            foreach (var direction in BlockHelper.directions)
            {
                var neighborPos = blockPos + direction.GetVector();
                if (IsInRange(neighborPos))
                {
                    var neighborBlock = GetBlock(neighborPos);
                    if (neighborBlock.BlockData.opacity < 15 && neighborBlock.GetBlockLight() < lightLevel - 1)
                    {
                        neighborBlock.SetBlockLight(lightLevel-1);
                        litBlocks.Enqueue(neighborPos);
                    }
                }
            }
        }
    }

    public void CalculateSunLight()
    {
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();
    
        for (var x = 0; x < chunkSize; x++)
        {
            for (var z = 0; z < chunkSize; z++)
            {
                var lightRay = 15;
                
                for (var y = worldRef.worldHeight-1; y >= 0; y--)
                {
                    var block = GetBlock(new Vector3Int(x, y, z));
                    var transparency = block.BlockData.opacity;
                    lightRay -= transparency;
                    lightRay = Mathf.Clamp(lightRay, 0, 15);

                    block.SetSkyLight(lightRay);

                    if (block.section.GetLight(block.position) > 1)
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
                if (IsInRange(neighborPos))
                {
                    var neighborBlock = GetBlock(neighborPos);
                    if (neighborBlock.BlockData.opacity < 15 && neighborBlock.GetSkyLight() < block.GetSkyLight()-1)
                    {
                        neighborBlock.SetSkyLight(block.GetSkyLight() - 1);

                        if (neighborBlock.GetSkyLight() > 1)
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

public struct BlockLightNode
{
    public Block block;
    public byte lightLevel;
    
    public BlockLightNode(Block block, byte lightLevel)
    {
        this.block = block;
        this.lightLevel = lightLevel;
    }
}