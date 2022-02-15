using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
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
        this.ChunkData = data;
    }

    private void RenderMesh(MeshData meshData)
    {
        mesh.Clear();

        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.waterMesh.vertices).ToArray();
        
        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.waterMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.waterMesh.uv).ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh
        {
            vertices = meshData.colliderVertices.ToArray(),
            triangles = meshData.colliderTriangles.ToArray()
        };
        collisionMesh.RecalculateNormals();

        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        RenderMesh(Chunk.GetChunkMeshData(ChunkData));
    }
    
    public void UpdateChunk(MeshData meshData)
    {
        RenderMesh(meshData);
    }
    
    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        if (Application.isPlaying && ChunkData != null)
        {
            if (Selection.activeObject == gameObject)
            {
                Gizmos.color = new Color(0,1,0,0.4f);
            }
            else
            {
                Gizmos.color = new Color(1,0,0,0.4f);
            }
            
            Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.chunkHeight / 2f, ChunkData.chunkSize / 2f),
                new Vector3(ChunkData.chunkSize, ChunkData.chunkHeight, ChunkData.chunkSize));
        }
        
    }

#endif
}