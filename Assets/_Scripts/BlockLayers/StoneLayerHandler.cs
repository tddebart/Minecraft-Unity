using UnityEngine;

public class StoneLayerHandler : BlockLayerHandler
{
    [Range(0, 1)] public float stoneThreshold = 0.5f;
    [SerializeField] private NoiseSettings stoneNoiseSettings;
    
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (chunk.worldPos.y > surfaceHeightNoise)
        {
            return false;
        }
        
        stoneNoiseSettings.worldOffset = mapSeedOffset;
        var stoneNoise = MyNoise.OctavePerlin(chunk.worldPos.x + pos.x, chunk.worldPos.z + pos.z, stoneNoiseSettings);

        int endPosition = surfaceHeightNoise;
        if (chunk.worldPos.y < 0)
        {
            endPosition = chunk.worldPos.y + chunk.chunkHeight;
        }

        if (stoneNoise > stoneThreshold)
        {
            for (var i = chunk.worldPos.y; i <= endPosition; i++)
            {
                var stonePos = new Vector3Int(pos.x, i, pos.z);
                if (Chunk.GetBlock(chunk, stonePos) != BlockType.Sand)
                {
                    Chunk.SetBlock(chunk, stonePos, BlockType.Stone);
                }
            }
            return true;
        }

        return false;
    }
}
