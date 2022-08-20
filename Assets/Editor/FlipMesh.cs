using System.Linq;
using UnityEditor;
using UnityEngine;

public class FlipMesh : MonoBehaviour
{
    public void Flip()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
    }
}

[CustomEditor( typeof( FlipMesh ) )]
public class FlipMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if ( GUILayout.Button( "Flip" ) )
        {
            FlipMesh flipMesh = target as FlipMesh;
            flipMesh.Flip();
        }
    }
}
