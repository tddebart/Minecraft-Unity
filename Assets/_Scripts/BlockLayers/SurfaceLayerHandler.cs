
using UnityEngine;

public class SurfaceLayerHandler : BlockLayerHandler
{
    public BlockType surfaceBlockType;
    public BlockType subSurfaceBlockType;
    
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y == surfaceHeightNoise)
        {
            chunk.SetBlock(localPos,surfaceBlockType);
            for (var i = 1; i < 3; i++)
            {
                chunk.SetBlock(localPos - new Vector3Int(0, i, 0), subSurfaceBlockType);
            }
            return true;
        }
        return false;
    }
}
