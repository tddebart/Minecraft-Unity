
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
        var biomeLength = 5 * chunkSize;
        
        var origin = new Vector3Int(Mathf.RoundToInt(playerPos.x/biomeLength)*biomeLength, 0, Mathf.RoundToInt(playerPos.z/biomeLength)*biomeLength);
        
        HashSet<Vector3Int> biomeCenters = new HashSet<Vector3Int>();
        
        biomeCenters.Add(origin);

        var extra = renderDistance / 5;
        extra = extra < 1 ? 1 : extra;
        
        // This generates a list of all the biome centers which is a 5x5 square of points around the player
        for (var i = -3*extra; i < 2*extra; i++)
        {
            for (var j = -3*extra; j < 2*extra; j++)
            {
                var biomeCenter = new Vector3Int(origin.x + i * biomeLength, 0, origin.z + j * biomeLength);
                biomeCenters.Add(biomeCenter);
            }
        }

        return new List<Vector3Int>(biomeCenters);
    }
}
