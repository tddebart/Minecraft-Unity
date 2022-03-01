
using System;
using System.Collections.Generic;

public class ChunkSection
{
    public Block[,,] blocks = new Block[16, 16, 16];
    public int yOffset;
    public ChunkData chunkData;
    
}
