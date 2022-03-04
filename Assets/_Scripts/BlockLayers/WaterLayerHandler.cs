
using UnityEngine;

public class WaterLayerHandler : BlockLayerHandler
{

    public int waterLevel = 1;
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y > surfaceHeightNoise && worldPos.y < waterLevel)
        {
            chunk.SetBlock(localPos, BlockType.Water);

            if (worldPos.y == surfaceHeightNoise + 1)
            {
                chunk.SetBlock(localPos + Vector3Int.down, BlockType.Sand);
                chunk.SetBlock(localPos + Vector3Int.down*2, BlockType.Sand);
            }
            
            return true;
        }
        return false;
    }
}
