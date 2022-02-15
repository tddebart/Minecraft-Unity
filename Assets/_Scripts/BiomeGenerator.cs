using UnityEditor;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public int waterThreshold = 50; // water level
    public NoiseSettings settings;
    
    public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2Int mapSeedOffset)
    {
        settings.worldOffset = mapSeedOffset;
        var groundPos = GetSurfaceHeightNoise(data.worldPos.x + x, data.worldPos.z + z,data.chunkHeight);
        for (var y = 0; y < data.chunkHeight; y++)
        {
            var blockType = BlockType.Dirt;
            if (y > groundPos)
            {
                if (y < waterThreshold)
                {
                    blockType = BlockType.Water;
                }
                else
                {
                    blockType = BlockType.Air;
                }
            }
            else if (y == groundPos && y < waterThreshold)
            {
                blockType = BlockType.Sand;
            }
            else if(y == groundPos)
            {
                blockType = BlockType.Grass;
            }
                    
            Chunk.SetBlock(data, new Vector3Int(x, y, z), blockType);
        }
        
        
        return data;
    }

    private int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
    {
        var terrainHeight = MyNoise.OctavePerlin(x, z, settings);
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
