using Cysharp.Threading.Tasks;
using UnityEngine;


public static class BlockHelper
{
    public static Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.forwards,
        Direction.left,
        Direction.right,
        Direction.up
    };


    public static MeshData GetMeshData(ChunkData chunk, Vector3Int pos, MeshData meshData, BlockType blockType)
    {
        if(blockType == BlockType.Air || blockType == BlockType.Nothing)
        {
            return meshData;
        }
        TextureData textureData = BlockDataManager.textureDataDictionary[(int)blockType];

        foreach (var dir in directions)
        {
            Vector3Int neighbourPos = pos + dir.GetVector();

            BlockType neighbourBlockType = chunk.GetBlock(neighbourPos).type;
            if (neighbourBlockType != BlockType.Nothing)
            {
                TextureData neighbourTextureData = BlockDataManager.textureDataDictionary[(int)neighbourBlockType];

                if (textureData.isTransparent)
                {
                    if (blockType == BlockType.Water)
                    {
                        if (neighbourBlockType != BlockType.Water && neighbourTextureData.isTransparent)
                        {
                            meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, blockType,textureData, chunk);
                        }
                    }
                    else if (neighbourTextureData.isTransparent)
                    {
                        meshData.transparentMesh = GetFaceDataIn(dir, pos, meshData.transparentMesh, blockType,textureData, chunk);
                    }
                }
                else if(neighbourTextureData.isTransparent)
                {
                    meshData = GetFaceDataIn(dir, pos, meshData, blockType,textureData, chunk);
                }
            }
        }
        return meshData;
    }

    public static MeshData GetFaceDataIn(Direction dir, Vector3Int pos, MeshData meshData, BlockType blockType, TextureData textureData, ChunkData chunk)
    {
        GetFaceVertices(dir, pos, meshData);
        if (chunk != null)
        {
            GetFaceColors(dir,pos,meshData,chunk);
        }
        meshData.AddQuadTriangles();
        var uvs = FaceUVs(dir, blockType, textureData);
        meshData.uv.AddRange(uvs);

        return meshData;
    }
    
    public static Vector2Int TexturePosition(Direction dir, BlockType blockType, TextureData textureData)
    {
        return dir switch
        {
            Direction.up => textureData.up,
            Direction.down => textureData.down,
            _ => textureData.side
        };
    }

    public static void GetFaceVertices(Direction direction, Vector3Int pos, MeshData meshData)
    {
        //order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                break;
            case Direction.forwards:
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                break;
            case Direction.left:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                break;

            case Direction.right:
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                break;
            case Direction.down:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f));
                break;
            case Direction.up:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f));
                break;
        }
    }

    public static void GetFaceColors(Direction direction, Vector3Int pos, MeshData meshData, ChunkData chunk)
    {
        float lightLevel;
        
        var yPos = pos.y+1;
        var inShade = false;

        while (yPos < World.Instance.worldHeight)
        {
            if (chunk.GetBlock(new Vector3Int(pos.x, yPos, pos.z)).type != BlockType.Air)
            {
                inShade = true;
                break;
            }

            yPos++;
        }
        
        if (inShade)
        {
            lightLevel = 0.4f;
        }
        else
        {
            lightLevel = 0f;
        }
        
        meshData.AddColor(new Color(0,0,0,lightLevel));
        meshData.AddColor(new Color(0,0,0,lightLevel));
        meshData.AddColor(new Color(0,0,0,lightLevel));
        meshData.AddColor(new Color(0,0,0,lightLevel));
    }
    
    public static Vector2[] FaceUVs(Direction dir, BlockType blockType, TextureData textureData = null)
    {
        Vector2[] UVs = new Vector2[4];
        textureData ??= BlockDataManager.textureDataDictionary[(int)blockType];
        var tilePos = TexturePosition(dir, blockType, textureData);
        var tileSizeX = BlockDataManager.tileSizeX;
        var tileSizeY = BlockDataManager.tileSizeY;

        UVs[0] = new Vector2(tilePos.x * tileSizeX, tilePos.y * tileSizeY);
        UVs[1] = new Vector2(tilePos.x * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[2] = new Vector2((tilePos.x + 1) * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[3] = new Vector2((tilePos.x + 1) * tileSizeX, tilePos.y * tileSizeY);

        return UVs;
    }
}
