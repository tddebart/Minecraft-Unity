using UnityEngine;

public class ChunkData
{
    public BlockType[] blocks;
    public int chunkSize = 16;
    public int chunkHeight = 100;
    public World worldRef;
    public Vector3Int worldPos;
    
    public bool modifiedByPlayer = false;
    
    public ChunkData(int chunkSize, int chunkHeight, World worldRef, Vector3Int worldPos)
    {
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.worldRef = worldRef;
        this.worldPos = worldPos;
        blocks = new BlockType[chunkSize * chunkSize * chunkHeight];
    }
}