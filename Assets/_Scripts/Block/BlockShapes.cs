
public static class BlockShapesList
{
    public static BlockShape FullBlock = new(0, 0, 0, 16, 16, 16);
    public static BlockShape Slab = new(0, 0, 0, 16, 8, 16);
}

public enum BlockShapes
{
    FullBlock,
    Slab
}

public static class BlockShapesExtensions
{
    public static BlockShape GetShape(this BlockShapes shape)
    {
        switch (shape)
        {
            case BlockShapes.FullBlock:
                return BlockShapesList.FullBlock;
            case BlockShapes.Slab:
                return BlockShapesList.Slab;
            default:
                return null;
        }
    }
}
