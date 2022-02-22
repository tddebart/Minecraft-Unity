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
        GenerateWorld(Vector3Int.zero);
    }

    private void GenerateWorld(Vector3Int position)
    {
        WorldGenerationData worldGenerationData = GetPositionsInRenderDistance(position);

        foreach (var pos in worldGenerationData.chunkPositionsToRemove)
        {
            WorldDataHelper.RemoveChunk(this, pos);
        }
        
        foreach (var pos in worldGenerationData.chunkDataToRemove)
        {
            WorldDataHelper.RemoveChunkData(this, pos);
        }

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
        
        var chunkPositionsToRemove = WorldDataHelper.GetUnneededChunkPositions(worldData, allChunkPositionsNeeded);
        var chunkDataToRemove = WorldDataHelper.GetUnneededDataPositions(worldData, allChunkDataPositionsNeeded);
        

        var data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = chunkPositionsToRemove,
            chunkDataToRemove = chunkDataToRemove
        };
        
        return data;
    }
    
    public bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        var chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
        {
            return false;
        }
        
        var blockPos = GetBlockPos(hit);

        // // Dig out the blocks around the block
        // var blocksToDig = new List<Vector3Int>();
        // blocksToDig.Add(blockPos);
        // blocksToDig.Add(blockPos + Vector3Int.up);
        // blocksToDig.Add(blockPos + Vector3Int.down);
        // blocksToDig.Add(blockPos + Vector3Int.left);
        // blocksToDig.Add(blockPos + Vector3Int.right);
        // blocksToDig.Add(blockPos + Vector3Int.forward);
        // blocksToDig.Add(blockPos + Vector3Int.back);
        //
        // foreach (var pos in blocksToDig)
        // {
        //     SetBlock(chunk, pos, blockType);
        // }

        SetBlock(chunk, blockPos, blockType);
        return true;
    }

    public void SetBlock(ChunkRenderer chunk, Vector3Int blockPos, BlockType blockType)
    {
        WorldDataHelper.SetBlock(chunk.ChunkData.worldRef, blockPos, blockType);
        chunk.ModifiedByPlayer = true;

        if (Chunk.IsOnEdge(chunk.ChunkData, blockPos))
        {
            List<ChunkData> neighbourChunkData = Chunk.GetNeighbourChunk(chunk.ChunkData, blockPos);
            foreach (var neighChunkData in neighbourChunkData)
            {
                ChunkRenderer neighbourChunk = WorldDataHelper.GetChunk(neighChunkData.worldRef, neighChunkData.worldPos);
                if(neighbourChunk != null)
                {
                    neighbourChunk.UpdateChunk();
                }
            }
        }
        
        chunk.UpdateChunk();
    }
    
    public Vector3Int GetBlockPos(RaycastHit hit)
    {
        var pos = new Vector3(
            GetBlockPosIn(hit.point.x, hit.normal.x),
            GetBlockPosIn(hit.point.y, hit.normal.y),
            GetBlockPosIn(hit.point.z, hit.normal.z)
            );
        
        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPosIn(float pos, float normal)
    {
        if (Mathf.Abs(pos % 1) == 0.5f)
        {
            pos -= (normal / 2);
        }
        
        return pos;
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
        // Debug.Log("Loading additional chunks");
        GenerateWorld(Vector3Int.RoundToInt(localPlayer.transform.position));
        OnNewChunksGenerated?.Invoke();
    }
    
    public void RemoveChunk(ChunkRenderer chunk)
    {
        chunk.gameObject.SetActive(false);
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
                Clear();
                world.GenerateWorld();
            }

            if (GUILayout.Button("Clear World"))
            {
                Clear();
            }
            
            base.OnInspectorGUI();
        }

        private void Clear()
        {
            var world = target as World;
            world.worldData.chunkDataDict.Clear();
            world.worldData.chunkDict.Clear();
            foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
            {
                DestroyImmediate(chunk.gameObject);
            }
        }
    }
#endif
}
