using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Lighting
{
    public static void CalculateLight(ChunkData data)
    {
        // Sky light calculation
        if (data.skyLightRemoveQueue.Count > 0)
        {
            while (data.skyLightRemoveQueue.Count > 0)
            {
                var node = data.skyLightRemoveQueue.Dequeue();
                RemoveSkyLight(data, node.block, node.lightLevel);
            }
        }
        
        if (data.skyLightUpdateQueue.Count > 0)
        {
            while (data.skyLightUpdateQueue.Count > 0)
            {
                var node = data.skyLightUpdateQueue.Dequeue();
                PlaceSkyLight(data, node.block, node.lightLevel);
            }
        }
        
        
        // Block light calculation
        if(data.blockLightRemoveQueue.Count > 0)
        {
            while(data.blockLightRemoveQueue.Count > 0)
            {
                var node = data.blockLightRemoveQueue.Dequeue();
                RemoveBlockLight(data, node.block, node.lightLevel);
            }
        }
        
        
        if (data.blockLightUpdateQueue.Count > 0)
        {
            while (data.blockLightUpdateQueue.Count > 0)
            {
                var node = data.blockLightUpdateQueue.Dequeue();
                PlaceBlockLight(data, node.block, node.lightLevel);
            }
        }

        data.chunkToUpdateAfterLighting = data.chunkToUpdateAfterLighting.Distinct().ToList();
        foreach (var chunk in data.chunkToUpdateAfterLighting)
        {
            World.Instance.AddChunkToUpdate(chunk, true);
        }
        data.chunkToUpdateAfterLighting.Clear();
    }

    public static void CalculateSkyLightExtend(ChunkData data)
    {
        if (data.skyExtendList.Count == 0) return;
        
        foreach (var block in data.skyExtendList)
        {
            if (block.GetSkyLight() == 15)
            {
                ExtendSunRay(data,block);
            }
        }
        
        data.skyExtendList.Clear();
    }

    public static void ExtendSunRay(ChunkData data, Block block)
    {
        var pos = block.localChunkPosition;

        while (true)
        {
            if (pos.y < 0)
            {
                return;
            }
            else
            {
                var blockBelow = data.GetBlock(pos); 
                if (blockBelow.BlockData.opacity < 15)
                {
                    if (blockBelow.GetSkyLight() != 15)
                    {
                        data.skyLightUpdateQueue.Enqueue(new BlockLightNode(blockBelow, 15));
                    }
                }
                else
                {
                    return;
                }
                
                pos.y--;
            }
        }
    }

    public static void CalculateSkyLightRemove(ChunkData data)
    {
        if (data.skyRemoveList.Count == 0) return;

        foreach (var block in data.skyRemoveList)
        {
            if (block.GetSkyLight() == 0)
            {
                BlockSunRay(data,block);   
            }
        }
        
        data.skyRemoveList.Clear();
    }

    public static void BlockSunRay(ChunkData data, Block block)
    {
        var pos = block.localChunkPosition;
        pos.y--;

        while (true)
        {
            if (pos.y < 0)
            {
                return;
            }
            else
            {
                var blockBelow = data.GetBlock(pos); 
                if (blockBelow.GetSkyLight() == 15)
                {
                    blockBelow.SetSkyLight(0);
                    RemoveSkyLight(data,blockBelow, 15);
                }
                else
                {
                    return;
                }
                
                pos.y--;
            }
        }
    }
    
    public static void RemoveSkyLight(ChunkData data, Block block, int oldLightValue)
    {
        if (block == null) return;
        
        if (oldLightValue > 0)
        {
            oldLightValue--;
        }
        else
        {
            oldLightValue = 0;
        }

        foreach (var neighbor in block.GetNeighbors())
        {
            var neighborLight = neighbor.GetSkyLight();
            if (neighborLight > 0)
            {
                if (neighborLight <= oldLightValue)
                {
                    neighbor.SetSkyLight(0);
                    CheckForEdgeUpdate(neighbor, data);
                    data.skyLightRemoveQueue.Enqueue(new BlockLightNode(neighbor, (byte)neighborLight));
                }
                else
                {
                    data.skyLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, 0));
                }
            }
        }
    }
    
    public static void PlaceSkyLight(ChunkData data, Block block, int lightLevel)
    {
        if (block == null) return;
        
        if (lightLevel > block.GetSkyLight())
        {
            block.SetSkyLight(lightLevel);
        }
        else
        {
            lightLevel = block.GetSkyLight();
        }

        if (lightLevel <= 1)
        {
            return;
        }

        lightLevel--;

        foreach (var neighbor in block.GetNeighbors().Where(b => b.type != BlockType.Nothing))
        {
            if(neighbor.GetSkyLight() < lightLevel && neighbor.BlockData.opacity != 15)
            {
                neighbor.SetSkyLight(lightLevel);
                CheckForEdgeUpdate(neighbor, data);
                data.skyLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, (byte)lightLevel));
            }
        }
    }

    public static void RemoveBlockLight(ChunkData data, Block block, byte oldLightValue)
    {
        foreach (var neighbor in block.GetNeighbors())
        {
            var neighbourLightValue = neighbor.GetBlockLight();
            if (neighbourLightValue != 0 && neighbourLightValue < oldLightValue)
            {
                neighbor.SetBlockLight(0);
                CheckForEdgeUpdate(neighbor, data);
                data.blockLightRemoveQueue.Enqueue(new BlockLightNode(neighbor, (byte)neighbourLightValue));
            } else if (neighbourLightValue >= oldLightValue)
            {
                data.blockLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, (byte)neighbourLightValue));
            }
        }
    }

    public static void PlaceBlockLight(ChunkData data, Block block, byte lightValue)
    {
        var currentLight = block.GetBlockLight();
        foreach (var neighbor in block.GetNeighbors().Where(b => b.type != BlockType.Nothing))
        {
            if (neighbor.BlockData.opacity < 15 && neighbor.GetBlockLight() < currentLight-1)
            {
                neighbor.SetBlockLight(currentLight-1);
                CheckForEdgeUpdate(neighbor, data);

                data.blockLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, (byte)(currentLight-1)));
            }
        }
    }

    public static void CheckForEdgeUpdate(Block block, ChunkData data)
    {
        if (block.chunkData != data)
        {
            data.chunkToUpdateAfterLighting.Add(block.chunkData.renderer);
        }
        else if (data.IsOnEdge(block.globalWorldPosition))
        {
            var chunks = data.GetNeighbourChunk(block.globalWorldPosition);
            foreach (var chunk in chunks)
            {
                data.chunkToUpdateAfterLighting.Add(chunk.renderer);
            }
        }
    }
    
    public static void RecastSunLightFirstTime(ChunkData chunkData)
    {
        for (var x = 0; x < chunkData.chunkSize; x++)
        {
            for (var z = 0; z < chunkData.chunkSize; z++)
            {
                RecastSunLight(chunkData, new Vector3Int(x,chunkData.worldRef.worldHeight,z));
            }
        }

        for (var x = 0; x < chunkData.chunkSize; x++)
        {
            for (var y = 0; y < World.Instance.worldHeight-1; y++)
            {
                for (var z = 0; z < chunkData.chunkSize; z++)
                {
                    var block = chunkData.GetBlock(new Vector3Int(x, y, z));
                    if (block.GetSkyLight() != 0)
                    {
                        chunkData.skyLightUpdateQueue.Enqueue(new BlockLightNode(block, (byte)block.GetSkyLight()));
                    }
                }
            }
        }
    }
    
    public static void RecastSunLight(ChunkData chunkData, Vector3Int startPos)
    {
        bool obstructed = false;

        // Loop from top to bottom of chunk.
        for (int y = startPos.y; y > -1; y--) {
            var block = chunkData.GetBlock(new Vector3Int(startPos.x, y, startPos.z));

            // If light has been obstructed, all blocks below that point are set to 0.
            if (obstructed) {
                block.SetSkyLight(0);
                // Else if block has opacity, set light to 0 and obstructed to true.
            } else if (block.BlockData.opacity > 0) {
                block.SetSkyLight(0);
                obstructed = true;
                // Else set light to 15.
            } else {
                block.SetSkyLight(15);
            }
        }
    }
}
