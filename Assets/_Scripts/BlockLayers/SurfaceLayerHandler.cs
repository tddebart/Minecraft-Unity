
using UnityEngine;

public class SurfaceLayerHandler : BlockLayerHandler
{
    public BlockType surfaceBlockType;
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y == surfaceHeightNoise)
        {
            chunk.SetBlock(localPos,surfaceBlockType);
            return true;
        }
        return false;
    }
}
