using UnityEngine;

public class StoneLayerHandler : BlockLayerHandler
{
    [Range(0, 1)] public float stoneThreshold = 0.5f;
    [SerializeField] private NoiseSettings stoneNoiseSettings;
    
    public DomainWarping stoneDomainWarping;
    
    protected override bool TryHandling(ChunkData chunk,Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        return false;
        if (chunk.worldPos.y > surfaceHeightNoise)
        {
            return false;
        }
        
        stoneNoiseSettings.worldSeedOffset = mapSeedOffset;
        //var stoneNoise = MyNoise.OctavePerlin(chunk.worldPos.x + pos.x, chunk.worldPos.z + pos.z, stoneNoiseSettings);
        var stoneNoise = stoneDomainWarping.GenerateDomainNoise(chunk.worldPos.x + localPos.x, chunk.worldPos.z + localPos.z, stoneNoiseSettings);

        int endPosition = surfaceHeightNoise;
        if (chunk.worldPos.y < 0)
        {
            endPosition = chunk.worldPos.y + chunk.chunkHeight;
        }

        if (chunk.GetBlock(new Vector3Int(localPos.x,endPosition,localPos.z)) == BlockType.Sand)
        {
            return false;
        }

        if (stoneNoise > stoneThreshold)
        {
            for (var i = chunk.worldPos.y; i <= endPosition; i++)
            {
                var stonePos = new Vector3Int(localPos.x, i, localPos.z);
                chunk.SetBlock(stonePos, BlockType.Stone);
            }
            return true;
        }

        return false;
    }
}
