using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public partial class World
{
    public readonly ConcurrentQueue<Vector3Int> doneDataQueue = new();
    public readonly ConcurrentQueue<ChunkData> dataToMeshQueue = new();  
    public readonly ConcurrentQueue<KeyValuePair<Vector3Int,MeshData>> meshToRenderQueue = new();
    public readonly Dictionary<Vector3Int, Action> actionOnChunkDone = new();

    public readonly Stopwatch dataStopwatch = new();
    public readonly Stopwatch loadStopwatch = new();
    public readonly Stopwatch featureStopwatch = new();
    public readonly Stopwatch meshStopwatch = new();

    public void GenerateWorld()
    {
        StartWorld();
        GenerateWorld(Vector3Int.zero);
    }

    public void StartWorld()
    {
        OnValidate();
        IsWorldCreated = false;

        updateThread?.Abort();
        updateThread = new Thread(UpdateLoop);
        updateThread.Start();
    }

    public async void GenerateWorld(Vector3Int position)
    {
        Profiler.BeginThreadProfiling("GenerateWorld", "GenerateWorld");

        fullStopwatch.Start();

        terrainGenerator.GenerateBiomePoints(position, renderDistance, chunkSize, mapSeedOffset);
        
        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsInRenderDistance(position), taskTokenSource.Token);
        
        foreach (var pos in worldGenerationData.chunkPositionsToRemove)
        {
            WorldDataHelper.RemoveChunk(this, pos);
        }
        
        foreach (var pos in worldGenerationData.chunkDataToRemove)
        {
            WorldDataHelper.RemoveChunkData(this, pos);
        }

        // Generate data chunks
        
        
        Profiler.BeginThreadProfiling("GenerateWorld", "GenerateData");

        
        if (worldGenerationData.chunkDataPositionsToCreate.Count > 0)
        {
            try
            {
                CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }

        
        if (worldGenerationData.chunkDataPositionsToLoad.Count > 0)
        {
            try
            {
                LoadChunksAsync(worldGenerationData.chunkDataPositionsToLoad, worldName);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }

        Profiler.EndThreadProfiling();
        
        Profiler.EndThreadProfiling();
    }

    public UniTask<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            var dict = new ConcurrentDictionary<Vector3Int, MeshData>();
            if (IsWorldCreated)
            {
                while (dataToRender.Count > 0)
                {
                    Parallel.For(0, 1, i =>
                    {
                        if(taskTokenSource.IsCancellationRequested)
                        {
                            taskTokenSource.Token.ThrowIfCancellationRequested();
                        }

                        var data = dataToRender.First();
                        var meshData = data.GetMeshData();
                        dict.TryAdd(data.worldPos, meshData);
                        dataToRender.RemoveAt(0);
                    });
                    UniTask.NextFrame();
                }
            }
            else
            {
                Parallel.ForEach(dataToRender, data =>
                {
                    if(taskTokenSource.IsCancellationRequested)
                    {
                        taskTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    
                    var meshData = data.GetMeshData();
                    dict.TryAdd(data.worldPos, meshData);
                });
            }

            // foreach (var data in dataToRender)
            // {
            //     if(taskTokenSource.IsCancellationRequested)
            //     {
            //         taskTokenSource.Token.ThrowIfCancellationRequested();
            //     }
            //     
            //     var meshData = data.GetMeshData();
            //     dict.TryAdd(data.worldPos, meshData);
            // }

            return dict;
        }, true, taskTokenSource.Token);
    }

    public void CreateMeshData(ChunkData dataToRender)
    {
        var meshData = dataToRender.GetMeshData();
        meshToRenderQueue.Enqueue(new KeyValuePair<Vector3Int, MeshData>(dataToRender.worldPos,meshData));
    }

    private UniTask CalculateWorldChunkData(HashSet<Vector3Int> chunkDataPositionsToCreate)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            Profiler.BeginThreadProfiling("MyThreads","CalculateWorldChunkData");
            // var dataDict = new ConcurrentDictionary<Vector3Int, ChunkData>();
            // while (chunkDataPositionsToCreate.Count > 0)
            // {
            //     if(chunkDataPositionsToCreate.Count < chunksGenerationPerFrame)
            //     {
            //         chunksGenerationPerFrame = chunkDataPositionsToCreate.Count;
            //     }
            //     
            //     Parallel.For(0,chunksGenerationPerFrame, i =>
            //     {
            //         if(taskTokenSource.IsCancellationRequested)
            //         {
            //             taskTokenSource.Token.ThrowIfCancellationRequested();
            //         }
            //
            //         var pos = chunkDataPositionsToCreate.First();
            //         var data = new ChunkData(chunkSize, chunkHeight, this, pos);
            //         var newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
            //         dataDict.TryAdd(pos, newData);
            //         
            //         chunkDataPositionsToCreate.Remove(pos);
            //     });
            //     UniTask.NextFrame();
            // }
            dataStopwatch.Start();
            Parallel.ForEach(chunkDataPositionsToCreate,new ParallelOptions {MaxDegreeOfParallelism = 8}, pos =>
            {
                if(taskTokenSource.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }

                var data = new ChunkData(chunkSize, chunkHeight, this, pos);
                var newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                if (worldData.chunkDataDict.TryAdd(pos, newData))
                {
                    doneDataQueue.Enqueue(pos);
                }
                // else
                // {
                //     Debug.LogError($"Failed to add chunk data '{pos}' to doneData");
                // }
            });
            dataStopwatch.Stop();

            // foreach(var pos in chunkDataPositionsToCreate)
            // {
            //     if(taskTokenSource.IsCancellationRequested)
            //     {
            //         taskTokenSource.Token.ThrowIfCancellationRequested();
            //     }
            //     
            //     var data = new ChunkData(chunkSize, chunkHeight, this, pos);
            //     var newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
            //     dataDict.TryAdd(pos, newData);
            //
            // };

            Profiler.EndThreadProfiling();
        }, true, taskTokenSource.Token);
    }

    private UniTask CalculateFeatures(HashSet<Vector3Int> chunkDataPositionsToCreate)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            Parallel.ForEach(chunkDataPositionsToCreate, pos =>
            {
                if(taskTokenSource.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                
                var data = worldData.chunkDataDict[pos];
                terrainGenerator.GenerateFeatures(data, mapSeedOffset);
                data.isGenerated = true;
            });
        },true, taskTokenSource.Token);
    }
    
    private void CalculateFeature(Vector3Int pos)
    {
        var data = worldData.chunkDataDict[pos];
        terrainGenerator.GenerateFeatures(data, mapSeedOffset);
        data.isGenerated = true;
        
        actionOnChunkDone.TryGetValue(pos, out var action);
        action?.Invoke();
        
        if (!isInPlayMode)
        {
            dataToMeshQueue.Enqueue(data);
        }
    }

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataConDict, Vector3Int playerPos)
    {
        var meshDataDict = meshDataConDict.OrderBy(pair => Vector3Int.Distance(playerPos, pair.Key));
        for (var i = 0; i < meshDataDict.Count(); i++)
        {
            var item = meshDataDict.ElementAt(i);
            CreateChunk(worldData, item.Key, item.Value);
            if (Application.isPlaying && i % chunksPerFrame == 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        if (!IsWorldCreated && Application.isPlaying)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
            // NetworkClient.Send(new WorldServer.SpawnPlayerMessage(SteamClient.SteamId));
        }

        if (!Application.isPlaying)
        {
            fullStopwatch.Stop();
            Debug.Log($"World created in {fullStopwatch.ElapsedMilliseconds}ms");
            fullStopwatch.Reset();
        }
    }

    private void CreateChunk(WorldData worldData, Vector3Int pos, MeshData meshData)
    {
        var chunkRenderer = worldRenderer.RenderChunk(worldData, pos, meshData);
        worldData.chunkDict.TryAdd(pos, chunkRenderer);
    }
}
