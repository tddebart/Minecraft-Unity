
using JetBrains.Annotations;
using UnityEngine;

public class Block
{
    public Vector3Int position;
    public BlockType type;
    public ChunkSection section;

    public Block(BlockType type, Vector3Int position, [CanBeNull] ChunkSection section = null)
    {
        this.type = type;
        this.position = position;
        this.section = section;
    }
}
