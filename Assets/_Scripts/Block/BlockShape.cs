
using System;
using UnityEngine;

public class BlockShape
{
    // min 0,0,0 and max 16,16,16 is the size of a full block
    public int xMin, yMin, zMin, xMax, yMax, zMax;
    
    public BlockShape(int xMin, int yMin, int zMin, int xMax,  int yMax,  int zMax)
    {
        this.xMin = xMin;
        this.yMin = yMin;
        this.zMin = zMin;
        this.xMax = xMax;
        this.yMax = yMax;
        this.zMax = zMax;
    }

    public void SetFaceVertices(Direction direction, Vector3Int pos, MeshData meshData)
    {
        //order of vertices matters for the normals and how we render the mesh
        // add vertices to the meshData
        
        var min = new Vector3(pos.x + xMin / 16f + 0.0005f, pos.y + yMin / 16f + 0.0005f, pos.z + zMin / 16f + 0.0005f);
        var max = new Vector3(pos.x + xMax / 16f + 0.0005f, pos.y + yMax / 16f + 0.0005f, pos.z + zMax / 16f + 0.0005f);

        //order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                meshData.AddVertex(min); // ---
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f ,pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f )); // -+-
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f )); // ++-
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f , pos.z + zMin/16f+0.0005f )); // +--
                break;
            case Direction.forwards:
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f )); // +-+
                meshData.AddVertex(max); // +++
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMax/16f+0.0005f )); // -++
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f )); // --+
                break;
            case Direction.left:
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // --+
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // -++
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // -+-
                meshData.AddVertex(min); // ---
                break;
            case Direction.right:
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // +--
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // ++-
                meshData.AddVertex(max); // +++
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // +-+
                break;
            case Direction.down:
                meshData.AddVertex(min); // ---
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // +--
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // +-+
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMin/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // --+
                break;
            case Direction.up:
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMax/16f+0.0005f)); // -++
                meshData.AddVertex(max); // +++
                meshData.AddVertex(new Vector3(pos.x + xMax/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // ++-
                meshData.AddVertex(new Vector3(pos.x + xMin/16f+0.0005f, pos.y + yMax/16f+0.0005f, pos.z + zMin/16f+0.0005f)); // -+-
                break;
        }
    }

    public bool isSideFull(Direction dir)
    {
        switch (dir)
        {
            case Direction.backwards:
                return zMin == 0 && yMin == 0 && yMax == 16;
            case Direction.forwards:
                return zMax == 16 && yMin == 0 && yMax == 16;
            case Direction.left:
                return xMin == 0 && yMin == 0 && yMax == 16;
            case Direction.right:
                return xMax == 16 && yMin == 0 && yMax == 16;
            case Direction.down:
                return yMin == 0;
            case Direction.up:
                return yMax == 16;
        }
        return false;
    }
    
    public bool isFullBlock()
    {
        return xMin == 0 && xMax == 16 && yMin == 0 && yMax == 16 && zMin == 0 && zMax == 16;
    }
}
