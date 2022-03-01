
using UnityEngine;

public class WaterLayerHandler : BlockLayerHandler
{

    public int waterLevel = 1;
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (worldPos.y > surfaceHeightNoise && worldPos.y < waterLevel)
        {
            Chunk.SetBlock(chunk, localPos, BlockType.Water);

            if (worldPos.x == 33 && worldPos.z == 5)//16
            {
                var x = 0;
            }
            
            if (worldPos.y == surfaceHeightNoise + 1)
            {
                if ((localPos + Vector3Int.down).y < 0 || (localPos + Vector3Int.down * 2).y < 0)
                {
                    chunk.worldRef.blocksToPlaceAfterGeneration[worldPos+Vector3Int.down] = BlockType.Sand;
                    chunk.worldRef.blocksToPlaceAfterGeneration[worldPos+Vector3Int.down*2] = BlockType.Sand;
                }
                else
                {
                    Chunk.SetBlock(chunk, localPos + Vector3Int.down, BlockType.Sand);
                    Chunk.SetBlock(chunk, localPos + Vector3Int.down*2, BlockType.Sand);
                }
            }
            
            return true;
        }
        return false;
    }
}
