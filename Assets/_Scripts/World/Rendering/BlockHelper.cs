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

    public static MeshData GetMeshData(ChunkData chunk, Vector3Int pos, MeshData meshData, BlockType blockType)
    {
        if(blockType == BlockType.Air || blockType == BlockType.Nothing)
        {
            return meshData;
        }

        BlockTypeData blockTypeData = BlockDataManager.blockTypeDataDictionary[(int)blockType];

        foreach (var dir in directions)
        {
            Vector3Int neighbourPos = pos + dir.GetVector();

            BlockType neighbourBlockType = chunk.GetBlock(neighbourPos).type;
            if (true/*neighbourBlockType != BlockType.Nothing*/)
            {
                BlockTypeData neighbourBlockTypeData = BlockDataManager.blockTypeDataDictionary[(int)neighbourBlockType];

                if (blockTypeData.isTransparent)
                {
                    if (blockType == BlockType.Water)
                    {
                        if (neighbourBlockType != BlockType.Water && neighbourBlockTypeData.isTransparent)
                        {
                            meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, blockType,blockTypeData, chunk);
                        }
                    }
                    else if (neighbourBlockTypeData.isTransparent)
                    {
                        meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, blockType,blockTypeData, chunk);
                    }
                }
                else if(neighbourBlockTypeData.isTransparent)
                {
                    meshData = GetFaceDataIn(dir, pos, meshData, blockType,blockTypeData, chunk);
                }
            }
        }
        return meshData;
    }

    public static MeshData GetFaceDataIn(Direction dir, Vector3Int pos, MeshData meshData, BlockType type, BlockTypeData blockTypeData, ChunkData chunk)
    {
        GetFaceVertices(dir, pos, meshData);
        if (chunk != null)
        {
            GetFaceColors(dir,pos,meshData,chunk);
            GetVertexAOSides(dir,pos,meshData,chunk);
        }
        meshData.AddQuadTriangles();
        var uvs = FaceUVs(dir, type, blockTypeData);
        meshData.uv.AddRange(uvs);

        return meshData;
    }
    
    public static Vector2Int TexturePosition(Direction dir, BlockTypeData blockTypeData)
    {
        return dir switch
        {
            Direction.up => blockTypeData.up,
            Direction.down => blockTypeData.down,
            _ => blockTypeData.side
        };
    }

    public static void GetFaceVertices(Direction direction, Vector3Int pos, MeshData meshData)
    {
        //order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // ---
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // -+-
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // ++-
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // +--
                break;
            case Direction.forwards:
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // +-+
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // +++
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // -++
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // --+
                break;
            case Direction.left:
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // --+
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // -++
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // -+-
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // ---
                break;

            case Direction.right:
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // +--
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // ++-
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // +++
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // +-+
                break;
            case Direction.down:
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // --
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z - 0.50001f)); // +-
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // ++
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y - 0.50001f, pos.z + 0.50001f)); // -+
                break;
            case Direction.up:
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // -+
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z + 0.50001f)); // ++
                meshData.AddVertex(new Vector3(pos.x + 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // +-
                meshData.AddVertex(new Vector3(pos.x - 0.50001f, pos.y + 0.50001f, pos.z - 0.50001f)); // --
                break;
        }
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
        switch (direction)
        {
            case Direction.up:
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y+1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y+1, pos.z + 1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y+1, pos.z + 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y+1, pos.z+1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x+1, pos.y+1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x + 1, pos.y+1, pos.z + 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x+1, pos.y+1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y+1, pos.z-1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x + 1, pos.y+1, pos.z - 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y+1, pos.z-1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x-1, pos.y+1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y+1, pos.z - 1)).BlockData.opacity != 0)));
                break;
            case Direction.down:
                // ---
                // +--
                // +-+
                // --+
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y-1, pos.z-1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x-1, pos.y-1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y-1, pos.z - 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x+1, pos.y-1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y-1, pos.z-1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x + 1, pos.y-1, pos.z - 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y-1, pos.z+1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x+1, pos.y-1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x + 1, pos.y-1, pos.z + 1)).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y-1, pos.z)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x, pos.y-1, pos.z + 1)).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(new Vector3Int(pos.x - 1, pos.y-1, pos.z + 1)).BlockData.opacity != 0)));
                break;
            case Direction.left:
                // --+
                // -++
                // -+-
                // ---
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.down.GetVector() ).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock( pos + Direction.left.GetVector() + Direction.down.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.forwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.forwards.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.left.GetVector() + Direction.backwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0)));
                break;
            case Direction.right:
                // +--
                // ++-
                // +++
                // +-+
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.backwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0)));   
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.forwards.GetVector() + Direction.backwards.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.forwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.right.GetVector() + Direction.down.GetVector() ).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock( pos + Direction.right.GetVector() + Direction.down.GetVector() + Direction.forwards.GetVector()).BlockData.opacity != 0)));
                break;
            
            case Direction.backwards:
                // ---
                // -+-
                // ++-
                // +--
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.left.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.up.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.right.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.backwards.GetVector() + Direction.down.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0)));
                break;
            
            case Direction.forwards:
                // +-+
                // +++
                // -++
                // --+
                meshData.sides.Add(new Vector3(// +-
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.right.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// ++
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.up.GetVector() + Direction.right.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// -+
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.left.GetVector() + Direction.up.GetVector()).BlockData.opacity != 0)));
                meshData.sides.Add(new Vector3(// --
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.down.GetVector()).BlockData.opacity != 0),
                    Convert.ToSingle(chunk.GetBlock(pos + Direction.forwards.GetVector() + Direction.down.GetVector() + Direction.left.GetVector()).BlockData.opacity != 0)));
                break;
            default:
                meshData.sides.Add(new Vector3(0,0,0));
                meshData.sides.Add(new Vector3(0,0,0));
                meshData.sides.Add(new Vector3(0,0,0));
                meshData.sides.Add(new Vector3(0,0,0));
                break;
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
        var tileSizeX = BlockDataManager.tileSizeX;
        var tileSizeY = BlockDataManager.tileSizeY;

        UVs[0] = new Vector2(tilePos.x * tileSizeX, tilePos.y * tileSizeY);
        UVs[1] = new Vector2(tilePos.x * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[2] = new Vector2((tilePos.x + 1) * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[3] = new Vector2((tilePos.x + 1) * tileSizeX, tilePos.y * tileSizeY);

        return UVs;
    }
}
