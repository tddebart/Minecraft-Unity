using System;
using UnityEngine;

public static class DirectionExtensions
{
    public static Vector3Int GetVector(this Direction dir)
    {
        return dir switch
        {
            Direction.up => Vector3Int.up,
            Direction.down => Vector3Int.down,
            Direction.left => Vector3Int.left,
            Direction.right => Vector3Int.right,
            Direction.forwards => Vector3Int.forward,
            Direction.backwards => Vector3Int.back,
            _ => throw new Exception("Invalid direction"),
        };
    }
}
