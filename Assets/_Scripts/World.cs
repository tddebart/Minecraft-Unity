using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    public int mapSize = 6;
    public int chunkSize = 16; // 16x16 chunks
    public int chunkHeight = 100; // 100 blocks high
    public GameObject chunkPrefab;
    
    public TerrainGenerator terrainGenerator;
    public Vector2Int mapOffset;
    
    Dictionary<Vector3Int, ChunkData> chunkDataDict = new Dictionary<Vector3Int, ChunkData>();
    Dictionary<Vector3Int, ChunkRenderer> chunkDict = new Dictionary<Vector3Int, ChunkRenderer>();

    public UnityEvent OnWorldCreated;
    public UnityEvent OnNewChunksGenerated;

    public void GenerateWorld()
    {
        chunkDataDict.Clear();
        foreach (ChunkRenderer chunk in chunkDict.Values)
        {
            DestroyImmediate(chunk.gameObject);
        }

        foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
        {
            DestroyImmediate(chunk.gameObject);
        }
        chunkDict.Clear();
        
        for (var x = 0; x < mapSize; x++)
        {
            for (var z = 0; z < mapSize; z++)
            {
                var data = new ChunkData(chunkSize, chunkHeight, this, new Vector3Int(x*chunkSize, 0,z*chunkSize));
                // GenerateChunkBlocks(data);
                var newData = terrainGenerator.GenerateChunkData(data, mapOffset);
                chunkDataDict.Add(newData.worldPos, newData);
            }
        }
        
        foreach (var data in chunkDataDict.Values)
        {
            var meshData = Chunk.GetChunkMeshData(data);
            var chunkObj = Instantiate(chunkPrefab, data.worldPos, Quaternion.identity);
            var chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
            chunkDict.Add(data.worldPos, chunkRenderer);
            chunkRenderer.Initialize(data);
            chunkRenderer.UpdateChunk(meshData);
        }
        
        OnWorldCreated?.Invoke();
    }

    public BlockType GetBlock(ChunkData chunkData, Vector3Int pos)
    {
        var chunkPos = Chunk.ChunkPosFromBlockCoords(this, pos);

        chunkDataDict.TryGetValue(chunkPos, out var containerChunk);
        
        if (containerChunk == null)
        {
            return BlockType.Nothing;
        }

        var blockPos = Chunk.GetLocalBlockCoords(containerChunk, new Vector3Int(pos.x, pos.y, pos.z));
        return Chunk.GetBlock(containerChunk, blockPos);
    }
    
    public void LoadAdditionalChunks(GameObject localPlayer)
    {
        Debug.Log("Loading additional chunks");
        OnNewChunksGenerated?.Invoke();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(World))]
    public class WorldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var world = target as World;
            if (GUILayout.Button("Generate World"))
            {
                world.GenerateWorld();
            }

            if (GUILayout.Button("Clear World"))
            {
                world.chunkDataDict.Clear();
                world.chunkDict.Clear();
                foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
                {
                    DestroyImmediate(chunk.gameObject);
                }
            }
            
            base.OnInspectorGUI();
        }
    }
#endif
}
