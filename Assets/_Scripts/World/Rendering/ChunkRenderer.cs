using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshCollider meshCollider2;
    private Mesh mesh;
    public bool showGizmo = false;
    
    public ChunkData ChunkData { get; private set; }

    public bool ModifiedByPlayer
    {
        get => ChunkData.modifiedByPlayer;
        set => ChunkData.modifiedByPlayer = value;
    }

    public void Initialize(ChunkData data)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.sharedMesh = new Mesh();
        mesh = meshFilter.sharedMesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        this.ChunkData = data;
    }

    private void RenderMesh(MeshData meshData)
    {
        mesh.Clear();
        mesh.MarkDynamic();
        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.transparentMesh.vertices).ToArray();
        
        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.transparentMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.transparentMesh.uv).ToArray();
        mesh.RecalculateNormals();
        mesh.Optimize();

        meshCollider.sharedMesh = null;
        var collisionMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        collisionMesh.SetVertices(meshData.colliderVertices.Concat(meshData.transparentMesh.colliderVertices).ToArray());
        collisionMesh.SetTriangles(meshData.colliderTriangles.Concat(meshData.transparentMesh.colliderTriangles).ToArray(), 0);

        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        var data = ChunkData.GetMeshData();
        RenderMesh(data);
    }

    public void UpdateChunk(MeshData meshData)
    {
        RenderMesh(meshData);
    }
    
    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        if (ChunkData != null)
        {
            if (Selection.activeObject == gameObject)
            {
                Gizmos.color = new Color(0,1,0,0.4f);
            }
            else
            {
                Gizmos.color = new Color(1,0,0,0.4f);
            }
            
            Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.worldRef.worldHeight / 2f, ChunkData.chunkSize / 2f),
                new Vector3(ChunkData.chunkSize, ChunkData.worldRef.worldHeight, ChunkData.chunkSize));
        }
        
    }

#endif
}