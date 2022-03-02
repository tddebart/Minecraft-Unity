
using System;
using System.Collections.Generic;

public class ChunkSection
{
    public Block[,,] blocks;
    public int yOffset;
    public ChunkData dataRef;

    public ChunkSection(ChunkData dataRef, int yOffset)
    {
        this.dataRef = dataRef;
        this.yOffset = yOffset;
        blocks = new Block[dataRef.chunkSize, dataRef.chunkSize, dataRef.chunkSize];
    }
}
