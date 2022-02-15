using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public BiomeGenerator biomeGenerator;
    
    public ChunkData GenerateChunkData(ChunkData data, Vector2Int mapOffset)
    {
        for (var x = 0; x < data.chunkSize; x++)
        {
            for (var z = 0; z < data.chunkSize; z++)
            {
                data = biomeGenerator.ProcessChunkColumn(data, x, z, mapOffset);
            }
        }

        return data;
    }
}
