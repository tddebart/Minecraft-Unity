using UnityEngine;

public static class Chunk
{
    public static Vector3Int ChunkPosFromBlockCoords(World world, Vector3Int pos)
    {
        Vector3Int chunkPos = new Vector3Int(
            Mathf.FloorToInt(pos.x / (float)world.chunkSize) * world.chunkSize,
            0,
            Mathf.FloorToInt(pos.z / (float)world.chunkSize) * world.chunkSize
        );
        return chunkPos;
    }
}
