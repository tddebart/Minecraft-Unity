using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public partial class World : MonoBehaviour
{
    public static World Instance;
    public byte renderDistance = 6;
    [Space]
    [Header("Sizes")]
    public byte chunkSize = 16; // 16x16 chunks
    public byte chunkHeight = 100; // 100 blocks high
    [RangeEx(16, 240, 16)]
    public byte worldHeight = 240; // 256 blocks high
    [Space]
    public byte chunksPerFrame = 2;
    public int chunksGenerationPerFrame = 4;
    public WorldRenderer worldRenderer;
    
    public TerrainGenerator terrainGenerator;
    public Vector3Int mapSeedOffset;
    
    public bool GenerateMoreChunks = true;
    [Space]
    [Header("Lighting")] 
    [Range(0, 1)]
    public float globalLightLevel = 1f;
    public Color dayColor;
    public Color nightColor;
    public float minLightLevel = 0.1f;
    public float maxLightLevel = 0.90f;
    public float lightFalloff = 0.08f;

    private CancellationTokenSource taskTokenSource = new CancellationTokenSource();

    public UnityEvent OnWorldCreated;
    public UnityEvent OnNewChunksGenerated;

    public WorldData worldData { get; private set; }
    public bool IsWorldCreated { get; private set; }
    
    private Stopwatch fullStopwatch = new Stopwatch();
    
    public Dictionary<Vector3Int, BlockType> blocksToPlaceAfterGeneration = new Dictionary<Vector3Int, BlockType>();

    public bool validateDone;


    private void Awake()
    {
        Instance = this;
        validateDone = false;
        OnValidate();
        ClearVisable();
        GenerateWorld();
    }

    private void OnValidate()
    {
        Update();
        if (!Application.isPlaying || !validateDone)
        {
            worldData = new WorldData
            {
                chunkDataDict = new Dictionary<Vector3Int, ChunkData>(),
                chunkDict = new Dictionary<Vector3Int, ChunkRenderer>()
            };
            Instance = this;
            
            Shader.SetGlobalFloat("minGlobalLightLevel", minLightLevel);
            Shader.SetGlobalFloat("maxGlobalLightLevel", maxLightLevel);
            
            validateDone = true;
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
    
    private Task<ConcurrentDictionary<ChunkRenderer, MeshData>> UpdateMeshDataAsync(List<ChunkRenderer> dataToRender)
    {
        return Task.Run(() =>
        {
            var dict = new ConcurrentDictionary<ChunkRenderer, MeshData>();
            Parallel.ForEach(dataToRender, chunkRenderer =>
            {
                chunkRenderer.ModifiedByPlayer = true;
                var data = chunkRenderer.ChunkData.GetMeshData();
                dict.TryAdd(chunkRenderer, data);
            });
            return dict;
        });
    }
    
    public bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        var pos = WorldDataHelper.GetChunkPosition(this, Vector3Int.RoundToInt(hit.point));
        var chunk = WorldDataHelper.GetChunk(this, pos);

        var blockPos = GetBlockPos(hit);
        
        var blocksToDig = new List<Vector3Int>();
        
        // Dig out a 3x3x3 cube around the block
        // for (var x = -1; x <= 2; x++)
        // {
        //     for (var y = -1; y <= 2; y++)
        //     {
        //         for (var z = -1; z <= 2; z++)
        //         {
        //             var blockPosToDig = new Vector3Int(blockPos.x + x, blockPos.y + y, blockPos.z + z);
        //             blocksToDig.Add(blockPosToDig);
        //         }
        //     }
        // }
        //
        // SetBlocks(chunk, blocksToDig, blockType);

        SetBlock(chunk, blockPos, blockType);
        return true;
    }

    public void SetBlock(Vector3 pos, BlockType blockType)
    {
        SetBlock(Vector3Int.FloorToInt(pos), blockType);
    }
    
    public void SetBlock(Vector3Int blockPos, BlockType blockType)
    {
        var chunkPos = WorldDataHelper.GetChunkPosition(this, blockPos);
        var chunk = WorldDataHelper.GetChunk(this, chunkPos);
        SetBlock(chunk, blockPos, blockType);
    }

    public void SetBlocks(IEnumerable<Vector3Int> blockPoss, BlockType blockType)
    {
        var chunkPos = WorldDataHelper.GetChunkPosition(this, blockPoss.First());
        var chunk = WorldDataHelper.GetChunk(this, chunkPos);
        SetBlocks(chunk, blockPoss, blockType);
    }

    public async void SetBlocks(ChunkRenderer chunk, IEnumerable<Vector3Int> blockPoss, BlockType blockType)
    {
        var neightBourUpdates = new List<ChunkRenderer>();
        
        foreach (var pos in blockPoss)
        {
            // if (blockType == BlockType.Air)
            // {
            //     WorldDataHelper.SetBlock(chunk.ChunkData.worldRef, pos, Blocks.AIR);
            // }
            // else
            // {
                WorldDataHelper.SetBlock(chunk.ChunkData.worldRef, pos, blockType);
            // }
            
            if (chunk.ChunkData.IsOnEdge(pos))
            {
                List<ChunkData> neighbourChunkData = chunk.ChunkData.GetNeighbourChunk(pos);
                foreach (var neighChunkData in neighbourChunkData)
                {
                    if(neighChunkData == null) continue;
                    
                    ChunkRenderer neighbourChunk = WorldDataHelper.GetChunk(neighChunkData.worldRef, neighChunkData.worldPos);
                    if(neighbourChunk != null)
                    {
                        neightBourUpdates.Add(neighbourChunk);
                    }
                }
            }
        }
        neightBourUpdates.Add(chunk);

        var meshData = await UpdateMeshDataAsync(neightBourUpdates);
        
        foreach (var chunkRenderer in neightBourUpdates)
        {
            chunkRenderer.UpdateChunk(meshData[chunkRenderer]);
        }
    }

    public void SetBlock(ChunkRenderer chunk, Vector3Int blockPos, BlockType blockType)
    {
        SetBlocks(chunk, new[] {blockPos}, blockType);
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

    public Block GetBlock(Vector3Int globalPos)
    {
        var chunkPos = Chunk.ChunkPosFromBlockCoords(this, globalPos);

        worldData.chunkDataDict.TryGetValue(chunkPos, out var containerChunk);
        
        if (containerChunk == null)
        {
            return Blocks.NOTHING;
        }

        var blockPos = containerChunk.GetLocalBlockCoords(new Vector3Int(globalPos.x, globalPos.y, globalPos.z));
        return containerChunk.GetBlock(blockPos);
    }

    public Block GetBlock(Vector3 globalPos)
    {
        return GetBlock(Vector3Int.FloorToInt(globalPos));
    }

    public async void LoadAdditionalChunks(GameObject localPlayer)
    {
        if (!GenerateMoreChunks) return;
        
        // Debug.Log("Loading additional chunks");
        await GenerateWorld(Vector3Int.RoundToInt(localPlayer.transform.position));
        OnNewChunksGenerated?.Invoke();
    }
    
    private void ClearVisable()
    {
        worldData = new WorldData
        {
            chunkDataDict = new Dictionary<Vector3Int, ChunkData>(),
            chunkDict = new Dictionary<Vector3Int, ChunkRenderer>(),
        };
        worldData.chunkDataDict?.Clear();
        worldData.chunkDict?.Clear();
        worldRenderer.chunkPool?.Clear();
        
        MyNoise.noiseStopwatch.Reset();
        MyNoise.noise3DStopwatch.Reset();

        foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
        {
            DestroyImmediate(chunk.gameObject);
        }
    }

    public void OnDisable()
    {
        taskTokenSource.Cancel();
    }

    public struct WorldGenerationData
    {
        public HashSet<Vector3Int> chunkPositionsToCreate;
        public HashSet<Vector3Int> chunkDataPositionsToCreate;
        public HashSet<Vector3Int> chunkPositionsToRemove;
        public HashSet<Vector3Int> chunkDataToRemove;
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
                world.ClearVisable();
                world.GenerateWorld();
            }

            if (GUILayout.Button("Clear World"))
            {
                world.ClearVisable();
            }
            
            base.OnInspectorGUI();
        }
    }
#endif
}

public struct WorldData
{
    public Dictionary<Vector3Int, ChunkData> chunkDataDict;
    public Dictionary<Vector3Int, ChunkRenderer> chunkDict;
}
