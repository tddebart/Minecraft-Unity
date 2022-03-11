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
        TextureData textureData = BlockDataManager.textureDataDictionary[blockType];

        foreach (var dir in directions)
        {
            Vector3Int neighbourPos = pos + dir.GetVector();

            BlockType neighbourBlockType = chunk.GetBlock(neighbourPos).type;
            if (neighbourBlockType == BlockType.Nothing && neighbourPos.y > 0)
            {
                var p = 0;
            }
            if (neighbourBlockType != BlockType.Nothing)
            {
                TextureData neighbourTextureData = BlockDataManager.textureDataDictionary[neighbourBlockType];

                if (textureData.isTransparent)
                {
                    if (blockType == BlockType.Water)
                    {
                        if (neighbourBlockType != BlockType.Water && neighbourTextureData.isTransparent)
                        {
                            meshData.transparentMesh = GetFaceDataIn(dir, chunk, pos, meshData.transparentMesh, blockType,textureData);
                        }
                    }
                    else if (neighbourTextureData.isTransparent)
                    {
                        meshData.transparentMesh = GetFaceDataIn(dir, chunk, pos, meshData.transparentMesh, blockType,textureData);
                    }
                }
                else if(neighbourTextureData.isTransparent)
                {
                    meshData = GetFaceDataIn(dir, chunk, pos, meshData, blockType,textureData);
                }
            }
        }
        return meshData;
    }

    public static MeshData GetFaceDataIn(Direction dir, ChunkData chunk, Vector3Int pos, MeshData meshData, BlockType blockType, TextureData textureData)
    {
        GetFaceVertices(dir, pos, meshData, blockType);
        meshData.AddQuadTriangles(textureData.generateCollider);
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

    public static void GetFaceVertices(Direction direction, Vector3Int pos, MeshData meshData, BlockType blockType)
    {
        var generatesCollider = BlockDataManager.textureDataDictionary[blockType].generateCollider;
        //order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                break;
            case Direction.forwards:
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                break;
            case Direction.left:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                break;

            case Direction.right:
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                break;
            case Direction.down:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y - 0.5f, pos.z + 0.5f), generatesCollider);
                break;
            case Direction.up:
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                meshData.AddVertex(new Vector3(pos.x - 0.5f, pos.y + 0.5f, pos.z - 0.5f), generatesCollider);
                break;
        }
    }
    
    public static Vector2[] FaceUVs(Direction dir, BlockType blockType, TextureData textureData = null)
    {
        Vector2[] UVs = new Vector2[4];
        textureData ??= BlockDataManager.textureDataDictionary[blockType];
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
