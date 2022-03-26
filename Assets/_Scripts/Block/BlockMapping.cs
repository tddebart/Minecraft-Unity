
public static class BlockMapping
{
    public static Block MapTypeToBlock(BlockType type, Block block)
    {
        return type switch
        {
            BlockType.Water => new WaterBlock(block),
            _ => block
        };
    }
}
