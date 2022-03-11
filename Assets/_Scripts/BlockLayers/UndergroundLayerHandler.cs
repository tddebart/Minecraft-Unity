
using UnityEngine;

public class UndergroundLayerHandler : BlockLayerHandler
{
    public BlockType undergroundBlockType;
    
    protected override bool TryHandling(ChunkData chunk, Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector3Int mapSeedOffset)
    {
        if (worldPos.y < surfaceHeightNoise)
        {
            chunk.SetBlock(localPos, undergroundBlockType);
            return true;
        }
        return false;
    }
}
