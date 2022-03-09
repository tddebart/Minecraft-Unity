using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public partial class World
{
    public async void GenerateWorld()
    {
        await GenerateWorld(Vector3Int.zero);
    }

    private async Task GenerateWorld(Vector3Int position)
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
        
        
        // foreach(var pos in worldGenerationData.chunkDataPositionsToCreate)
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
        
        try
        {
            dataDict = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task cancelled");
            return;
        }

        foreach (var calculatedData in dataDict)
        {
            worldData.chunkDataDict.Add(calculatedData.Key, calculatedData.Value);
        }
        
        Profiler.EndThreadProfiling();

        dataStopWatch.Stop();
        if (!Application.isPlaying)
        {
            Debug.Log($"Data generation took {dataStopWatch.ElapsedMilliseconds}ms");
        }


        var featureStopWatch = new Stopwatch();
        featureStopWatch.Start();
        
        // Generate features like trees
        // await CalculateFeatures(worldGenerationData.chunkPositionsToCreate);
        

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
            .Where(x => worldGenerationData.chunkPositionsToCreate.Contains(x.Key))
            .Select(x => x.Value)
            .ToList();

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
            Debug.Log($"Time spent generating noise {MyNoise.stopWatch.ElapsedMilliseconds}ms");
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDict, position));
        
        Profiler.EndThreadProfiling();
    }
    
    public UniTask<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            var dict = new ConcurrentDictionary<Vector3Int, MeshData>();
            Parallel.ForEach(dataToRender, data =>
            {
                if(taskTokenSource.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                
                var meshData = data.GetMeshData();
                dict.TryAdd(data.worldPos, meshData);
            });

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
            StartCoroutine(UpdateLoop());
            OnWorldCreated?.Invoke();
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
