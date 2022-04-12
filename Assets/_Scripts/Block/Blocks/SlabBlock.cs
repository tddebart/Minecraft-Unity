using System;
using UnityEngine;

[Serializable]
public class SlabBlock : Block
{
    public string test;
    public SlabBlock(Block block) : base(block)
    {
        blockShape = BlockShapes.Slab;
    }
}
