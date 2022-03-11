
using UnityEngine;

public class WaterLayerHandler : BlockLayerHandler
{

    public int waterLevel = 1;
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector3Int mapSeedOffset)
    {
        var y = worldPos.y;
        if (y > surfaceHeightNoise && y < waterLevel)
        {
            chunk.SetBlock(localPos, BlockType.Water);

            if (y == surfaceHeightNoise + 1)
            {
                chunk.SetBlock(localPos + Vector3Int.down, BlockType.Sand);
                chunk.SetBlock(localPos + Vector3Int.down*2, BlockType.Sand);
                chunk.SetBlock(localPos + Vector3Int.down*3, BlockType.Sand);
            }
            
            return true;
        }
        return false;
    }
}
