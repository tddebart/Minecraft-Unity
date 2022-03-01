using UnityEngine;

public class ChunkData
{
    public BlockType[] blocks;
    public int chunkSize = 16;
    public int chunkHeight = 16;
    public World worldRef;
    public Vector3Int worldPos;
    public ChunkSection[] sections = new ChunkSection[16];
    
    public bool modifiedByPlayer = false;
    public TreeData treeData;

    public ChunkData(int chunkSize, int chunkHeight, World worldRef, Vector3Int worldPos)
    {
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.worldRef = worldRef;
        this.worldPos = worldPos;
        blocks = new BlockType[chunkSize * chunkSize * chunkHeight];
    }
}