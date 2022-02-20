﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public NoiseSettings settings;

    public DomainWarping domainWarping;

    public bool useWarping = true;

    public BlockLayerHandler startLayerHandler;

    public List<BlockLayerHandler> additionalLayerHandlers;
    
    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset)
    {
        settings.worldOffset = mapSeedOffset;
        var groundPos = GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.chunkHeight);
        for (var y = 0; y < data.chunkHeight; y++)
        {
            startLayerHandler.Handle(data, new Vector3Int(x,y,z), groundPos, mapSeedOffset);
        }
        
        foreach (var layer in additionalLayerHandlers)
        {
            layer.Handle(data, new Vector3Int(x,data.worldPos.y,z), groundPos, mapSeedOffset);
        }

        return data;
    }

    private int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
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
        var surfaceHeight = (int)Mathf.Lerp(0, chunkHeight, terrainHeight);
        return surfaceHeight;
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