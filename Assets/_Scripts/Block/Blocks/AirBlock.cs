
using JetBrains.Annotations;
using UnityEngine;

public class AirBlock : Block
{
    public AirBlock(Vector3Int position, [CanBeNull] ChunkSection section = null) : base(BlockType.Air, position, section)
    {
    }

    public override void OnBlockPlaced()
    {
        
    }

    public override void OnBlockDestroyed()
    {
        
    }

    public override void OnBlockUpdate()
    {
        
    }
}
