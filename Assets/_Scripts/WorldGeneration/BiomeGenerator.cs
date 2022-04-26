using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BiomeGenerator : MonoBehaviour
{
    public NoiseSettings settings;

    public DomainWarping domainWarping;

    public bool useWarping = true;

    public bool enableLodes = true;

    public BlockLayerHandler startLayerHandler;
    
    public TreeNoiseGenerator treeNoiseGenerator;

    public List<BlockLayerHandler> featureLayerHandlers;
    
    public List<Lode> lodes;
    
    public int extraTerrainHeightPercentage = 0;

    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector3Int mapSeedOffset, int? terrainHeightNoise)
    {
        settings.worldSeedOffset = mapSeedOffset;
        
        var groundPos = terrainHeightNoise ?? GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.worldRef.worldHeight);

        var worldPos = new Vector3Int(data.worldPos.x + x, 0, data.worldPos.z + z);
        var localPos = new Vector3Int(x, 0, z);

        for (var y = 0; y < data.worldRef.worldHeight; y++)
        {
            worldPos.y = y;
            localPos.y = y;

            // if (MyNoise.OctavePerlin3D(worldPos, settings))
            // {
            //     data.SetBlock(localPos, BlockType.Stone);
            // }
            // else
            // {
            //     data.SetBlock(localPos, BlockType.Air);
            // }
            //
            startLayerHandler.Handle(data, worldPos, localPos, groundPos, mapSeedOffset);
        }


        return data;
    }


    public void ProcessFeatures(ChunkData data, int x, int z, Vector3Int mapSeedOffset, int? terrainHeightNoise)
    {
        settings.worldSeedOffset = mapSeedOffset;
        var groundPos = terrainHeightNoise ?? GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.worldRef.worldHeight);

        var worldPos = new Vector3Int(data.worldPos.x + x, 0, data.worldPos.z + z);
        var localPos = new Vector3Int(x, 0, z);

        if (enableLodes)
        {
            // Process lodes
            foreach (var lode in lodes)
            {
                for (var y = lode.maxHeight - 1; y >= lode.minHeight; y--)
                {
                    worldPos.y = y;
                    localPos.y = y;
                    
                    if (data.GetBlock(localPos).type is BlockType.Air or BlockType.Water)
                    {
                        continue;
                    }
                    
                    lode.noiseSettings.worldSeedOffset = mapSeedOffset;

                    if (MyNoise.OctavePerlin3D(worldPos, lode.noiseSettings, lode.threshold))
                    {
                        if (data.GetBlock(localPos).type == BlockType.Grass && data.GetBlock(localPos+Vector3Int.down).type == BlockType.Dirt)
                        {
                            data.SetBlock(localPos+Vector3Int.down, BlockType.Grass);
                        }
                        data.SetBlock(localPos, lode.blockType);
                    }
                }
            }
        }
        
        foreach (var layer in featureLayerHandlers)
        {
            for (var y = 0; y < data.worldRef.worldHeight; y++)
            {
                worldPos.y = y;
                localPos.y = y;
                
                layer.Handle(data, worldPos,localPos, groundPos, mapSeedOffset);
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
        var surfaceHeight = (int)Mathf.Lerp(0+worldHeight*(extraTerrainHeightPercentage/100f), worldHeight-1, terrainHeight);
        return surfaceHeight;
    }

    public TreeData GenerateTreeData(ChunkData data, Vector3Int mapSeedOffset)
    {
        if(treeNoiseGenerator == null)
        {
            return new TreeData();
        }
        return treeNoiseGenerator.GenerateTreeData(data, mapSeedOffset);
    }
}

[System.Serializable]
public class Lode
{
    public BlockType blockType;
    public int minHeight;
    public int maxHeight;
    public float threshold;
    public NoiseSettings noiseSettings;
}

#if UNITY_EDITOR

[CustomEditor(typeof(BiomeGenerator))]
public class BiomeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var biomeGenerator = (BiomeGenerator)this.target;
        DrawDefaultInspector();
        EditorGUILayout.Space(20);
        var customEditor = Editor.CreateEditor(biomeGenerator.settings);
        customEditor.OnInspectorGUI();
    }
}
    
#endif
