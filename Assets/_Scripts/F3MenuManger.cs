using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class F3MenuManger : MonoBehaviour
{
    public RectTransform f3Container;
    public GameObject f3TemplateText;
    public Dictionary<string, TextMeshProUGUI> f3Texts = new Dictionary<string, TextMeshProUGUI>();
    public Dictionary<string, ContentSizeFitter> f3Groups = new Dictionary<string, ContentSizeFitter>();
    public Coroutine fpsCoroutine;

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.F3))
        {
            if (!GameManager.Instance.localPlayer.f3KeyComboUsed)
            {
                f3Container.gameObject.SetActive(!f3Container.gameObject.activeSelf);
            }

            GameManager.Instance.localPlayer.f3KeyComboUsed = false;
        }
        
        if (GameManager.Instance.playerSpawned && f3Container.gameObject.activeSelf)
        {
            if (fpsCoroutine == null)
            {
                fpsCoroutine = StartCoroutine(UpdateFps());
            }
            
            // Left
            var playerPosition = GameManager.Instance.localPlayer.transform.position;
            CreateF3Text("coords", "XYZ: " + Math.Round(playerPosition.x,3).ToString("N3").Replace(',', '.') + " / " + Math.Round(playerPosition.y,5).ToString("N5").Replace(',', '.') + " / " + Math.Round(playerPosition.z,3).ToString("N3").Replace(',', '.'));
            CreateF3Text("block", "Block: " + Mathf.FloorToInt(playerPosition.x) +" "+ Mathf.FloorToInt(playerPosition.y) +" " + Mathf.FloorToInt(playerPosition.z));
            CreateF3Text("chunk", "Chunk: " + Mathf.FloorToInt(playerPosition.x % 16) + " " + Mathf.FloorToInt(playerPosition.y % 16) + " " + Mathf.FloorToInt(playerPosition.z % 16) + " in " + Mathf.FloorToInt(playerPosition.x / 16) +" " + Mathf.FloorToInt(playerPosition.y / 16) +" "+ Mathf.FloorToInt(playerPosition.z / 16));

            
            // Right
            // CreateF3Text("fps", "FPS: " + Mathf.RoundToInt(1 / Time.deltaTime));
            var targetedBlock = GameManager.Instance.localPlayer.TargetedBlock(20,out _);
            if (targetedBlock != null)
            {
                var blockPos = targetedBlock.section.dataRef.GetGlobalBlockCoords(targetedBlock.position);
                blockPos.y += targetedBlock.section.yOffset;
                CreateF3Text("Target", "Targeted Block: " + blockPos.x + ", " + blockPos.y + ", " + blockPos.z, false);
                CreateF3Text("BlockType", targetedBlock.type.ToString(), false);
            }
            else
            {
                CreateF3Text("Target", "",false);
                CreateF3Text("BlockType", "",false);
            }
            
            foreach (var sizeFitter in f3Groups.Values)
            {
                sizeFitter.enabled = false;
                sizeFitter.enabled = true;
            }
        }
    }

    public void CreateF3Text(string key, string value, bool left = true)
    {
        if (f3Texts.ContainsKey(key))
        {
            f3Texts[key].text = value;
        }
        else
        {
            var newText = Instantiate(f3TemplateText, left ? f3Container.transform.GetChild(0) : f3Container.transform.GetChild(1));
            newText.SetActive(true);
            newText.name = key;
            newText.GetComponentInChildren<TextMeshProUGUI>().text = value;
            f3Texts.Add(key, newText.GetComponentInChildren<TextMeshProUGUI>());
            f3Groups.Add(key, newText.GetComponentInChildren<ContentSizeFitter>());
        }
    }

    private IEnumerator UpdateFps()
    {
        while (true)
        {
            CreateF3Text("fps", (int)(1 / Time.unscaledDeltaTime) + " fps");
            yield return new WaitForSeconds(1);
        }
    }
}