
using UnityEngine;

public class Block
{
    protected Vector3Int position;
    protected BlockType type;
    protected bool isSolid;
    protected bool isTransparent;

    public Block(BlockType type, bool isSolid, bool isTransparent, Vector3Int position)
    {
        this.type = type;
        this.isSolid = isSolid;
        this.isTransparent = isTransparent;
        this.position = position;
    }
}
