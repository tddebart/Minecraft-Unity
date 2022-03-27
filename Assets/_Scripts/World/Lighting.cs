using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Lighting
{
    public static void CalculateLight(ChunkData data)
    {
        // Block light calculation
        if(data.blockLightRemoveQueue.Count > 0)
        {
            while(data.blockLightRemoveQueue.Count > 0)
            {
                var node = data.blockLightRemoveQueue.Dequeue();
                RemoveBlockLightBFS(data, node.block, node.lightLevel);
            }
        }
        
        
        if (data.blockLightUpdateQueue.Count > 0)
        {
            while (data.blockLightUpdateQueue.Count > 0)
            {
                var node = data.blockLightUpdateQueue.Dequeue();
                PlaceBlockLightBFS(data, node.block, node.lightLevel);
            }
        }

        data.chunkToUpdateAfterLighting = data.chunkToUpdateAfterLighting.Distinct().ToList();
        foreach (var chunk in data.chunkToUpdateAfterLighting)
        {
            World.Instance.AddChunkToUpdate(chunk, true);
        }
        data.chunkToUpdateAfterLighting.Clear();
    }

    public static void RemoveBlockLightBFS(ChunkData data, Block block, byte oldLightValue)
    {
        foreach (var neighbor in block.GetNeighbors())
        {
            var neighbourLightValue = neighbor.GetBlockLight();
            if (neighbourLightValue != 0 && neighbourLightValue < oldLightValue)
            {
                neighbor.SetBlockLight(0);
                if (neighbor.chunkData != data)
                {
                    data.chunkToUpdateAfterLighting.Add(neighbor.chunkData.renderer);
                }
                data.blockLightRemoveQueue.Enqueue(new BlockLightNode(neighbor, (byte)neighbourLightValue));
            } else if (neighbourLightValue >= oldLightValue)
            {
                data.blockLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, (byte)neighbourLightValue));
            }
        }
    }

    public static void PlaceBlockLightBFS(ChunkData data, Block block, byte lightValue)
    {
        var currentLight = block.GetBlockLight();
        foreach (var neighbor in block.GetNeighbors())
        {
            if (neighbor.BlockData.opacity < 15 && neighbor.GetBlockLight() < currentLight-1)
            {
                neighbor.SetBlockLight(currentLight-1);
                if (neighbor.chunkData != data)
                {
                    data.chunkToUpdateAfterLighting.Add(neighbor.chunkData.renderer);
                }
                data.blockLightUpdateQueue.Enqueue(new BlockLightNode(neighbor, (byte)(currentLight-1)));
            }
        }
    }
}
