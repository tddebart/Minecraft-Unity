using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreesLayerHandler : BlockLayerHandler
{
    public int terrainHeightLimit = 25;
    
    // INFO about leave generation:
    // minTrunkHeight = 4
    // maxTrunkHeight = 6
    // baseTrunkHeight = 5
    //
    // the leaves are 2 cubes
    // one cube with a width of 5 and a height of 2
    // with the corners randomly removed
    //
    // the second is a 2 high plus with corners randomly placed
    //
    // there will always be 3 logs hidden in the leaves
    
    public Vector3Int[] directions = {
        new(0, 0, 1),
        new(0, 0, -1),
        new(1, 0, 0),
        new(-1, 0, 0)
    };


    protected override bool TryHandling(ChunkData chunk, Vector3Int worldPos, Vector3Int localPos, int surfaceHeightNoise, Vector3Int mapSeedOffset)
    {
        if (chunk.worldPos.y < 0)
        {
            return false;
        }

        if (surfaceHeightNoise < terrainHeightLimit && chunk.treeData.treePositions.Contains(new Vector2Int(worldPos.x, worldPos.z)))
        {
            var blockCoords = new Vector3Int(localPos.x,surfaceHeightNoise-chunk.worldPos.y,localPos.z);
            if(blockCoords.y < 0)
            {
                return false;
            } 
            var type = chunk.GetBlock(blockCoords).type;

            if (type is BlockType.Grass or BlockType.Dirt)
            {
                // Check if there is a tree around us
                // I have disabled this for now because it is too slow
                //
                // for (var i = 0; i < 2; i++)
                // {
                //     var coords = new Vector3Int(localPos.x, surfaceHeightNoise+1+i, localPos.z);
                //     if (directions.Any(dir => chunk.GetBlock(coords + dir).type == BlockType.Log))
                //     {
                //         return false;
                //     }
                // }
                
                chunk.SetBlock(blockCoords, BlockType.Dirt);

                var treeHeight = Mathf.RoundToInt(Mathf.Lerp(5, 7, MyNoise.OctavePerlin(worldPos.x, worldPos.z, chunk.treeData.treeNoiseSettings)));

                for (var i = 1; i < treeHeight; i++)
                {
                    blockCoords.y = surfaceHeightNoise-chunk.worldPos.y + i;
                    chunk.SetBlock(blockCoords, BlockType.Log);
                }
                
                var leavePositions = new List<Vector3Int>();

                // First cube
                for (var x = localPos.x-2; x < localPos.x+3; x++)
                {
                    for (var z = localPos.z-2; z < localPos.z+3; z++)
                    {
                        if (x != localPos.x || z != localPos.z)
                        {
                            leavePositions.Add(new Vector3Int(x, blockCoords.y-2, z));
                            leavePositions.Add(new Vector3Int(x, blockCoords.y-1, z));
                        }
                    }
                }
                
                // Second plus
                for (var x = localPos.x-1; x < localPos.x+2; x++)
                {
                    for (var z = localPos.z-1; z < localPos.z+2; z++)
                    {
                        if (x != localPos.x-1 && x != localPos.x+1 || z != localPos.z-1 && z != localPos.z+1)
                        {
                            if (x != localPos.x || z != localPos.z)
                            {
                                leavePositions.Add(new Vector3Int(x, blockCoords.y, z));
                            }
                            leavePositions.Add(new Vector3Int(x, blockCoords.y+1, z));
                        }
                    }
                }
                
                // Check if there is enough space for the leaves
                foreach (var l in leavePositions)
                {
                    if (chunk.GetBlock(l).type is not BlockType.Air and not BlockType.Leaves and not BlockType.Log and not BlockType.Nothing)
                    {
                        RemoveTree();

                        return false;
                    }
                }

                void RemoveTree()
                {
                    blockCoords = new Vector3Int(localPos.x, surfaceHeightNoise, localPos.z);
                    chunk.SetBlock(blockCoords, type);
                    for (var i = 1; i < treeHeight; i++)
                    {
                        blockCoords.y = surfaceHeightNoise-chunk.worldPos.y + i;
                        chunk.SetBlock(blockCoords, BlockType.Air);
                    }
                }

                foreach (var leavePos in leavePositions)
                {
                    if (chunk.GetBlock(leavePos).type != BlockType.Log)
                    {
                        chunk.SetBlock(leavePos, BlockType.Leaves);
                        // TODO: this lags?
                        // if (!ChunkData.IsInRange(leavePos))
                        // {
                        //     World.Instance.AddChunkToUpdate(WorldDataHelper.GetChunkData(World.Instance, chunk.GetGlobalBlockCoords(leavePos)).renderer);
                        // }
                    }
                }

            }
        }
        
        return false;
    }
}