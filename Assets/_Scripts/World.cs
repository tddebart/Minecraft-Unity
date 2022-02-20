using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class World : MonoBehaviour
{
    public int renderDistance = 6;
    public int chunkSize = 16; // 16x16 chunks
    public int chunkHeight = 100; // 100 blocks high
    public GameObject chunkPrefab;
    
    public TerrainGenerator terrainGenerator;
    public Vector2Int mapOffset;

    public UnityEvent OnWorldCreated;
    public UnityEvent OnNewChunksGenerated;

    public WorldData worldData { get; private set; }
    private void OnValidate()
    {
        worldData = new WorldData
        {
            chunkHeight = this.chunkHeight,
            chunkSize = this.chunkSize,
            chunkDataDict = new Dictionary<Vector3Int, ChunkData>(),
            chunkDict = new Dictionary<Vector3Int, ChunkRenderer>()
        };
    }

    public void GenerateWorld()
    {
        // chunkDataDict.Clear();
        // foreach (ChunkRenderer chunk in chunkDict.Values)
        // {
        //     DestroyImmediate(chunk.gameObject);
        // }
        //
        // foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
        // {
        //     DestroyImmediate(chunk.gameObject);
        // }
        // chunkDict.Clear();
        
        WorldGenerationData worldGenerationData = GetPositionsInRenderDistance(Vector3Int.zero);

        
        // Generate data chunks
        foreach (var pos in worldGenerationData.chunkDataPositionsToCreate)
        {
            var data = new ChunkData(chunkSize, chunkHeight, this, pos);
            var newData = terrainGenerator.GenerateChunkData(data, mapOffset);
            worldData.chunkDataDict.Add(newData.worldPos, newData);
        }

        // Generate visual chunks
        foreach (var pos in worldGenerationData.chunkPositionsToCreate)
        {
            var data = worldData.chunkDataDict[pos];
            var meshData = Chunk.GetChunkMeshData(data);
            var chunkObj = Instantiate(chunkPrefab, data.worldPos, Quaternion.identity);
            var chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
            worldData.chunkDict.Add(data.worldPos, chunkRenderer);
            chunkRenderer.Initialize(data);
            chunkRenderer.UpdateChunk(meshData);
        }

        if (Application.isPlaying)
        {
            OnWorldCreated?.Invoke();
        }
    }

    private WorldGenerationData GetPositionsInRenderDistance(Vector3Int playerPos)
    {
        var allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsInRenderDistance(this, playerPos);
        var allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsInRenderDistance(this, playerPos);
        
        var chunkPositionsToCreate = WorldDataHelper.GetPositionsToCreate(worldData, allChunkPositionsNeeded,playerPos);
        var chunkDataPositionsToCreate = WorldDataHelper.GetDataPositionsToCreate(worldData, allChunkDataPositionsNeeded,playerPos);

        var data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = new List<Vector3Int>(),
            chunkDataToRemove = new List<Vector3Int>()
        };
        
        return data;
    }

    public BlockType GetBlock(ChunkData chunkData, Vector3Int pos)
    {
        var chunkPos = Chunk.ChunkPosFromBlockCoords(this, pos);

        worldData.chunkDataDict.TryGetValue(chunkPos, out var containerChunk);
        
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
    
    public struct WorldGenerationData
    {
        public List<Vector3Int> chunkPositionsToCreate;
        public List<Vector3Int> chunkDataPositionsToCreate;
        public List<Vector3Int> chunkPositionsToRemove;
        public List<Vector3Int> chunkDataToRemove;
    }

    public struct WorldData
    {
        public Dictionary<Vector3Int, ChunkData> chunkDataDict;
        public Dictionary<Vector3Int, ChunkRenderer> chunkDict;
        public int chunkSize;
        public int chunkHeight;
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
                world.worldData.chunkDataDict.Clear();
                world.worldData.chunkDict.Clear();
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
