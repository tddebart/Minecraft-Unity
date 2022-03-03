using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BiomeGenerator : MonoBehaviour
{
    public NoiseSettings settings;

    public DomainWarping domainWarping;

    public bool useWarping = true;

    public BlockLayerHandler startLayerHandler;
    
    public TreeNoiseGenerator treeNoiseGenerator;

    public List<BlockLayerHandler> featureLayerHandlers;
    
    public int minHeight = 0;
    
    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset, int? terrainHeightNoise)
    {
        settings.worldSeedOffset = mapSeedOffset;
        
        var groundPos = terrainHeightNoise ?? GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.worldRef.worldHeight);
        
        if (groundPos < minHeight)
        {
            groundPos = minHeight;
        }
        
        for (var y = data.worldPos.y; y < data.worldPos.y + data.chunkHeight; y++)
        {
            startLayerHandler.Handle(data, new Vector3Int(x+data.worldPos.x,y,z+data.worldPos.z), new Vector3Int(x,y - data.worldPos.y,z), groundPos, mapSeedOffset);
        }


        return data;
    }


    public void ProcessFeatures(ChunkData data, int x, int z, Vector2Int mapSeedOffset, int? terrainHeightNoise)
    {
        settings.worldSeedOffset = mapSeedOffset;
        var groundPos = terrainHeightNoise ?? GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.worldRef.worldHeight);
        if (groundPos < minHeight)
        {
            groundPos = minHeight;
        }
        
        var worldPosX = data.worldPos.x + x;
        var worldPosZ = data.worldPos.z + z;
        
        var worldPosY = data.worldPos.y;

        foreach (var layer in featureLayerHandlers)
        {
            for (var y = worldPosY; y < worldPosY + data.chunkHeight; y++)
            {
                layer.Handle(data, new Vector3Int(worldPosX,y,worldPosZ),new Vector3Int(x,y - worldPosY,z), groundPos, mapSeedOffset);
            }
        }
    }

    public int GetSurfaceHeightNoise(int x, int z, int worldHeight)
    {
        float terrainHeight;
        if (useWarping)
        {
            terrainHeight = domainWarping.GenerateDomainNoise(x, z, settings);
        }
        else
        {
            terrainHeight = MyNoise.OctavePerlin(x, z, settings);
        }
        terrainHeight = MyNoise.Redistribution(terrainHeight, settings);
        var surfaceHeight = (int)Mathf.Lerp(0, worldHeight, terrainHeight);
        return surfaceHeight;
    }

    public TreeData GenerateTreeData(ChunkData data, Vector2Int mapSeedOffset)
    {
        if(treeNoiseGenerator == null)
        {
            return new TreeData();
        }
        return treeNoiseGenerator.GenerateTreeData(data, mapSeedOffset);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(BiomeGenerator))]
public class BiomeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var biomeGenerator = (BiomeGenerator)this.target;
        DrawDefaultInspector();
        var customEditor = Editor.CreateEditor(biomeGenerator.settings);
        customEditor.OnInspectorGUI();
    }
}
    
#endif
