
using UnityEngine;

public class AirLayerHandler : BlockLayerHandler
{
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (pos.y > surfaceHeightNoise)
        {
            Chunk.SetBlock(chunk, pos, BlockType.Air);
            return true;
        }
        return false;
    }
}
