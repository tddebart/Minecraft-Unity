using System;
using Cysharp.Threading.Tasks;
using UnityEngine;


public static class BlockHelper
{
    public static readonly Direction[] directions =
    {
        Direction.forwards,
        Direction.backwards,
        Direction.left,
        Direction.right,
        Direction.up,
        Direction.down,
    };
    
    public static readonly Direction[] revDirections =
    {
        Direction.backwards,
        Direction.forwards,
        Direction.right,
        Direction.left,
        Direction.down,
        Direction.up,
    };
    
    public static readonly Block NOTHING = new Block(BlockType.Nothing, Vector3Int.zero, null);
    
    
    //NOTE: block vertices start from bottom left and go clockwise

    public static MeshData GetMeshData(ChunkData chunk, Vector3Int pos, MeshData meshData, BlockType blockType)
    {
        if(blockType == BlockType.Air || blockType == BlockType.Nothing)
        {
            return meshData;
        }

        BlockTypeData blockTypeData = BlockDataManager.blockTypeDataDictionary[(int)blockType];
        Block block = chunk.GetBlock(pos);

        foreach (var dir in directions)
        {
            Vector3Int neighbourPos = pos + dir.GetVector();

            Block neighbourBlock = chunk.GetBlock(neighbourPos);
            BlockType neighbourBlockType = neighbourBlock.type;
            if (true/*neighbourBlockType != BlockType.Nothing*/)
            {
                BlockTypeData neighbourBlockTypeData = BlockDataManager.blockTypeDataDictionary[(int)neighbourBlockType];

                if (blockTypeData.isTransparent)
                {
                    if (blockType == BlockType.Water)
                    {
                        if (neighbourBlockType != BlockType.Water && neighbourBlockTypeData.isTransparent)
                        {
                            meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, block,blockTypeData, chunk);
                        }
                    }
                    else if (neighbourBlockTypeData.isTransparent)
                    {
                        meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, block,blockTypeData, chunk);
                    }
                }
                else if(neighbourBlockTypeData.isTransparent || !neighbourBlock.blockShape.GetShape().isFullBlock() && !neighbourBlock.blockShape.GetShape().isSideFull(dir.GetOpposite()))
                {
                    meshData = GetFaceDataIn(dir, pos, meshData, block,blockTypeData, chunk);
                } else if (!block.blockShape.GetShape().isFullBlock() && !block.blockShape.GetShape().isSideFull(dir))
                {
                    meshData = GetFaceDataIn(dir, pos, meshData, block,blockTypeData, chunk);
                }
            }
        }
        return meshData;
    }

    public static MeshData GetFaceDataIn(Direction dir, Vector3Int pos, MeshData meshData, Block block, BlockTypeData blockTypeData, ChunkData chunk)
    {
        block.blockShape.GetShape().SetFaceVertices(dir,pos,meshData);
        if (chunk != null)
        {
            GetFaceColors(dir,pos,meshData,chunk);
            GetVertexAOSides(dir,pos,meshData,chunk);
        }
        meshData.AddQuadTriangles();
        var uvs = FaceUVs(dir, block.type, blockTypeData);
        meshData.uv.AddRange(uvs);

        return meshData;
    }
    
    public static Vector2Int TexturePosition(Direction dir, BlockTypeData blockTypeData)
    {
        return dir switch
        {
            Direction.up => blockTypeData.textureData.up,
            Direction.down => blockTypeData.textureData.down,
            _ => blockTypeData.textureData.side
        };
    }

    public static Vector2 TextureExtends(Direction dir, BlockTypeData blockTypeData)
    {
        return dir switch
        {
            Direction.up => blockTypeData.textureData.upExtends,
            Direction.down => blockTypeData.textureData.downExtends,
            _ => blockTypeData.textureData.sideExtends
        };
    }

    public static void GetFaceColors(Direction direction, Vector3Int pos, MeshData meshData, ChunkData chunk)
    {
        var block = chunk.GetBlock(pos + direction.GetVector());

        float skyLightLevel = block.GetSkyLight();
        float blockLightLevel = block.GetBlockLight();

        meshData.skyLight.Add(skyLightLevel);
        meshData.skyLight.Add(skyLightLevel);
        meshData.skyLight.Add(skyLightLevel);
        meshData.skyLight.Add(skyLightLevel);
        
        meshData.blockLight.Add(blockLightLevel);
        meshData.blockLight.Add(blockLightLevel);
        meshData.blockLight.Add(blockLightLevel);
        meshData.blockLight.Add(blockLightLevel);
    }

    public static void GetVertexAOSides(Direction direction, Vector3Int pos, MeshData meshData, ChunkData chunk)
    {
        if (direction == Direction.up)
        {
            var forwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.forward).BlockData.opacity != 0);
            var backwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.back).BlockData.opacity != 0);
            var left = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.left).BlockData.opacity != 0);
            var right = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.right).BlockData.opacity != 0);
            
            meshData.sides.Add(new Vector3( // -+
                left,
                forwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.left + Vector3Int.forward).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                forwards,
                right,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.forward + Vector3Int.right).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // +-
                right,
                backwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.right + Vector3Int.back).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // --
                backwards,
                left,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.up + Vector3Int.back + Vector3Int.left).BlockData.opacity != 0)));

        }
        else if (direction == Direction.down)
        {
            // ---
            // +--
            // +-+
            // --+

            var backwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.back).BlockData.opacity != 0);
            var forwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.forward).BlockData.opacity != 0);
            var left = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.left).BlockData.opacity != 0);
            var right = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.right).BlockData.opacity != 0);

            meshData.sides.Add(new Vector3( // --
                backwards,
                left,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.back + Vector3Int.left).BlockData.opacity != 0)));

            meshData.sides.Add(new Vector3( // +-
                right,
                backwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.right + Vector3Int.back).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                forwards,
                right,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.forward + Vector3Int.right).BlockData.opacity != 0)));
            
            
            meshData.sides.Add(new Vector3( // -+
                left,
                forwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.down + Vector3Int.left + Vector3Int.forward).BlockData.opacity != 0)));
            
        }
        else if (direction == Direction.left)
        {
            // --+
            // -++
            // -+-
            // ---
            var down = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.down).BlockData.opacity != 0);
            var up = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.up).BlockData.opacity != 0);
            var forwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.forward).BlockData.opacity != 0);
            var backwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.back).BlockData.opacity != 0);

            meshData.sides.Add(new Vector3( // -+
                down,
                forwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.down + Vector3Int.forward).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                forwards,
                up,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.forward + Vector3Int.up).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // +-
                up,
                backwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.up + Vector3Int.back).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // --
                backwards,
                down,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.left + Vector3Int.back + Vector3Int.down).BlockData.opacity != 0)));
        }
        else if (direction == Direction.right)
        {
            // +--
            // ++-
            // +++
            // +-+
            var down = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.down).BlockData.opacity != 0);
            var up = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.up).BlockData.opacity != 0);
            var forwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.forward).BlockData.opacity != 0);
            var backwards = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.back).BlockData.opacity != 0);
            
            meshData.sides.Add(new Vector3( // --
                down,
                backwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.down + Vector3Int.back).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // +-
                backwards,
                up,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.back + Vector3Int.up).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                up,
                forwards,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.up + Vector3Int.forward).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // -+
                forwards,
                down,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.right + Vector3Int.forward + Vector3Int.down).BlockData.opacity != 0)));
            
        }
        else if (direction == Direction.backwards)
        {
            // ---
            // -+-
            // ++-
            // +--

            var down = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.down).BlockData.opacity != 0);
            var left = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.left).BlockData.opacity != 0);
            var up = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.up).BlockData.opacity != 0);
            var right = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.right).BlockData.opacity != 0);
            
            meshData.sides.Add(new Vector3( // --
                down,
                left,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.down + Vector3Int.left).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // -+
                left,
                up,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.left + Vector3Int.up).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                up,
                right,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.up + Vector3Int.right).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // +-
                right,
                down,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.back + Vector3Int.right + Vector3Int.down).BlockData.opacity != 0)));
        }
        else if (direction == Direction.forwards)
        {
            // +-+
            // +++
            // -++
            // --+

            var down = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.down).BlockData.opacity != 0);
            var left = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.left).BlockData.opacity != 0);
            var up = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.up).BlockData.opacity != 0);
            var right = Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.right).BlockData.opacity != 0);
            
            meshData.sides.Add(new Vector3( // +-
                down,
                right,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.down + Vector3Int.right).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // ++
                right,
                up,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.right + Vector3Int.up).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // -+
                up,
                left,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.up + Vector3Int.left).BlockData.opacity != 0)));
            
            meshData.sides.Add(new Vector3( // --
                left,
                down,
                Convert.ToSingle(chunk.GetBlock(pos + Vector3Int.forward + Vector3Int.left + Vector3Int.down).BlockData.opacity != 0)));
            
        }
        else
        {
            meshData.sides.Add(new Vector3(0, 0, 0));
            meshData.sides.Add(new Vector3(0, 0, 0));
            meshData.sides.Add(new Vector3(0, 0, 0));
            meshData.sides.Add(new Vector3(0, 0, 0));
        }
    }

    public static int GetLightIndex(Vector3Int pos, ChunkData chunk)
    {
        return pos.x + pos.y * chunk.chunkSize + pos.z * chunk.chunkSize * World.Instance.worldHeight;
    }

    public static Vector2[] FaceUVs(Direction dir, BlockType type, BlockTypeData blockTypeData = null)
    {
        Vector2[] UVs = new Vector2[4];
        blockTypeData ??= BlockDataManager.blockTypeDataDictionary[(int)type];
        var tilePos = TexturePosition(dir, blockTypeData);
        var tileExtends = TextureExtends(dir, blockTypeData);
        var tileSizeX = BlockDataManager.tileSizeX;
        var tileSizeY = BlockDataManager.tileSizeY;

        UVs[0] = new Vector2(tilePos.x * tileSizeX, tilePos.y * tileSizeY);
        UVs[1] = new Vector2(tilePos.x * tileSizeX, (tilePos.y + tileExtends.y) * tileSizeY);
        UVs[2] = new Vector2((tilePos.x + tileExtends.x) * tileSizeX, (tilePos.y + tileExtends.y) * tileSizeY);
        UVs[3] = new Vector2((tilePos.x + tileExtends.x) * tileSizeX, tilePos.y * tileSizeY);

        return UVs;
    }
}
