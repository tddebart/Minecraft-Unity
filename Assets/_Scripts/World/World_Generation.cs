using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public partial class World
{
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

    public async void GenerateWorld(Vector3Int position, Action onGenerateNewChunks = null)
    {
        Profiler.BeginThreadProfiling("GenerateWorld", "GenerateWorld");
        if (!Application.isPlaying)
        {
            fullStopwatch.Start();
        }
        
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
        ConcurrentDictionary<Vector3Int, ChunkData> dataDict = new ConcurrentDictionary<Vector3Int, ChunkData>();

        var dataStopWatch = new Stopwatch();
        dataStopWatch.Start();
        
        Profiler.BeginThreadProfiling("GenerateWorld", "GenerateData");

        if (worldGenerationData.chunkPositionsToCreate.Count > 0)
        {
            try
            {
                dataDict.AddRange(await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }
        dataStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Data generation took {dataStopWatch.ElapsedMilliseconds}ms");
        }

        var loadStopWatch = Stopwatch.StartNew();
        if (worldGenerationData.chunkDataPositionsToLoad.Count > 0)
        {
            try
            {
                dataDict.AddRange(await LoadChunksAsync(worldGenerationData.chunkDataPositionsToLoad, worldName));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }
        
        loadStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Loading took {loadStopWatch.ElapsedMilliseconds}ms");
        }

        foreach (var calculatedData in dataDict)
        {
            worldData.chunkDataDict.Add(calculatedData.Key, calculatedData.Value);
        }
        
        Profiler.EndThreadProfiling();



        var featureStopWatch = new Stopwatch();
        featureStopWatch.Start();
        
        // Generate features like trees
        await CalculateFeatures(worldGenerationData.chunkPositionsToCreate);
        

        await Task.Run(() =>
        {
            Parallel.ForEach(blocksToPlaceAfterGeneration, (block) =>
            {
                WorldDataHelper.SetBlock(this, block.Key, block.Value);
            });
        });

        featureStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Feature generation took {featureStopWatch.ElapsedMilliseconds}ms");
        }

        var visualStopWatch = new Stopwatch();
        visualStopWatch.Start();
        
        // Generate visual chunks
        // ConcurrentDictionary<Vector3Int, MeshData> meshDataDict = 
        //     await CalculateChunkMeshData(worldGenerationData.chunkPositionsToCreate);
        ConcurrentDictionary<Vector3Int, MeshData> meshDataDict;
        
        List<ChunkData> dataToRender = worldData.chunkDataDict
            .Where(x => worldGenerationData.chunkPositionsToCreate.Contains(x.Key) || worldGenerationData.chunkPositionsToLoad.Contains(x.Key))
            .Select(x => x.Value)
            .ToList();

        await CastLightFirstTime(dataToRender);

        try
        {
            meshDataDict = await CreateMeshDataAsync(dataToRender);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task cancelled");
            return;
        }
        
        visualStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Mesh generation took {visualStopWatch.ElapsedMilliseconds}ms");
            Debug.Log($"Time spent generating noise {MyNoise.noiseStopwatch.ElapsedMilliseconds}ms");
            Debug.Log($"Time spent generating 3D noise {MyNoise.noise3DStopwatch.ElapsedMilliseconds}ms");
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDict, position));
        
        onGenerateNewChunks?.Invoke();
        
        Profiler.EndThreadProfiling();
    }
    
    public async void GenerateOnlyData(Vector3Int position, Action onDone = null)
    {
        terrainGenerator.GenerateBiomePoints(position, renderDistance, chunkSize, mapSeedOffset);
        
        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsInRenderDistance(position), taskTokenSource.Token);

        // Generate data chunks
        ConcurrentDictionary<Vector3Int, ChunkData> dataDict = new ConcurrentDictionary<Vector3Int, ChunkData>();

        var dataStopWatch = new Stopwatch();
        dataStopWatch.Start();
        
        Profiler.BeginThreadProfiling("GenerateWorld", "GenerateData");

        if (worldGenerationData.chunkPositionsToCreate.Count > 0)
        {
            try
            {
                dataDict.AddRange(await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }
        dataStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Data generation took {dataStopWatch.ElapsedMilliseconds}ms");
        }

        var loadStopWatch = Stopwatch.StartNew();
        if (worldGenerationData.chunkDataPositionsToLoad.Count > 0)
        {
            try
            {
                dataDict.AddRange(await LoadChunksAsync(worldGenerationData.chunkDataPositionsToLoad, worldName));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Task cancelled");
                return;
            }
        }
        
        loadStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Loading took {loadStopWatch.ElapsedMilliseconds}ms");
        }

        foreach (var calculatedData in dataDict)
        {
            worldData.chunkDataDict.Add(calculatedData.Key, calculatedData.Value);
        }
        
        Profiler.EndThreadProfiling();



        var featureStopWatch = new Stopwatch();
        featureStopWatch.Start();
        
        // Generate features like trees
        await CalculateFeatures(worldGenerationData.chunkPositionsToCreate);
        

        await Task.Run(() =>
        {
            Parallel.ForEach(blocksToPlaceAfterGeneration, (block) =>
            {
                WorldDataHelper.SetBlock(this, block.Key, block.Value);
            });
        });

        featureStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Feature generation took {featureStopWatch.ElapsedMilliseconds}ms");
        }
        
        onDone?.Invoke();
    }

    // public async void GenerateOnlyMesh()
    // {
    //     // Generate visual chunks
    //     // ConcurrentDictionary<Vector3Int, MeshData> meshDataDict = 
    //     //     await CalculateChunkMeshData(worldGenerationData.chunkPositionsToCreate);
    //     ConcurrentDictionary<Vector3Int, MeshData> meshDataDict;
    //     
    //     List<ChunkData> dataToRender = worldData.chunkDataDict
    //         .Select(x => x.Value)
    //         .ToList();
    //
    //     await CastLightFirstTime(dataToRender);
    //
    //     try
    //     {
    //         meshDataDict = await CreateMeshDataAsync(dataToRender);
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         Debug.Log("Task cancelled");
    //         return;
    //     }
    //
    //     StartCoroutine(ChunkCreationCoroutine(meshDataDict, Vector3Int.zero));
    // }

    public UniTask CastLightFirstTime(List<ChunkData> dataToCast)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            Parallel.ForEach(dataToCast, Lighting.RecastSunLightFirstTime);
        });
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

    // private Task<ConcurrentDictionary<Vector3Int, MeshData>> CalculateChunkMeshData(List<Vector3Int> chunkPositionsToCreate)
    // {
    //     return Task.Run(() =>
    //     {
    //         var meshDataDict = new ConcurrentDictionary<Vector3Int, MeshData>();
    //         
    //         Parallel.ForEach(chunkPositionsToCreate, pos =>
    //         {
    //             var data = worldData.chunkDataDict[pos];
    //             var meshData = Chunk.GetChunkMeshData(data);
    //             meshDataDict.TryAdd(pos, meshData);
    //         });
    //         
    //         return meshDataDict;
    //     });
    // }

    private UniTask<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(HashSet<Vector3Int> chunkDataPositionsToCreate)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            Profiler.BeginThreadProfiling("MyThreads","CalculateWorldChunkData");
            var dataDict = new ConcurrentDictionary<Vector3Int, ChunkData>();
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
            
            Parallel.ForEach(chunkDataPositionsToCreate, pos =>
            {
                if(taskTokenSource.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }

                var data = new ChunkData(chunkSize, chunkHeight, this, pos);
                var newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                dataDict.TryAdd(pos, newData);

            });
            
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
            return dataDict;
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
            NetworkClient.Send(new WorldServer.SpawnPlayerMessage(SteamClient.SteamId));
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
