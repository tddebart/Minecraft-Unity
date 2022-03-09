using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uv = new List<Vector2>();
    
    public List<Vector3> colliderVertices = new List<Vector3>();
    public List<int> colliderTriangles = new List<int>();

    public int subMeshCount;

    public MeshData transparentMesh;

    public MeshData(bool isMainMesh)
    {
        if (isMainMesh)
        {
            transparentMesh = new MeshData(false);
        }
    }

    public void AddVertex(Vector3 vertices, bool generateCollider)
    {
        this.vertices.Add(vertices);

        if (generateCollider)
        {
            colliderVertices.Add(vertices);
        }
    }

    public void AddQuadTriangles(bool generateCollider)
    {
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 3); // vertex 1
        triangles.Add(vertices.Count - 2); // vertex 2
        
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 2); // vertex 2
        triangles.Add(vertices.Count - 1); // vertex 3

        //NOTE: enable colliders when needed
        if (generateCollider)
        {
            colliderTriangles.Add(colliderVertices.Count - 4);
            colliderTriangles.Add(colliderVertices.Count - 3);
            colliderTriangles.Add(colliderVertices.Count - 2);
            
            colliderTriangles.Add(colliderVertices.Count - 4);
            colliderTriangles.Add(colliderVertices.Count - 2);
            colliderTriangles.Add(colliderVertices.Count - 1);
        }
    }
}
