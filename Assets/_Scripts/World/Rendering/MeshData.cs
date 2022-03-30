using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uv = new List<Vector2>();
    public List<Color> colors = new List<Color>();
    public List<float> skyLight = new List<float>();
    public List<float> blockLight = new List<float>();

    public int subMeshCount;

    public MeshData transparentMesh;

    public MeshData(bool isMainMesh)
    {
        if (isMainMesh)
        {
            transparentMesh = new MeshData(false);
        }
    }

    public void AddVertex(Vector3 vertices)
    {
        this.vertices.Add(vertices);
    }
    
    public void AddColor(Color color) 
    {
        this.colors.Add(color);
    }

    public void AddQuadTriangles()
    {
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 3); // vertex 1
        triangles.Add(vertices.Count - 2); // vertex 2
        
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 2); // vertex 2
        triangles.Add(vertices.Count - 1); // vertex 3
    }
}
