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
    //
    // for now i will just make a array with the basic non random leaves
    
    // pos is here in local space
    protected override bool TryHandling(ChunkData chunk, Vector3Int pos, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (chunk.worldPos.y < 0)
        {
            return false;
        }

        if (surfaceHeightNoise < terrainHeightLimit &&
            chunk.treeData.treePositions.Contains(new Vector2Int(chunk.worldPos.x+pos.x, chunk.worldPos.z+pos.z)))
        {
            var blockCoords = new Vector3Int(pos.x,surfaceHeightNoise,pos.z);
            var type = Chunk.GetBlock(chunk, blockCoords);

            if (type is BlockType.Grass or BlockType.Dirt)
            {
                Chunk.SetBlock(chunk, blockCoords, BlockType.Dirt);

                for (var i = 1; i < 6; i++)
                {
                    blockCoords.y = surfaceHeightNoise + i;
                    Chunk.SetBlock(chunk, blockCoords, BlockType.Log);
                }
                
                var leavePositions = new List<Vector3Int>();

                // First cube
                for (var x = pos.x-2; x < pos.x+3; x++)
                {
                    for (var z = pos.z-2; z < pos.z+3; z++)
                    {
                        if (x != pos.x || z != pos.z)
                        {
                            leavePositions.Add(new Vector3Int(x, blockCoords.y-2, z));
                            leavePositions.Add(new Vector3Int(x, blockCoords.y-1, z));
                        }
                    }
                }
                
                // Second plus
                for (var x = pos.x-1; x < pos.x+2; x++)
                {
                    for (var z = pos.z-1; z < pos.z+2; z++)
                    {
                        if (x != pos.x-1 && x != pos.x+1 || z != pos.z-1 && z != pos.z+1)
                        {
                            if (x != pos.x || z != pos.z)
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
                    if (Chunk.GetBlock(chunk, l) is not BlockType.Air and not BlockType.Log and not BlockType.Nothing)
                    {
                        blockCoords = new Vector3Int(pos.x, surfaceHeightNoise, pos.z);
                        Chunk.SetBlock(chunk, blockCoords, type);
                        for (var i = 1; i < 6; i++)
                        {
                            blockCoords.y = surfaceHeightNoise + i;
                            Chunk.SetBlock(chunk, blockCoords, BlockType.Air);
                        }

                        return false;
                    }
                }

                foreach (var leavePos in leavePositions)
                {
                    chunk.treeData.leavePositions.Add(leavePos);
                }

            }
        }
        
        return false;
    }
}