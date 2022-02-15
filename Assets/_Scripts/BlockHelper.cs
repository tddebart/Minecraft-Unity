using UnityEngine;


public static class BlockHelper
{
    private static Direction[] directions =
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

        foreach (var dir in directions)
        {
            Vector3Int neighbourPos = pos + dir.GetVector();
            BlockType neighbourBlockType = Chunk.GetBlock(chunk, neighbourPos);
            
            if (neighbourBlockType != BlockType.Nothing && !BlockDataManager.textureDataDictionary[neighbourBlockType].isSolid)
            {
                if (blockType == BlockType.Water)
                {
                    if (neighbourBlockType == BlockType.Air)
                    {
                        meshData.waterMesh = GetFaceDataIn(dir, chunk, pos, meshData.waterMesh, blockType);
                    }
                }
                else
                {
                    meshData = GetFaceDataIn(dir, chunk, pos, meshData, blockType);
                }
            }
        }
        return meshData;
    }

    public static MeshData GetFaceDataIn(Direction dir, ChunkData chunk, Vector3Int pos, MeshData meshData, BlockType blockType)
    {
        GetFaceVertices(dir, pos, meshData, blockType);
        meshData.AddQuadTriangles(BlockDataManager.textureDataDictionary[blockType].generateCollider);
        meshData.uv.AddRange(FaceUVs(dir,blockType));
        
        return meshData;
    }
    
    public static Vector2Int TexturePosition(Direction dir, BlockType blockType)
    {
        return dir switch
        {
            Direction.up => BlockDataManager.textureDataDictionary[blockType].up,
            Direction.down => BlockDataManager.textureDataDictionary[blockType].down,
            _ => BlockDataManager.textureDataDictionary[blockType].side
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
    
    public static Vector2[] FaceUVs(Direction dir, BlockType blockType)
    {
        Vector2[] UVs = new Vector2[4];
        var tilePos = TexturePosition(dir, blockType);
        var tileSizeX = BlockDataManager.tileSizeX;
        var tileSizeY = BlockDataManager.tileSizeY;

        UVs[0] = new Vector2(tilePos.x * tileSizeX, tilePos.y * tileSizeY);
        UVs[1] = new Vector2(tilePos.x * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[2] = new Vector2((tilePos.x + 1) * tileSizeX, (tilePos.y + 1) * tileSizeY);
        UVs[3] = new Vector2((tilePos.x + 1) * tileSizeX, tilePos.y * tileSizeY);

        return UVs;
    }
}
