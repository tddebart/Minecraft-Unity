
using UnityEngine;

public class UndergroundLayerHandler : BlockLayerHandler
{
    public BlockType undergroundBlockType;
    
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (pos.y < surfaceHeightNoise)
        {
            Chunk.SetBlock(chunk, pos, undergroundBlockType);
            return true;
        }
        return false;
    }
}
