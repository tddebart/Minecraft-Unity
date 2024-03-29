﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public partial class World : MonoBehaviour
{
    public static World Instance;
    public int renderDistance = 6;
    [Space]
    [Header("Sizes")]
    public byte chunkSize = 16; // 16x16 chunks
    public byte chunkHeight = 16; // 100 blocks high
    [RangeEx(16, 240, 16)]
    public byte worldHeight = 96; // 256 blocks high
    [Space]
    public byte chunksPerFrame = 1;
    public int chunksGenerationPerFrame = 1;
    public WorldRenderer worldRenderer;
    
    public TerrainGenerator terrainGenerator;
    public Vector3Int mapSeedOffset;
    
    public bool GenerateMoreChunks = true;
    [Space]
    [Header("Lighting")] 
    [Range(0, 1)]
    public float gamma = 0.0f;
    [Range(0.15f, 1)]
    public float skyLightMultiplier = 0.75f;
    public float blockLightMultiplier = 1.5f;
    [Space]
    public Color dayColor;
    public Color nightColor;
    public string worldName;
    [HideInInspector]
    public bool disabled;
    public bool binaryCompressSaves = true;

    private CancellationTokenSource taskTokenSource = new CancellationTokenSource();

    public UnityEvent OnWorldCreated;
    public UnityEvent OnNewChunksGenerated;
    
    public WorldData worldData;
    public bool IsWorldCreated { get; private set; }
    
    private Stopwatch fullStopwatch = new Stopwatch();
    
    public Dictionary<Vector3Int, BlockType> blocksToPlaceAfterGeneration = new Dictionary<Vector3Int, BlockType>();

    public bool validateDone;
    [HideInInspector] public bool isInPlayMode;


    private void Awake()
    {
        Instance = this;
        validateDone = false;
        isInPlayMode = Application.isPlaying;
    }

    private void Start()
    {
        OnValidate();
        Clear();
        // GenerateWorld();
        
        NetworkClient.RegisterHandler<StartWorldMessage>(message =>
        {
            StartWorld();
            GenerateWorld(message.position);
        });
        
        // NetworkClient.RegisterHandler<WorldServer.ChunkReceiveMessage>(message =>
        // {
        //     Debug.Log("Received chunk");
        //     
        //     var chunkData = ChunkData.Deserialize(message.chunkSaveData);
        //
        //     worldData.chunkDataDict[chunkData.worldPos] = chunkData;
        //
        //     GenerateOnlyMesh();
        // });
    }

    private void OnValidate()
    {
        Update();
        if (!Application.isPlaying || !validateDone)
        {
            worldData = new WorldData
            {
                chunkDataDict = new Dictionary<Vector3Int, ChunkData>(),
                chunkDict = new Dictionary<Vector3Int, ChunkRenderer>(),
                worldName = worldName,
            };
            Instance = this;
            

            validateDone = true;
        }
        UpadteLightTexture();
    }

    public void UpadteLightTexture()
    {
        LightTextureCreator.gamma = gamma;
        LightTextureCreator.skyLightMultiplier = skyLightMultiplier;
        LightTextureCreator.blockLightMultiplier = blockLightMultiplier;
        LightTextureCreator.CreateLightTexture();
        Shader.SetGlobalVectorArray("lightColors", LightTextureCreator.lightColors);
    }

    private WorldGenerationData GetPositionsInRenderDistance(Vector3Int playerPos)
    {
        var allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsInRenderDistance(this, playerPos);
        var allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsInRenderDistance(this, playerPos);
        
        var chunkPositionsToCreate = WorldDataHelper.GetPositionsToCreate(worldData, allChunkPositionsNeeded,playerPos);
        var chunkDataPositionsToCreate = WorldDataHelper.GetDataPositionsToCreate(worldData, allChunkDataPositionsNeeded,playerPos);
        
        var chunkPositionsToLoad = WorldDataHelper.GetPositionsToLoad(worldData, allChunkPositionsNeeded,playerPos);
        var chunkDataPositionsToLoad = WorldDataHelper.GetDataPositionsToLoad(worldData, allChunkDataPositionsNeeded,playerPos);
        
        var chunkPositionsToRemove = WorldDataHelper.GetUnneededChunkPositions(worldData, allChunkPositionsNeeded);
        var chunkDataToRemove = WorldDataHelper.GetUnneededDataPositions(worldData, allChunkDataPositionsNeeded);
        

        var data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToLoad = chunkPositionsToLoad,
            chunkDataPositionsToLoad = chunkDataPositionsToLoad,
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

    // public void UpdateChunk(Vector3Int chunkPos)
    // {
    //     UpdateChunks(new [] {chunkPos});
    // }
    //
    // public async void UpdateChunks(IEnumerable<Vector3Int> chunkPositions)
    // {
    //     var dataToRender = new List<ChunkRenderer>();
    //     foreach (var chunkPos in chunkPositions)
    //     {
    //         worldData.chunkDict.TryGetValue(chunkPos, out var chunk);
    //         if (chunk != null)
    //         {
    //             dataToRender.Add(chunk);
    //         }
    //         else
    //         {
    //             Debug.LogError("Chunk not found at pos: " + chunkPos);
    //         }
    //     }
    //     
    //     var meshData = await UpdateMeshDataAsync(dataToRender);
    //     foreach (var chunkRenderer in dataToRender)
    //     {
    //         meshData.TryGetValue(chunkRenderer, out var data);
    //         chunkRenderer.UpdateChunk(data);
    //     }
    // }

    public void SetBlock(Vector3 pos, BlockType blockType)
    {
        SetBlock(Vector3Int.FloorToInt(pos), blockType);
    }
    
    public void SetBlock(Vector3Int blockPos, BlockType blockType)
    {
        var chunkPos = WorldDataHelper.GetChunkPosition(this, blockPos);
        var chunk = WorldDataHelper.GetChunk(this, chunkPos);
        if (chunk == null) return;
        chunk.ModifiedByPlayer = true;
        chunk.ChunkData.modifiedAfterSave = true;
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

        if (worldData.chunkDataDict.ContainsKey(chunkPos))
        {
            var containerChunk = worldData.chunkDataDict[chunkPos];

            var blockPos = containerChunk.GetLocalBlockCoords(new Vector3Int(globalPos.x, globalPos.y, globalPos.z));
            return containerChunk.GetBlock(blockPos);
        }
        return BlockHelper.NOTHING;
    }

    public Block GetBlock(Vector3 globalPos)
    {
        return GetBlock(Vector3Int.FloorToInt(globalPos));
    }

    public void LoadAdditionalChunks(GameObject localPlayer)
    {
        if (!GenerateMoreChunks) return;
        
        // Debug.Log("Loading additional chunks");
        GenerateWorld(Vector3Int.RoundToInt(localPlayer.transform.position), () =>
        {
            OnNewChunksGenerated?.Invoke();
        });
    }
    
    private void Clear()
    {
        worldData = new WorldData
        {
            chunkDataDict = new Dictionary<Vector3Int, ChunkData>(),
            chunkDict = new Dictionary<Vector3Int, ChunkRenderer>(),
            worldName = worldName
        };
        worldData.chunkDataDict?.Clear();
        worldData.chunkDict?.Clear();
        worldRenderer.chunkPool?.Clear();
        isSaving = false;
        
        chunksToUpdate?.Clear();
        
        MyNoise.noiseStopwatch.Reset();
        MyNoise.noise3DStopwatch.Reset();

        foreach (var chunk in FindObjectsOfType<ChunkRenderer>())
        {
            DestroyImmediate(chunk.gameObject);
        }
    }

    public void OnDisable()
    {
        updateThread?.Abort();
        taskTokenSource.Cancel();
        disabled = true;
    }

    public struct WorldGenerationData
    {
        public HashSet<Vector3Int> chunkPositionsToCreate;
        public HashSet<Vector3Int> chunkDataPositionsToCreate;
        public HashSet<Vector3Int> chunkPositionsToLoad;
        public HashSet<Vector3Int> chunkDataPositionsToLoad;
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
                world.Clear();
                world.GenerateWorld();
            }

            if (GUILayout.Button("Clear World"))
            {
                world.Clear();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }

            if (GUILayout.Button("Save World"))
            {
                world.SaveWorld();
            }

            base.OnInspectorGUI();
        }
    }
#endif
}

public struct StartWorldMessage : NetworkMessage
{
    public Vector3Int position;
    
    public StartWorldMessage(Vector3Int position)
    {
        this.position = position;
    }
}

public struct WorldData
{
    public string worldName;
    public Dictionary<Vector3Int, ChunkData> chunkDataDict;
    public Dictionary<Vector3Int, ChunkRenderer> chunkDict;
}
