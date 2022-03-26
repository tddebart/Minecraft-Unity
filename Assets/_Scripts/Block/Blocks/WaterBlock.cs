
using JetBrains.Annotations;
using UnityEngine;

public class WaterBlock : Block
{
    public static Direction[] Directions = { Direction.forwards, Direction.backwards, Direction.left, Direction.right, Direction.down };

    public WaterBlock(Vector3Int position, ChunkSection section) : base(BlockType.Water, position,
        section)
    {

    }

    public WaterBlock(Block block) : base(block)
    {
    }

    public override void OnBlockUpdate()
    {
        base.OnBlockUpdate();
        foreach (var dir in Directions)
        {
            if(section.dataRef.GetBlock(position + dir.GetVector()).type == BlockType.Air)
            {
                // section.dataRef.SetBlock(position + dir.GetVector(), Blocks.WATER, true);
            }
        }
    }
}
