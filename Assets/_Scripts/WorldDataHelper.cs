using UnityEngine;

public static class WorldDataHelper
{
    public static Vector3Int GetChunkPosition(World world, Vector3Int worldSpacePos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldSpacePos.x / (float)world.chunkSize) * world.chunkSize,
            Mathf.FloorToInt(worldSpacePos.y / (float)world.chunkHeight) * world.chunkHeight,
            Mathf.FloorToInt(worldSpacePos.z / (float)world.chunkSize) * world.chunkSize
        );
    }
}
