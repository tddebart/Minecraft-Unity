
using System.Collections.Generic;
using UnityEngine;

public static class BiomeCenterFinder
{
    public static List<Vector2Int> neighbour8Directions = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1)
    };

    public static List<Vector3Int> CalculateBiomeCenters(Vector3 playerPos, int renderDistance, int chunkSize)
    {
        var biomeLength = 4 * chunkSize;
        
        var origin = new Vector3Int(Mathf.RoundToInt(playerPos.x/biomeLength)*biomeLength, 0, Mathf.RoundToInt(playerPos.z/biomeLength)*biomeLength);
        
        HashSet<Vector3Int> biomeCenters = new HashSet<Vector3Int>();
        
        biomeCenters.Add(origin);
        
        // This generates a list of all the biome centers which is a 5x5 square of points around the player
        for (var i = -2*(renderDistance/4); i < 2*(renderDistance/4); i++)
        {
            for (var j = -2*(renderDistance/4); j < 2*(renderDistance/4); j++)
            {
                var biomeCenter = new Vector3Int(origin.x + i * biomeLength, 0, origin.z + j * biomeLength);
                biomeCenters.Add(biomeCenter);
            }
        }

        return new List<Vector3Int>(biomeCenters);
    }
}
