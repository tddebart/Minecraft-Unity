using UnityEngine;

public class TreeNoiseGenerator : MonoBehaviour
{
    public NoiseSettings treeNoiseSettings;
    public DomainWarping domainWarping;

    public TreeData GenerateTreeData(ChunkData chunkData, Vector3Int mapSeedOffset)
    {
        treeNoiseSettings.worldSeedOffset = mapSeedOffset;
        var treeData = new TreeData();
        float[,] noiseData = GenerateTreeNoiseData(chunkData, treeNoiseSettings);
        treeData.treePositions = DataProcessing.FindLocalMaxima(noiseData, chunkData.worldPos.x, chunkData.worldPos.z);
        
        return treeData;
    }

    private float[,] GenerateTreeNoiseData(ChunkData chunkData, NoiseSettings noiseSettings)
    {
        var noiseMax = new float[chunkData.chunkSize, chunkData.chunkSize];
        var xMin = chunkData.worldPos.x;
        var zMin = chunkData.worldPos.z;
        var xMax = chunkData.worldPos.x + chunkData.chunkSize;
        var zMax = chunkData.worldPos.z + chunkData.chunkSize;

        var xIndex = 0;
        var zIndex = 0;

        for (var x = xMin; x < xMax; x++)
        {
            for (var z = zMin; z < zMax; z++)
            {
                noiseMax[xIndex, zIndex] = domainWarping.GenerateDomainNoise(x, z, noiseSettings);
                zIndex++;
            }
            xIndex++;
            zIndex = 0;
        }
        
        return noiseMax;
    }
}