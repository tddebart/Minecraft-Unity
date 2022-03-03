using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public BiomeGenerator biomeGenerator;
    [SerializeField] private List<Vector3Int> biomeCenters = new List<Vector3Int>();
    private List<float> temperatureNoise = new List<float>();

    [SerializeField] private NoiseSettings temperatureNoiseSettings;
    
    public DomainWarping domainWarping;
    [Tooltip("Inverse Distance Weighting")]
    public bool useIDW = true;
    
    [SerializeField]  private List<BiomeData> biomeGeneratorsData = new List<BiomeData>();

    
    
    public ChunkData GenerateChunkData(ChunkData data, Vector2Int mapSeedOffset)
    {
        BiomeGeneratorSelection biomeSelection = SelectBiomeGeneratorWeight(data.worldPos, data,false);
        // TreeData treeData = biomeGenerator.GenerateTreeData(data, mapSeedOffset);
        data.treeData = biomeSelection.biomeGenerator.GenerateTreeData(data, mapSeedOffset);
        
        for (var x = 0; x < data.chunkSize; x++)
        {
            for (var z = 0; z < data.chunkSize; z++)
            {
                biomeSelection = SelectBiomeGeneratorWeight( new Vector3Int(data.worldPos.x + x, 0, data.worldPos.z + z), data);
                data = biomeSelection.biomeGenerator.ProcessChunkColumn(data, x, z, mapSeedOffset, biomeSelection.terrainSurfaceNoise);
            }
        }

        return data;
    }

    public void GenerateFeatures(ChunkData data, Vector2Int mapSeedOffset)
    {
        for (var x = 0; x < data.chunkSize; x++)
        {
            for (var z = 0; z < data.chunkSize; z++)
            {
                BiomeGeneratorSelection biomeSelection = SelectBiomeGeneratorWeight(new Vector3Int(data.worldPos.x + x, 0, data.worldPos.z + z), data);
                biomeSelection.biomeGenerator.ProcessFeatures(data, x, z, mapSeedOffset, biomeSelection.terrainSurfaceNoise);
            }
        }
    }


    // source: https://gisgeography.com/inverse-distance-weighting-idw-interpolation/
    //TODO: biome selection is incredibly slow, need to optimize it. For now i will just always use the first biome
    private BiomeGeneratorSelection SelectBiomeGeneratorWeight(Vector3Int worldPos, ChunkData data, bool useDomainWarping = true)
    {
        
        var originalWorldPos = worldPos;
        if (useDomainWarping)
        {
            var domainOffset = Vector2Int.RoundToInt(domainWarping.GenerateDomainOffset(worldPos.x, worldPos.z));
            worldPos += new Vector3Int(domainOffset.x, 0, domainOffset.y);
        }
        
        return biomeGeneratorsData.Select(b => new BiomeGeneratorSelection(b.biomeTerrainGenerator, b.biomeTerrainGenerator.GetSurfaceHeightNoise(worldPos.x,worldPos.z, data.worldRef.worldHeight))).First();
        
        var biomeSelectionHelpersByDistance = GetBiomeGeneratorSelectionHelpers(worldPos);
        var selectionHelpersByDistance = biomeSelectionHelpersByDistance as BiomeSelectionHelper[] ?? biomeSelectionHelpersByDistance.ToArray();

        // Select the biome generators based on the temperature noise
        var generator1 = SelectBiome(selectionHelpersByDistance[0].Index);
        var generator2 = SelectBiome(selectionHelpersByDistance[1].Index);
        var generator3 = SelectBiome(selectionHelpersByDistance[2].Index);
        

        var terrainHeight1 = generator1.GetSurfaceHeightNoise(worldPos.x,worldPos.z, data.worldRef.worldHeight);
        var terrainHeight2 = generator2.GetSurfaceHeightNoise(worldPos.x,worldPos.z, data.worldRef.worldHeight);
        var terrainHeight3 = generator3.GetSurfaceHeightNoise(worldPos.x,worldPos.z, data.worldRef.worldHeight);

        if (!useIDW)
        {
            return new BiomeGeneratorSelection(generator1, terrainHeight1);
        }
        
        if(selectionHelpersByDistance[0].Distance == 0)
        {
            return new BiomeGeneratorSelection(generator1, terrainHeight1);
        }

        var distance1 = selectionHelpersByDistance[0].Distance;
        var distance2 = selectionHelpersByDistance[1].Distance;
        var distance3 = selectionHelpersByDistance[2].Distance;

        var power = 3;

        if (worldPos.x is 169 or 168 && worldPos.z is -68)
        {
            var x = 0;
        }
        
        return new BiomeGeneratorSelection(generator1, Mathf.RoundToInt(
            (
                terrainHeight1/Mathf.Pow(distance1,power) + 
                terrainHeight2/Mathf.Pow(distance2,power) +
                terrainHeight3/Mathf.Pow(distance3,power)
                )
                / 
                (1/Mathf.Pow(distance1,power) + 
                 1/Mathf.Pow(distance2,power) +
                 1/Mathf.Pow(distance3,power)
                 )
                
                ));
        
        // return new BiomeGeneratorSelection(generator1, Mathf.RoundToInt((terrainHeight1+terrainHeight2)/2f));

    }

    private BiomeGenerator SelectBiome(int index)
    {
        var temp = temperatureNoise[index];
        temp *= 4f;
        foreach (var data in biomeGeneratorsData)
        {
            if(temp >= data.temperatureStartThreshold && temp <= data.temperatureEndThreshold)
            {
                return data.biomeTerrainGenerator;
            }
        }

        Debug.LogError("No biome found for temperature: " + temp);
        return biomeGeneratorsData[0].biomeTerrainGenerator;
    }

    private IEnumerable<BiomeSelectionHelper> GetBiomeGeneratorSelectionHelpers(Vector3Int pos)
    {
        pos.y = 0;
        return GetClosestBiomeIndex(pos);
    }

    private IEnumerable<BiomeSelectionHelper> GetClosestBiomeIndex(Vector3Int pos)
    {
        return biomeCenters.Select((center, index) => 
        new BiomeSelectionHelper
        {
            Index = index,
            Distance = Vector3.Distance(pos, center)
        }).OrderBy(helper => helper.Distance).Take(4);
    }
    
    private struct BiomeSelectionHelper
    {
        public int Index;
        public float Distance;
    }

    public void GenerateBiomePoints(Vector3 playerPos, int renderDistance, int chunkSize, Vector2Int mapSeedOffset)
    {
        biomeCenters = new List<Vector3Int>();
        biomeCenters = BiomeCenterFinder.CalculateBiomeCenters(playerPos, renderDistance, chunkSize);

        var originamplitude = domainWarping.amplitude;
        domainWarping.amplitude.x = Mathf.RoundToInt(domainWarping.amplitude.x*3.3f);
        domainWarping.amplitude.y = Mathf.RoundToInt(domainWarping.amplitude.y*3.3f);
        for (var i = 0; i < biomeCenters.Count; i++)
        {
            var domainWarpingOffset = domainWarping.GenerateDomainOffsetInt(biomeCenters[i].x, biomeCenters[i].z);
            biomeCenters[i] += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
        }
        domainWarping.amplitude = originamplitude;
        
        temperatureNoise = CalculateTemperatureNoise(biomeCenters, mapSeedOffset);
    }

    private List<float> CalculateTemperatureNoise(List<Vector3Int> positions, Vector2Int mapSeedOffset)
    {
        temperatureNoiseSettings.worldSeedOffset = mapSeedOffset;
        return positions.Select(pos => MyNoise.OctavePerlin(pos.x,pos.z,temperatureNoiseSettings)).ToList();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Selection.activeObject != gameObject) return;
        Gizmos.color = Color.blue;
        
        foreach (var biomeCenter in biomeCenters)
        {
            Gizmos.DrawLine(biomeCenter, biomeCenter + Vector3.up*255f);
        }
    }
    #endif
}

public class BiomeGeneratorSelection
{
    public BiomeGenerator biomeGenerator = null;
    public int? terrainSurfaceNoise = null;
    
    public BiomeGeneratorSelection(BiomeGenerator biomeGenerator, int? terrainSurfaceNoise = null)
    {
        this.biomeGenerator = biomeGenerator;
        this.terrainSurfaceNoise = terrainSurfaceNoise;
    }
}

[Serializable]
public struct BiomeData
{
    [Range(0f, 4f)] public float temperatureStartThreshold, temperatureEndThreshold;
    public BiomeGenerator biomeTerrainGenerator;
}