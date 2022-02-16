
using UnityEngine;

public class SurfaceLayerHandler : BlockLayerHandler
{
    public BlockType surfaceBlockType;
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (pos.y == surfaceHeightNoise)
        {
            Chunk.SetBlock(chunk,pos,surfaceBlockType);
            return true;
        }
        return false;
    }
}
