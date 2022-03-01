
using UnityEngine;

public class UndergroundLayerHandler : BlockLayerHandler
{
    public BlockType undergroundBlockType;
    
    protected override bool TryHandling(ChunkData chunk, Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y < surfaceHeightNoise)
        {
            Chunk.SetBlock(chunk, localPos, undergroundBlockType);
            return true;
        }
        return false;
    }
}
