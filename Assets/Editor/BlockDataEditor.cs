using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[EditorWindowTitle(title="Block data editor")]
public class BlockDataEditor : EditorWindow
{
    public BlockDataManager blockDataManager;
    [FormerlySerializedAs("selectedTextureData")] public BlockTypeData selectedBlockTypeData;

    private PreviewRenderUtility previewRenderer;
    private Mesh cubeMesh;
    public Material previewMaterial;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Minecraft/BlockDataEditor")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(BlockDataEditor));
    }

    public void Initialize()
    {
        previewRenderer = new PreviewRenderUtility();
        previewRenderer.camera.fieldOfView = 30;
        previewRenderer.camera.nearClipPlane = 0.01f;
        previewRenderer.camera.farClipPlane = 100;
        previewRenderer.camera.transform.position = new Vector3(0, 3, -5);

        cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
    }

    private void OnEnable()
    {
        blockDataManager = FindObjectOfType<BlockDataManager>();
    }

    private void OnDisable()
    {
        previewRenderer.Cleanup();
    }

    private void OnGUI()
    {
        blockDataManager = (BlockDataManager)EditorGUILayout.ObjectField(blockDataManager, typeof(BlockDataManager), true);
        previewMaterial = (Material)EditorGUILayout.ObjectField(previewMaterial, typeof(Material), true);
        if (blockDataManager == null || previewMaterial == null)
        {
            return;
        }

        var blockDataList = blockDataManager.textureData.textureDataList;
        
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(100) });
        
        for (int i = 0; i < blockDataList.Count; i++)
        {
            var blockData = blockDataList[i];
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(blockData.blockType.ToString(), new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter }))
            {
                selectedBlockTypeData = blockData;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();

        if (selectedBlockTypeData != null)
        {
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Block type: ", GUILayout.Width(80));
            selectedBlockTypeData.blockType = (BlockType)EditorGUILayout.EnumPopup(selectedBlockTypeData.blockType);
            EditorGUILayout.EndHorizontal();
            
            
            if (previewRenderer == null)
            {
                Initialize();
            }
            
            
            var uvs = new List<Vector2>();
            foreach (Direction direction in BlockHelper.directions)
            {
                uvs.AddRange(BlockHelper.FaceUVs(direction, selectedBlockTypeData.blockType));
            }
            
            cubeMesh.uv = uvs.ToArray();
            
            previewRenderer.BeginPreview(new Rect(150, 100, 300, 300), "box");
            
            previewRenderer.camera.transform.LookAt(Vector3.zero);
            previewRenderer.camera.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * 1);
            
            previewRenderer.DrawMesh(cubeMesh, Vector3.zero, Quaternion.identity, previewMaterial , 0);
            previewRenderer.camera.Render();

            previewRenderer.EndAndDrawPreview(new Rect(150, 100, 300, 300));

            EditorGUILayout.EndVertical();
        
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void Update()
    {
        Repaint();
    }
}
