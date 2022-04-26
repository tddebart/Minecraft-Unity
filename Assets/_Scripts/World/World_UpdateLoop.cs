using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class World
{
    public Queue<Block> blockToUpdate = new Queue<Block>();
    [HideInInspector]
    public List<ChunkRenderer> chunksToUpdate = new List<ChunkRenderer>();
    public Queue<ChunkRenderer.MeshChunkObject> chunksToUpdateMesh = new();
    public object chunkUpdateThreadLock = new object();
    private Thread updateThread;

    public readonly Vector3Int[] neighborOffsets =
    {
        new(-16, 0, -16),
        new(-16, 0, +16),
        new(+16, 0, -16),
        new(+16, 0, +16),
        new(-16, 0, 0),
        new(+16, 0, 0),
        new(0, 0, -16),
        new(0, 0, +16)
    };

    public void UpdateLoop()
    {
        while (!disabled)
        {
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
            
            // World generation queues
            if (doneDataQueue.Count > 0)
            {
                try
                {
                    // Parallel.For(0, doneDataQueue.Count, _ =>
                    // {
                        doneDataQueue.TryDequeue(out var chunkPos);

                        // Check if neighbor chunks are ready
                        if (neighborOffsets.All(o => worldData.chunkDataDict.ContainsKey(chunkPos + o)))
                        {
                            featureStopwatch.Start();
                            CalculateFeature(chunkPos);
                            featureStopwatch.Stop();
                        }
                        else
                        {
                            doneDataQueue.Enqueue(chunkPos);
                        }
                    // });
                }
                catch (ThreadAbortException)
                {
                }
            }

            if (dataToMeshQueue.Count > 0)
            {
                // Parallel.For(0, dataToMeshQueue.Count, _ =>
                // {
                    meshStopwatch.Start();
                    dataToMeshQueue.TryDequeue(out var chunkData);
                 
                    if(neighborOffsets.All(o => worldData.chunkDataDict.ContainsKey(chunkData.worldPos + o) /*&& worldData.chunkDataDict[chunkData.worldPos + o].isGenerated*/))
                    {
                        Lighting.RecastSunLightFirstTime(chunkData);
                        
                        CreateMeshData(chunkData);
                    } 
                    else
                    {
                        dataToMeshQueue.Enqueue(chunkData);
                    }   
                    
                    meshStopwatch.Stop();
                // });
            }
        }
    }

    public void UpdateChunks()
    {
        lock (chunkUpdateThreadLock)
        {
            chunksToUpdate[0]?.UpdateChunkAsync();
            chunksToUpdate.RemoveAt(0);
        }
    }
    
    public void AddChunkToUpdate(ChunkRenderer chunk, bool first = false)
    {
        lock (chunkUpdateThreadLock)
        {
            if(!chunksToUpdate.Contains(chunk))
            {
                if (first)
                {
                    chunksToUpdate.Insert(0, chunk);
                }
                else
                {
                    chunksToUpdate.Add(chunk);
                }
                
            }
        }
    }

    #if UNITY_EDITOR
    public static void EditorUpdate()
    {
        try
        {
            if (Application.isPlaying) return;

            var world = Instance;
            // Debug.Log("Editor Update");
            if (world.meshToRenderQueue.Count > 0)
            {
                world.meshToRenderQueue.TryDequeue(out var data);
                world.CreateChunk(world.worldData, data.Key, data.Value);

                if (!world.IsWorldCreated && world.worldData.chunkDict.Count == WorldDataHelper.GetChunkPositionsInRenderDistance(Instance, Vector3Int.zero).Count)
                {
                    world.IsWorldCreated = true;
                    // if (Application.isPlaying)
                    // {
                    //     world.OnWorldCreated?.Invoke();
                    //     NetworkClient.Send(new WorldServer.SpawnPlayerMessage(SteamClient.SteamId));
                    // }

                    Debug.Log($"Data generation took {world.dataStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"Loading took {world.loadStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"Feature generation took {world.featureStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"Mesh generation took {world.meshStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"Time spent generating noise {MyNoise.noiseStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"Time spent generating 3D noise {MyNoise.noise3DStopwatch.ElapsedMilliseconds}ms");
                    Debug.Log($"World created in {world.fullStopwatch.ElapsedMilliseconds}ms");
                    world.fullStopwatch.Stop();
                    world.fullStopwatch.Reset();
                    world.dataStopwatch.Stop();
                    world.dataStopwatch.Reset();
                    world.loadStopwatch.Stop();
                    world.loadStopwatch.Reset();
                    world.featureStopwatch.Stop();
                    world.featureStopwatch.Reset();
                    world.meshStopwatch.Stop();
                    world.meshStopwatch.Reset();
                }
            }
        }
        catch (NullReferenceException e)
        {
            if (SceneManager.GetActiveScene().name == "World")
            {
                Debug.LogError(e);
            }
            EditorApplication.update -= EditorUpdate;
        }
    }
    #endif

    private void Update()
    {
        if(WorldServer.IsDedicated) return;

        //TODO: remove this because it is slow
        Camera.main.backgroundColor = Color.Lerp(nightColor,dayColor , skyLightMultiplier);
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            NetworkClient.Send(new WorldServer.ChunkRequestMessage(Vector3Int.zero, renderDistance));
        }

        if (chunksToUpdateMesh.Count > 0)
        {
            var data = chunksToUpdateMesh.Dequeue();
            data.chunk.RenderMesh(data.meshData);
        }

        if (meshToRenderQueue.Count > 0)
        {
            meshToRenderQueue.TryDequeue(out var data);
            WorldDataHelper.RemoveChunk(this,data.Key);
            CreateChunk(worldData, data.Key, data.Value);

            if (!IsWorldCreated)
            {
                IsWorldCreated = true;
                OnWorldCreated?.Invoke();
                NetworkClient.Send(new WorldServer.SpawnPlayerMessage(SteamClient.SteamId));

                fullStopwatch.Stop();
                Debug.Log($"World created in {fullStopwatch.ElapsedMilliseconds}ms");
                fullStopwatch.Reset();
            }
            UniTask.NextFrame();
        }
    }

    public void UpdateBlocks()
    {
        while (blockToUpdate.Count > 0)
        {
            Block block = blockToUpdate.Dequeue();
            block.OnBlockUpdate();
        }
    }
}
