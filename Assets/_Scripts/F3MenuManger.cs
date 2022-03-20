using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class F3MenuManger : MonoBehaviour
{
    public GameObject f3Container;
    public GameObject f3TemplateText;
    public Dictionary<string, TextMeshProUGUI> f3Texts = new Dictionary<string, TextMeshProUGUI>();
    public Coroutine fpsCoroutine;

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.F3))
        {
            f3Container.SetActive(!f3Container.activeSelf);
        }
        
        if (GameManager.Instance.playerSpawned)
        {
            if (fpsCoroutine == null)
            {
                fpsCoroutine = StartCoroutine(UpdateFps());
            }
            
            var playerPosition = GameManager.Instance.localPlayer.transform.position;
            CreateF3Text("coords", "XYZ: " + Math.Round(playerPosition.x,3).ToString("N3").Replace(',', '.') + " / " + Math.Round(playerPosition.y,5).ToString("N5").Replace(',', '.') + " / " + Math.Round(playerPosition.z,3).ToString("N3").Replace(',', '.'));
            CreateF3Text("block", "Block: " + Mathf.FloorToInt(playerPosition.x) +" "+ Mathf.FloorToInt(playerPosition.y) +" " + Mathf.FloorToInt(playerPosition.z));
            CreateF3Text("chunk", "Chunk: " + Mathf.FloorToInt(playerPosition.x % 16) + " " + Mathf.FloorToInt(playerPosition.y % 16) + " " + Mathf.FloorToInt(playerPosition.z % 16) + " in " + Mathf.FloorToInt(playerPosition.x / 16) +" " + Mathf.FloorToInt(playerPosition.y / 16) +" "+ Mathf.FloorToInt(playerPosition.z / 16));

        }
    }
    
    public void CreateF3Text(string key, string value)
    {
        if (f3Texts.ContainsKey(key))
        {
            f3Texts[key].text = value;
        }
        else
        {
            var newText = Instantiate(f3TemplateText, f3Container.transform);
            newText.SetActive(true);
            newText.name = key;
            newText.GetComponentInChildren<TextMeshProUGUI>().text = value;
            f3Texts.Add(key, newText.GetComponentInChildren<TextMeshProUGUI>());
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