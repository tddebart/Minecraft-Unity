using System;
using System.Collections.Generic;
using UnityEngine;

public static class DataProcessing
{
    public static readonly List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), //N
        new Vector2Int(1, 1), //NE
        new Vector2Int(1, 0), //E
        new Vector2Int(1, -1), //SE
        new Vector2Int(0, -1), //S
        new Vector2Int(-1, -1), //SW
        new Vector2Int(-1, 0), //W
        new Vector2Int(-1, 1) //NW
    };
    
    public static HashSet<Vector2Int> FindLocalMaxima(float[,] noiseData, int chunkXCoord, int chunkZCoord)
    {
        var maximas = new HashSet<Vector2Int>();
        for (var x = 0; x < noiseData.GetLength(0); x++)
        {
            for (var y = 0; y < noiseData.GetLength(1); y++)
            {
                var noiseVal = noiseData[x, y];
                if (CheckNeighbours(noiseData, x, y, noiseVal))
                {
                    maximas.Add(new Vector2Int(x + chunkXCoord, y + chunkZCoord));
                }
            }
        }
        return maximas;
    }

    private static bool CheckNeighbours(float[,] noiseData, int x, int y, float noiseVal)
    {
        foreach (var dir in directions)
        {
            var newPos = new Vector2Int(x + dir.x, y + dir.y);
            
            // Check if the new position is within the bounds of the noiseData array
            if (newPos.x < 0 || newPos.x >= noiseData.GetLength(0) || newPos.y < 0 || newPos.y >= noiseData.GetLength(1))
                continue;
            
            // Check if the new position is a local maximum
            if (!(noiseData[newPos.x, newPos.y] > noiseVal))
                return false;
        }
        
        return true;
    }
}