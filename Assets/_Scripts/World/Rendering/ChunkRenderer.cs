using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
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
        meshFilter.sharedMesh = new Mesh();
        mesh = meshFilter.sharedMesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        this.ChunkData = data;
    }

    private void RenderMesh(MeshData meshData)
    {
        //NOTE: using AddRange instead of concat and then ToArray is a lot faster. Try to avoid concat and ToArray/ToList if possible
        
        mesh.Clear();
        mesh.MarkDynamic();
        mesh.subMeshCount = 2;
        meshData.vertices.AddRange(meshData.transparentMesh.vertices);
        mesh.SetVertices(meshData.vertices);
        meshData.colors.AddRange(meshData.transparentMesh.colors);
        mesh.SetColors(meshData.colors);
        
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.SetTriangles(meshData.transparentMesh.triangles.Select(val => val + (meshData.vertices.Count-meshData.transparentMesh.vertices.Count)).ToList(), 1);
        meshData.uv.AddRange(meshData.transparentMesh.uv);
        mesh.SetUVs(0, meshData.uv);
        mesh.RecalculateNormals();
        mesh.Optimize();
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
            
            Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f-0.5f, ChunkData.worldRef.worldHeight / 2f-0.5f, ChunkData.chunkSize / 2f-0.5f),
                new Vector3(ChunkData.chunkSize, ChunkData.worldRef.worldHeight, ChunkData.chunkSize));
        }
        
    }

#endif
}