
using UnityEngine;

public class WaterLayerHandler : BlockLayerHandler
{

    public int waterLevel = 1;
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (pos.y > surfaceHeightNoise && pos.y < waterLevel)
        {
            Chunk.SetBlock(chunk, pos, BlockType.Water);
            
            if (pos.y == surfaceHeightNoise + 1)
            {
                pos.y = surfaceHeightNoise;
                Chunk.SetBlock(chunk, pos, BlockType.Sand);
                Chunk.SetBlock(chunk, pos + Vector3Int.down, BlockType.Sand);
            }
            
            return true;
        }
        return false;
    }
}
