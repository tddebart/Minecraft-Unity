
using UnityEngine;

public class AirLayerHandler : BlockLayerHandler
{
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y > surfaceHeightNoise)
        {
            Chunk.SetBlock(chunk, localPos, BlockType.Air);
            return true;
        }
        return false;
    }
}
